using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using KefirTest.Core;

namespace KefirTest.Game
{
    //Спаунер сущностей. Отрабатывает логику спауна и деспауна сущностей.
    //Для периодического спауна используются параметры спауна из соответствующихй ScriptableObject'ов. Сущности без периодического спауна спаунятся в ответ на соответствующие ивенты.
    public class SpaceEntitiesSpawner : ISpawningPlayer, IUsedPool
    {
        private IManager manager;

        //Так как осколки астероидов и снаряды основого и второстепенного орудия спаунятся только в ответ на ивенты, то для них не заводится отдельный класс, хранящий параметры спауна.
        //Для них хранятся только параметры самих сущностей и индекс пула объектов, который для них выделен.
        private SpaceEntityParameters shardEntityParameters;
        private int shardPoolIndex;

        private List<BulletContainer> bulletContainers;

        private WeaponParameters playerPrimaryWeaponParameters;
        private int primaryProjectilePoolIndex;

        private WeaponParameters playerSecondaryWeaponParameters;
        private int secondaryProjectilePoolIndex;

        //Для сущностей которые спаунятся с определенной периодичностью создаются отдельные спаунеры, в которых хранятся все параметры и время до спауна.
        private List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters;
        private List<Spawner> spawners;

        private Vector2 cameraWorldSize;

        //Ивенты, на которые подписывается пул объектов
        public event IUsedPool.CreatePoolHandle CreatePoolEvent;
        public event IUsedPool.PullObjectHandle PullObjectEvent;
        public event IUsedPool.PushObjectHandle PushObjectEvent;

        //Ивенты на которые подписываются контроллеры. В частности ивент спауна игрока, на который подписывается только контроллер, который обрабатывает логику игрока.
        public event ISpawning.SpawnSpaceEntityHandle SpawnSpaceEntityEvent;
        public event ISpawningPlayer.SpawnPlayerSpaceEntityHandle SpawnPlayerSpaceEntityEvent;

        public SpaceEntitiesSpawner(
            IManager manager,
            WeaponParameters playerPrimaryWeaponParameters,
            WeaponParameters playerSecondaryWeaponParameters,
            SpaceEntityParameters shardEntityParameters,
            List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters
            )
        {
            this.manager = manager;
            this.playerPrimaryWeaponParameters = playerPrimaryWeaponParameters;
            this.playerSecondaryWeaponParameters= playerSecondaryWeaponParameters;
            this.shardEntityParameters = shardEntityParameters;
            this.spaceEntitySpawnParameters = spaceEntitySpawnParameters;
        }
        public void Initialize(
            List<IDespawning> despawners,
            SpaceEntityParameters playerEntityParameters,
            PlayerInput playerInput,
            float boundsEdge)
        {
            //Расчет размеров камеры чтобы потом использовать их для спауна астероидов и врагов по ее краям.
            cameraWorldSize = new Vector2(2 * Camera.main.orthographicSize * Camera.main.aspect * (1f + boundsEdge), 2 * Camera.main.orthographicSize * (1f + boundsEdge));

            //Спаун игрока.
            GameObject player = GameObject.Instantiate(playerEntityParameters.spaceEntityPrefab, Vector3.zero, Quaternion.identity, manager.managerMonoBehaviour.transform);
            player.name = "Player";
            SpawnPlayerSpaceEntityEvent?.Invoke(player, playerEntityParameters, playerInput, playerPrimaryWeaponParameters, playerSecondaryWeaponParameters);

            //Создание списков классов, в которых хранится информация для спауна. Для полноценных спаунов через промежутки времени - спаунеры, для снарядов орудий отдельный классы,
            //так как для них не надо хранить и обрабатывать время, но надо хранить индекс пула. Для осколков астероидов просто индекс пула, так как они спаунятся без конкуретнов.
            //Если вместе с осколками будет спауниться еще какая-то сущность, то нужно будет так же заводить отдельный класс.
            spawners = new List<Spawner>();
            bulletContainers = new List<BulletContainer>();
            shardPoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + shardEntityParameters.spaceEntityPrefab.name, shardEntityParameters.spaceEntityPrefab, 1);

            primaryProjectilePoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + playerPrimaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab.name, playerPrimaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab, 1);
            bulletContainers.Add(new BulletContainer(primaryProjectilePoolIndex, new List<GameObject>()));

            secondaryProjectilePoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + playerSecondaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab.name, playerSecondaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab, 1);
            bulletContainers.Add(new BulletContainer(secondaryProjectilePoolIndex, new List<GameObject>()));

            //Создание полноценного спаунера для каждого ScriptableObject с параметрами спауна, который получен от менеджера. Так же спаунер сразу подключается к собственному пулу объектов.
            //Если добавить еще какие-нибудь сущности с полноценным спауном, то для них так же автоматически создастся спаунер и пул объектов. 
            foreach (SpaceEntitySpawnParameters sp in spaceEntitySpawnParameters)
            {
                int poolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + sp.spaceEntityParameters.spaceEntityPrefab.name, sp.spaceEntityParameters.spaceEntityPrefab, 1);
                spawners.Add(new Spawner(sp, sp.spaceEntityParameters, sp.startSpawn, poolIndex, new List<GameObject>()));
            }
            
            //Подпись на ивенты контроллеров.
            foreach (IDespawning despawner in despawners)
            {
                despawner.DespawnSpaceEntityEvent += OnDespawnSpaceEntityEvent;
                despawner.SpawnPrimaryProjectileEvent += OnSpawnPrimaryProjectileEvent;
                despawner.SpawnSecondaryProjectileEvent += OnSpawnSecondaryProjectileEvent;
            }

            //Так как для запуска корутин нужен монобех, то для этого используется монобех менеджера.
            manager.managerMonoBehaviour.StartCoroutine(spawnCoroutine());
        }

        //Спаун снарядов основного и второстепенного орудия в ответ на соответствующие ивенты.
        private void OnSpawnPrimaryProjectileEvent(Transform playerTransform)
        {
            GameObject instance = PullObjectEvent?.Invoke(primaryProjectilePoolIndex);
            instance.transform.position = playerTransform.position + playerTransform.up * 1.5f;
            instance.transform.rotation = playerTransform.rotation;
            bulletContainers[0].spawnedSpaceEntities.Add(instance);
            SpawnSpaceEntityEvent(instance, playerPrimaryWeaponParameters.spaceEntityParameters, playerPrimaryWeaponParameters.spaceEntityParameters.SpaceEntityType);
        }

        private void OnSpawnSecondaryProjectileEvent(Transform playerTransform)
        {
            GameObject instance = PullObjectEvent?.Invoke(secondaryProjectilePoolIndex);
            instance.transform.position = playerTransform.position + playerTransform.up * 1.5f;
            instance.transform.rotation = playerTransform.rotation;
            bulletContainers[1].spawnedSpaceEntities.Add(instance);
            SpawnSpaceEntityEvent(instance, playerSecondaryWeaponParameters.spaceEntityParameters, playerSecondaryWeaponParameters.spaceEntityParameters.SpaceEntityType);
        }

        //Корутина спауна. Перебирает все созданные спаунеры и изменяет таймер в них на Time.DeltaTime. Если при этом достигнуто заданное значение времени, то из соответствующего пула
        //берется объект и отправляется контроллеру.
        private IEnumerator spawnCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i].timer <= 0)
                    {
                        //Тестовый класс босса спаунится моментально, но присутствует в единственном экземпляре.
                        if (!(spawners[i].spaceEntitySpawnParameters.isSingle && spawners[i].spawnedSpaceEntities.Count > 0))
                        {
                            (Vector3, float) positionRotation = GetRandomSpawnPositionRotation();
                            //Получение объекта из пулла. Для босса, существующего в единственном экземпляре по идее можно было и не заводить пул.
                            GameObject instance = PullObjectEvent?.Invoke(spawners[i].poolIndex);
                            instance.transform.position = positionRotation.Item1;
                            instance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, positionRotation.Item2));

                            //Если у сущности в иерархии есть спрайт, то вращаем его на случайный угол, чтобы астероиды и их осколки имели спрайт не связанный с их направлением движения.
                            if (instance.transform.childCount > 0)
                            {
                                instance.transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-180, 180)));
                            }
                            spawners[i].spawnedSpaceEntities.Add(instance);

                            //Если заспауненная сущность противник или босс, то используем комонент ParentConstraint, чтобы их спрайт не вращался вместе со сменой направления.
                            if (spawners[i].spaceEntitySpawnParameters.spaceEntityType == IManager.SpaceEntityType.Enemy)
                            {
                                ConstraintSource source = new ConstraintSource();
                                source.sourceTransform = manager.managerMonoBehaviour.transform;
                                source.weight = 1;
                                ParentConstraint parentConstraint = instance.GetComponentInChildren<ParentConstraint>();
                                parentConstraint.AddSource(source);
                            }
                            SpawnSpaceEntityEvent?.Invoke(instance, spawners[i].spaceEntityParameters, spawners[i].spaceEntitySpawnParameters.spaceEntityType);
                            spawners[i].timer = spawners[i].spaceEntitySpawnParameters.spawnFrequency;
                        }
                    }
                    spawners[i].timer -= Time.deltaTime;
                }
                yield return null;
            }
        }

        //Если какой-либо из контроллеров, на который подписан этот спаунер, вызывает ивент деспауна.
        private void OnDespawnSpaceEntityEvent(GameObject spaceEntity, IManager.SpaceEntityType spaceEntityType, bool isDestroyed)
        {
            //Если тип сущности снаряд, то смотрим в контейнерах снарядов.
            if (spaceEntityType == IManager.SpaceEntityType.Bullet)
            {
                int pullIndex = -1;
                foreach (BulletContainer bulletContainer in bulletContainers)
                {
                    if (bulletContainer.spawnedSpaceEntities.Contains(spaceEntity))
                    {
                        pullIndex = bulletContainer.poolIndex;
                    }
                }
                PushObjectEvent?.Invoke(pullIndex, spaceEntity);
            }
            else
            {
                //Если это не снаряд, то смотрим в полноценных спаунерах.
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i].spaceEntitySpawnParameters.spaceEntityType == spaceEntityType)
                    {
                        if (spawners[i].spawnedSpaceEntities.Contains(spaceEntity))
                        {
                            //Если это был астероид, то спавним вместо него три осколка.
                            if (spaceEntityType == IManager.SpaceEntityType.Asteroid && isDestroyed)
                            {
                                SpawnShard(spaceEntity);
                                SpawnShard(spaceEntity);
                                SpawnShard(spaceEntity);
                            }
                            //И возвращаем в пул объект для дальнейшего использования.
                            int pullIndex = spawners[i].poolIndex;
                            spawners[i].spawnedSpaceEntities.Remove(spaceEntity);
                            PushObjectEvent?.Invoke(pullIndex, spaceEntity);
                        }
                    }
                }
            }
        }

        //Функция спауна осколка.
        private GameObject SpawnShard(GameObject spaceEntity)
        {
            GameObject instance = PullObjectEvent?.Invoke(shardPoolIndex);
            instance.transform.position = spaceEntity.transform.position;
            instance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-180, 180)));
            if (instance.transform.childCount > 0)
            {
                instance.transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-180, 180)));
            }
            SpawnSpaceEntityEvent?.Invoke(instance, shardEntityParameters, IManager.SpaceEntityType.Asteroid);
            return instance;
        }

        //Получение случайной стороны и направления. В зависимости от того на какой стороне экрана заспаунилась сущность, она поворачивается так, чтобы лететь через игровое поле.
        private (Vector3, float) GetRandomSpawnPositionRotation()
        {
            Vector2 position = Vector2.zero;
            float rotation = 0;
            int spawnSide = Random.Range(0, 4);
            switch (spawnSide)
            {
                case 0:
                    //Top
                    position = new Vector2(Random.Range(-cameraWorldSize.x / 2, cameraWorldSize.x / 2), (cameraWorldSize.y / 2));
                    rotation = Random.Range(-90, 90);
                    rotation = rotation < 0 ? -180f - rotation : 180f - rotation;
                    break;
                case 1:
                    //Right
                    position = new Vector2((cameraWorldSize.x / 2), Random.Range(-cameraWorldSize.y / 2, cameraWorldSize.y / 2));
                    rotation = Random.Range(0, 180);
                    break;
                case 2:
                    //Bottom
                    position = new Vector2(Random.Range(-cameraWorldSize.x / 2, cameraWorldSize.x / 2), (-cameraWorldSize.y / 2));
                    rotation = Random.Range(-90, 90);
                    break;
                case 3:
                    //Left
                    position = new Vector2((-cameraWorldSize.x / 2), Random.Range(-cameraWorldSize.y / 2, cameraWorldSize.y / 2));
                    rotation = Random.Range(-180, 0);
                    break;
            }
            return (position, rotation);
        }

        //Класс в котором хранятся параметры полноценных спаунеров, которые обрабатывают время.
        public class Spawner
        {
            public SpaceEntitySpawnParameters spaceEntitySpawnParameters;
            public SpaceEntityParameters spaceEntityParameters;
            public float timer;
            public int poolIndex;
            public List<GameObject> spawnedSpaceEntities;
            public Spawner(
                SpaceEntitySpawnParameters spaceEntitySpawnParameters,
                SpaceEntityParameters spaceEntityParameters,
                float timer,
                int poolIndex,
                List<GameObject> spawnedSpaceEntities)
            {
                this.spaceEntitySpawnParameters = spaceEntitySpawnParameters;
                this.spaceEntityParameters = spaceEntityParameters;
                this.timer = timer;
                this.poolIndex = poolIndex;
                this.spawnedSpaceEntities = spawnedSpaceEntities;
            }
        }

        //Класс, в котором хранятся снаряды. Так как они относятся к одному типу, то при диспауне надо находить - кого именно мы деспауним и в какой пул.
        public class BulletContainer
        {
            public int poolIndex;
            public List<GameObject> spawnedSpaceEntities;
            public BulletContainer  (int poolIndex, List<GameObject> spawnedSpaceEntities)
            { 
                this.poolIndex = poolIndex; 
                this.spawnedSpaceEntities = spawnedSpaceEntities;
            }
        }
    }
}