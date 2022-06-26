using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KefirTest.Core
{
    //Контроллер сущностей. Обрабатывает логику сущностей, созданных спаунерами и возвращает их им, когда они уничтожены/вылетели за пределы экрана.
    //В итоге можно разделить его на несколько более специализированных контроллеров, например контроллер игрока, контроллер астероидов и врагов.
    public class SpaceEntitiesController : IDespawning
    {
        private IManager manager;
        private List<SpaceEntity> spaceEntities = new List<SpaceEntity>();
        private Player playerSpaceEntity;
        //Проверка вылета сущностей за пределы экрана делается при помощи Bounds. Возможно это слишком затратно, тогда можно просто проверять значения координат.
        private Bounds bounds;

        //Ивенты на которые подписывается класс интерфейса.
        public delegate void UpdatePlayerInformationHandle(Vector3 position, float rotation, float speed, int secondaryWeaponAmmoCount, float secondaryWeaponAmmoRestoreTime);
        public UpdatePlayerInformationHandle UpdatePlayerInformationEvent;

        public delegate void PlayerDestroyHandle(int score);
        public PlayerDestroyHandle PlayerDestroyEvent;

        public SpaceEntitiesController(IManager manager)
        {
            this.manager = manager;
        }

        public event IDespawning.DespawnSpaceEntityHandle DespawnSpaceEntityEvent;
        public event IDespawning.SpawnPrimaryProjectileHandle SpawnPrimaryProjectileEvent;
        public event IDespawning.SpawnSecondaryProjectileHandle SpawnSecondaryProjectileEvent;

        public void Initialize(List<ISpawning> spawners, float boundsEdge)
        {
            //Граница за которой сущности деспаунятся меняется в зависимости от разрешения в котором запущена игра.
            Vector2 cameraWorldSize = new Vector2(2 * Camera.main.orthographicSize * Camera.main.aspect, 2 * Camera.main.orthographicSize);
            bounds = new Bounds(Vector3.zero, new Vector3(cameraWorldSize.x * (1f + boundsEdge), cameraWorldSize.y * (1f + boundsEdge), 1));

            //Подпись на ивенты спаунеров, независимо от их количества.
            foreach (ISpawning spawner in spawners)
            {
                spawner.SpawnSpaceEntityEvent += OnSpawnSpaceEntityEvent;
                if (spawner is ISpawningPlayer)
                {
                    //Один из спаунеров спаунит игрока, подпись на этот ивент, так как данный контроллер обрабатывает логику игрока.
                    ISpawningPlayer playerSpawner = (ISpawningPlayer)spawner;
                    playerSpawner.SpawnPlayerSpaceEntityEvent += OnSpawnPlayerSpaceEntityEvent;
                }
            }
            //Подпись на Update менеджера, чтобы синхронизировать с ним логику.
            manager.UpdateEvent += OnUpdateEvent;
        }

        //Метод, передающий сущности игрока все нужные зависимости и параметры. Можно сделать покомпактнее, упаковав параметры орудий в отдельный класс.
        private void OnSpawnPlayerSpaceEntityEvent(GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, PlayerInput playerInput, WeaponParameters primaryWeaponParameters, WeaponParameters secondaryWeaponParameters)
        {
            playerSpaceEntity = new Player(
                spaceEntity,
                IManager.SpaceEntityType.Player,
                spaceEntity.GetComponent<Transform>(),
                spaceEntity.GetComponent<Rigidbody2D>(),
                spaceEntityParameters.entitySpeed,
                spaceEntityParameters.isDestroyable,
                0,
                Vector2.up,
                0,

                0,
                primaryWeaponParameters.reloadTime,
                primaryWeaponParameters.ammoCount,
                primaryWeaponParameters.ammoCount,
                primaryWeaponParameters.ammoRestoreTime,
                primaryWeaponParameters.ammoRestoreTime,

                0,
                secondaryWeaponParameters.reloadTime,
                secondaryWeaponParameters.ammoCount,
                secondaryWeaponParameters.ammoCount,
                secondaryWeaponParameters.ammoRestoreTime,
                secondaryWeaponParameters.ammoRestoreTime,
                0
                );

            spaceEntities.Add(playerSpaceEntity);

            //Подпись на ивенты управления игрока.
            playerInput.PlayerRotateEvent += OnPlayerRotationEvent;
            playerInput.PlayerAccelerateEvent += OnPlayerAccelerationEvent;
            playerInput.PlayerPrimaryFireEvent += OnPlayerPrimaryFireEvent;
            playerInput.PlayerSecondaryFireEvent += PlayerSecondaryFireEvent;
        }

        //И их реализации.
        private void OnPlayerPrimaryFireEvent()
        {
            if (playerSpaceEntity != null)
            {
                if (playerSpaceEntity.primaryReload <= 0)
                {
                    playerSpaceEntity.primaryReload = playerSpaceEntity.primaryReloadTimer;
                    SpawnPrimaryProjectileEvent?.Invoke(playerSpaceEntity.spaceEntityTransform);
                }
            }
        }

        private void PlayerSecondaryFireEvent()
        {
            if (playerSpaceEntity != null)
            {
                if (playerSpaceEntity.secondaryReload <= 0 && playerSpaceEntity.secondaryAmmoCount > 0)
                {
                    playerSpaceEntity.secondaryReload = playerSpaceEntity.secondaryReloadTimer;
                    SpawnSecondaryProjectileEvent?.Invoke(playerSpaceEntity.spaceEntityTransform);
                    playerSpaceEntity.secondaryAmmoCount--;
                }
            }
        }

        private void OnPlayerAccelerationEvent(float acceleration)
        {
            if (playerSpaceEntity != null)
            {
                playerSpaceEntity.direction = playerSpaceEntity.spaceEntityTransform.up;
                playerSpaceEntity.acceleration += acceleration * 10f;
            }
        }

        private void OnPlayerRotationEvent(float rotation)
        {
            if (playerSpaceEntity != null)
            {
                playerSpaceEntity.rotation -= rotation * 100f;
                if (playerSpaceEntity.rotation <= -180)
                {
                    playerSpaceEntity.rotation += 360;
                }
                if (playerSpaceEntity.rotation >= 180)
                {
                    playerSpaceEntity.rotation -= 360;
                }
            }
        }

        //Все сущности, которые передает контроллеру спаунер добавляются в общий лист и их логика обрабатывается в OnUpdateEvent.
        private void OnSpawnSpaceEntityEvent(GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, IManager.SpaceEntityType spaceEntityType)
        {
            switch (spaceEntityType)
            {
                case IManager.SpaceEntityType.Bullet:
                    spaceEntities.Add(new SpaceEntity(spaceEntity, spaceEntityType, spaceEntity.GetComponent<Transform>(), spaceEntity.GetComponent<Rigidbody2D>(), spaceEntityParameters.entitySpeed, spaceEntityParameters.isDestroyable));
                    break;
                case IManager.SpaceEntityType.Asteroid:
                    spaceEntities.Add(new SpaceEntity(spaceEntity, spaceEntityType, spaceEntity.GetComponent<Transform>(), spaceEntity.GetComponent<Rigidbody2D>(), spaceEntityParameters.entitySpeed, spaceEntityParameters.isDestroyable));
                    break;
                case IManager.SpaceEntityType.Enemy:
                    SpaceEntity target = spaceEntities.FirstOrDefault(item => item.spaceEntityType == IManager.SpaceEntityType.Player);
                    spaceEntities.Add(new Enemy(spaceEntity, spaceEntityType, spaceEntity.GetComponent<Transform>(), spaceEntity.GetComponent<Rigidbody2D>(), spaceEntityParameters.entitySpeed, target != null ? target.spaceEntityTransform : null, spaceEntityParameters.isDestroyable));
                    break;
            }
        }

        private void OnUpdateEvent()
        {
            //Отрисовка в эдиторе границ за которыми сущности деспаунятся.
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, 0), new Vector3(bounds.max.x, bounds.max.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, 0), new Vector3(bounds.max.x, bounds.min.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, 0), new Vector3(bounds.min.x, bounds.min.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, 0), new Vector3(bounds.min.x, bounds.max.y, 0), Color.red);

            //Перебор всех сущностей, подчиненных этому контроллеру.
            for (int i = 0; i < spaceEntities.Count; i++)
            {
                List<Collider2D> results = new List<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D().NoFilter();
                switch (spaceEntities[i].spaceEntityType)
                {
                    //Логика сущности управляемой игроком.
                    case IManager.SpaceEntityType.Player:

                        Player player = (Player)spaceEntities[i];
                        player.spaceEntityTransform.rotation = Quaternion.Euler(new Vector3(0, 0, player.rotation));
                        player.acceleration -= Time.deltaTime;
                        player.acceleration = Math.Clamp(player.acceleration, 0, player.speed);
                        player.spaceEntityTransform.Translate(player.direction * Time.deltaTime * player.acceleration, Space.World);
                        player.primaryReload -= Time.deltaTime;
                        player.secondaryReload -= Time.deltaTime;

                        if (player.secondaryAmmoCount < player.secondaryAmmoMaxCount)
                        {
                            player.secondaryAmmoRestoreTimer -= Time.deltaTime;
                        }
                        if (player.secondaryAmmoRestoreTimer <= 0 && player.secondaryAmmoCount < player.secondaryAmmoMaxCount)
                        {
                            player.secondaryAmmoCount++;
                            player.secondaryAmmoRestoreTimer = player.secondaryAmmoRestore;
                        }

                        //Ивент на который подписан класс интерфейса и по которому он выводит на экран всю нужную информацию.
                        UpdatePlayerInformationEvent?.Invoke(playerSpaceEntity.spaceEntityTransform.position, playerSpaceEntity.rotation, playerSpaceEntity.acceleration, playerSpaceEntity.secondaryAmmoCount, playerSpaceEntity.secondaryAmmoRestoreTimer);

                        //Обработка столкновения игрока с другими сущностями.
                        if (spaceEntities[i].spaceEntityRigidbody.OverlapCollider(filter, results) > 0)
                        {
                            //Все сущности, которые следовали за игроком отвязываются от него и начинают лететь по прямой.
                            IEnumerable<SpaceEntity> enemies = spaceEntities.Where(item => item.spaceEntityType == IManager.SpaceEntityType.Enemy);
                            foreach (Enemy enemy in enemies)
                            {
                                enemy.target = null;
                            }

                            //Отключение геймобджекта игрока и отправка команды интерфейсу вывести на экран итоговый счет.
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            PlayerDestroyEvent?.Invoke(playerSpaceEntity.score);
                            playerSpaceEntity = null;

                            //Получение сущности, с которой столкнулся игрок и ее деспаун. 
                            SpaceEntity spaceEntity = spaceEntities.First(item => item.spaceEntityRigidbody == results[0].attachedRigidbody);
                            spaceEntity.spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntity.spaceEntityGameObject, spaceEntity.spaceEntityType, true);
                            spaceEntities.Remove(spaceEntities[i]);
                            spaceEntities.Remove(spaceEntity);
                        }
                        else
                        {
                            //Если игрок вылетел за пределы экрана - телепортируем его в противоположные координаты.
                            if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                            {
                                if (playerSpaceEntity.spaceEntityTransform.position.x > bounds.size.x / 2)
                                {
                                    playerSpaceEntity.spaceEntityTransform.position = new Vector3(-bounds.size.x / 2, playerSpaceEntity.spaceEntityTransform.position.y, 0);
                                }
                                if (playerSpaceEntity.spaceEntityTransform.position.x < -bounds.size.x / 2)
                                {
                                    playerSpaceEntity.spaceEntityTransform.position = new Vector3(bounds.size.x / 2, playerSpaceEntity.spaceEntityTransform.position.y, 0);
                                }
                                if (playerSpaceEntity.spaceEntityTransform.position.y > bounds.size.y / 2)
                                {
                                    playerSpaceEntity.spaceEntityTransform.position = new Vector3(playerSpaceEntity.spaceEntityTransform.position.x, -bounds.size.y / 2, 0);
                                }
                                if (playerSpaceEntity.spaceEntityTransform.position.y < -bounds.size.y / 2)
                                {
                                    playerSpaceEntity.spaceEntityTransform.position = new Vector3(playerSpaceEntity.spaceEntityTransform.position.x, bounds.size.y / 2, 0);
                                }
                            }
                        }
                        break;
                    //Логика сущностей астероидов (Астероиды, их осколки, или кто-либо еще, добавленный впоследствии).
                    case IManager.SpaceEntityType.Asteroid:

                        //Движение в направлении вектора Up трансформа.
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //Деспаун при вылете за пределы экрана.
                        if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                        {
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);
                            spaceEntities.Remove(spaceEntities[i]);
                        }

                        break;
                    //Логика сущностей врагов (Обычные враги и босс, которого я добавил для тестирования или кто-либо еще, добавленный впоследствии).
                    case IManager.SpaceEntityType.Enemy:

                        //Нацеливание и движение на игрока.
                        Transform target = ((Enemy)spaceEntities[i]).target;
                        if (target != null)
                        {
                            spaceEntities[i].spaceEntityTransform.up = target.position - spaceEntities[i].spaceEntityTransform.position;
                        }
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //Деспаун при вылете за пределы экрана.
                        if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                        {
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);
                            spaceEntities.Remove(spaceEntities[i]);
                        }

                        break;
                    //Логика сущностей снарядов (Пуля и лазер или кто-либо еще, добавленный впоследствии) Стоит переименовать в Projectile.
                    case IManager.SpaceEntityType.Bullet:

                        //Прямолинейное движение в направлении вектора Up трансформа. 
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //Если сущность сталкивается с какой-либо другой сущностью.
                        if (spaceEntities[i].spaceEntityRigidbody.OverlapCollider(filter, results) > 0)
                        {
                            //Если сущность уничтожается при столкновении, как, например, пуля.
                            if (spaceEntities[i].isDestroyable)
                            {
                                //Деактивация и деспаун сущности.
                                spaceEntities[i].spaceEntityGameObject.SetActive(false);
                                DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);

                                //Деактивация и деспаун сущности с которой столкнулся снаряд
                                SpaceEntity spaceEntity = spaceEntities.First(item => item.spaceEntityRigidbody == results[0].attachedRigidbody);
                                spaceEntity.spaceEntityGameObject.SetActive(false);
                                DespawnSpaceEntityEvent?.Invoke(spaceEntity.spaceEntityGameObject, spaceEntity.spaceEntityType, true);

                                spaceEntities.Remove(spaceEntities[i]);
                                spaceEntities.Remove(spaceEntity);

                                //Добавление одного очка в счет игрока.
                                if (playerSpaceEntity != null)
                                {
                                    playerSpaceEntity.score++;
                                }
                            }
                            //Если сущность не уничтожается при столкновении, как, например, лазер.
                            else
                            {
                                //Тут аналогично, но деактивируются и деспаунятся только сущности с которыми столкнулся снаряд. Сам он 
                                SpaceEntity spaceEntity = spaceEntities.First(item => item.spaceEntityRigidbody == results[0].attachedRigidbody);
                                spaceEntity.spaceEntityGameObject.SetActive(false);

                                DespawnSpaceEntityEvent?.Invoke(spaceEntity.spaceEntityGameObject, spaceEntity.spaceEntityType, true);
                                spaceEntities.Remove(spaceEntity);

                                if (playerSpaceEntity != null)
                                {
                                    playerSpaceEntity.score++;
                                }
                            }
                        }
                        else
                        {
                            //Деспаун снаряда если он вылетел за пределы экрана.
                            if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                            {
                                spaceEntities[i].spaceEntityGameObject.SetActive(false);
                                DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);
                                spaceEntities.Remove(spaceEntities[i]);
                            }
                        }
                        break;
                }
            }
        }

        //Класс для хранения всех параметров сущностей. Его используют астероиды и их осколки. Более сложные сущности наследуются от этого класса.
        public class SpaceEntity
        {
            public GameObject spaceEntityGameObject;
            public IManager.SpaceEntityType spaceEntityType;
            public Transform spaceEntityTransform;
            public Rigidbody2D spaceEntityRigidbody;
            public float speed;
            public bool isDestroyable;
            public SpaceEntity(GameObject spaceEntityGameObject, IManager.SpaceEntityType spaceEntityType, Transform spaceEntityTransform, Rigidbody2D spaceEntityRigidbody, float speed, bool isDestroyable)
            {
                this.spaceEntityGameObject = spaceEntityGameObject;
                this.spaceEntityType = spaceEntityType;
                this.spaceEntityTransform = spaceEntityTransform;
                this.spaceEntityRigidbody = spaceEntityRigidbody;
                this.speed = speed;
                this.isDestroyable = isDestroyable;
            }
        }

        //Класс для хранения параметров сущностей врагов. Наследуется от обычных сущностей, но так же хранит цель за которой двигается.
        public class Enemy : SpaceEntity
        {
            public Transform target;

            public Enemy(
                GameObject spaceEntityGameObject,
                IManager.SpaceEntityType spaceEntityType,
                Transform spaceEntityTransform,
                Rigidbody2D spaceEntityRigidbody,
                float speed,
                Transform target,
                bool isDestroyable
                ) : base(spaceEntityGameObject, spaceEntityType, spaceEntityTransform, spaceEntityRigidbody, speed, isDestroyable)
            {
                this.target = target;
            }
        }

        //Класс для хранения всех параметров игрока. Можно сделать комактнее, если сделать отдельный класс для параметров орудий и хранить его инстансы.
        public class Player : SpaceEntity
        {
            public float rotation;
            public Vector2 direction;
            public float acceleration;

            public float primaryReload;
            public float primaryReloadTimer;
            public int primaryAmmoCount;
            public int primaryAmmoMaxCount;
            public float primaryAmmoRestore;
            public float primaryAmmoRestoreTimer;

            public float secondaryReload;
            public float secondaryReloadTimer;
            public int secondaryAmmoCount;
            public int secondaryAmmoMaxCount;
            public float secondaryAmmoRestore;
            public float secondaryAmmoRestoreTimer;

            public int score;

            public Player(
                GameObject spaceEntityGameObject,
                IManager.SpaceEntityType spaceEntityType,
                Transform spaceEntityTransform,
                Rigidbody2D spaceEntityRigidbody,
                float speed,
                bool isDestroyable,
                float rotation,
                Vector2 direction,
                float acceleration,

                float primaryReload,
                float primaryReloadTimer,
                int primaryAmmoCount,
                int primaryAmmoMaxCount,
                float primaryAmmoRestore,
                float primaryAmmoRestoreTimer,

                float secondaryReload,
                float secondaryReloadTimer,
                int secondaryAmmoCount,
                int secondaryAmmoMaxCount,
                float secondaryAmmoRestore,
                float secondaryAmmoRestoreTimer,

                int score
                ) : base(spaceEntityGameObject, spaceEntityType, spaceEntityTransform, spaceEntityRigidbody, speed, isDestroyable)
            {
                this.rotation = rotation;
                this.direction = direction;
                this.acceleration = acceleration;

                this.primaryReload = primaryReload;
                this.primaryReloadTimer = primaryReloadTimer;
                this.primaryAmmoCount = primaryAmmoCount;
                this.primaryAmmoMaxCount = primaryAmmoMaxCount;
                this.primaryAmmoRestore = primaryAmmoRestore;
                this.primaryAmmoRestoreTimer = primaryAmmoRestoreTimer;

                this.secondaryReload = secondaryReload;
                this.secondaryReloadTimer = secondaryReloadTimer;
                this.secondaryAmmoCount = secondaryAmmoCount;
                this.secondaryAmmoMaxCount = secondaryAmmoMaxCount;
                this.secondaryAmmoRestore = secondaryAmmoRestore;
                this.secondaryAmmoRestoreTimer = secondaryAmmoRestoreTimer;

                this.score = score;
            }
        }
    }
}