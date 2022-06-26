using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using KefirTest.Core;

namespace KefirTest.Game
{
    //Тестовый спаунер, который спаунит отдельный вид сущностей параллельно с другими спаунерами. Полноценно работает с пулом объектов и вызывает ивенты.
    //При помощи отдельных спаунеров можно сделать основной спаунер компактнее. Например можно вынести сюда логику спауна снарядов и осколков астероидов, а в основном спаунере
    //Оставить только спаун тех сущностей, которые появляются по таймеру.
    public class TestClass2 : ISpawning, IUsedPool
    {
        private IManager manager;
        private List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters;
        private List<Spawner> spawners;
        private Vector2 cameraWorldSize;

        public event ISpawning.SpawnSpaceEntityHandle SpawnSpaceEntityEvent;
        public event IUsedPool.CreatePoolHandle CreatePoolEvent;
        public event IUsedPool.PullObjectHandle PullObjectEvent;
        public event IUsedPool.PushObjectHandle PushObjectEvent;

        public TestClass2(IManager manager, List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters)
        {
            this.manager = manager;
            this.spaceEntitySpawnParameters = spaceEntitySpawnParameters;
        }

        public void Initialize(List<IDespawning> despawners)
        {
            spawners = new List<Spawner>();
            foreach (SpaceEntitySpawnParameters sp in spaceEntitySpawnParameters)
            {
                int poolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + sp.spaceEntityParameters.spaceEntityPrefab.name, sp.spaceEntityParameters.spaceEntityPrefab, 1);
                spawners.Add(new Spawner(sp, sp.spaceEntityParameters, sp.startSpawn, poolIndex, new List<GameObject>()));
            }
            foreach (IDespawning despawner in despawners)
            {
                despawner.DespawnSpaceEntityEvent += OnDespawnSpaceEntityEvent;
            }
            manager.managerMonoBehaviour.StartCoroutine(spawnCoroutine());
        }

        private void OnDespawnSpaceEntityEvent(GameObject spaceEntity, IManager.SpaceEntityType spaceEntityType, bool isDestroyed)
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (spawners[i].spaceEntitySpawnParameters.spaceEntityType == spaceEntityType)
                {
                    int pullIndex = spawners[i].poolIndex;
                    spawners[i].spawnedSpaceEntities.Remove(spaceEntity);
                    PushObjectEvent?.Invoke(pullIndex, spaceEntity);
                }
            }
        }

        private IEnumerator spawnCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i].timer <= 0)
                    {
                        if (!(spawners[i].spaceEntitySpawnParameters.isSingle && spawners[i].spawnedSpaceEntities.Count > 0))
                        {
                            (Vector3, float) positionRotation = GetRandomSpawnPositionRotation();
                            GameObject instance = PullObjectEvent?.Invoke(spawners[i].poolIndex);
                            instance.transform.position = positionRotation.Item1;
                            instance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, positionRotation.Item2));
                            if (instance.transform.childCount > 0)
                            {
                                instance.transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-180, 180)));
                            }
                            spawners[i].spawnedSpaceEntities.Add(instance);
                            if (spawners[i].spaceEntitySpawnParameters.spaceEntityType == IManager.SpaceEntityType.Enemy)
                            {
                                ConstraintSource source = new ConstraintSource();
                                source.sourceTransform = manager.managerMonoBehaviour.transform;
                                source.weight = 1;
                                ParentConstraint parentConstraint = instance.GetComponentInChildren<ParentConstraint>();
                                parentConstraint.AddSource(source);
                            }
                            SpawnSpaceEntityEvent?.Invoke(instance, spawners[i].spaceEntityParameters, spawners[i].spaceEntitySpawnParameters.spaceEntityType);
                            spawners[i].timer = 0;
                            spawners[i].timer = spawners[i].spaceEntitySpawnParameters.spawnFrequency;
                        }
                    }
                    spawners[i].timer -= Time.deltaTime;
                }
                yield return null;
            }
        }

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

        public class Spawner
        {
            public SpaceEntitySpawnParameters spaceEntitySpawnParameters;
            public SpaceEntityParameters spaceEntityParameters;
            public float timer;
            public int poolIndex;
            public List<GameObject> spawnedSpaceEntities;
            public Spawner(SpaceEntitySpawnParameters spaceEntitySpawnParameters, SpaceEntityParameters spaceEntityParameters, float timer, int poolIndex, List<GameObject> spawnedSpaceEntities)
            {
                this.spaceEntitySpawnParameters = spaceEntitySpawnParameters;
                this.spaceEntityParameters = spaceEntityParameters;
                this.timer = timer;
                this.poolIndex = poolIndex;
                this.spawnedSpaceEntities = spawnedSpaceEntities;
            }
        }
    }
}