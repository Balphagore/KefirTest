using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using KefirTest.Core;

namespace KefirTest.Game
{
    //������� ���������. ������������ ������ ������ � �������� ���������.
    //��� �������������� ������ ������������ ��������� ������ �� ���������������� ScriptableObject'��. �������� ��� �������������� ������ ��������� � ����� �� ��������������� ������.
    public class SpaceEntitiesSpawner : ISpawningPlayer, IUsedPool
    {
        private IManager manager;

        //��� ��� ������� ���������� � ������� �������� � ��������������� ������ ��������� ������ � ����� �� ������, �� ��� ��� �� ��������� ��������� �����, �������� ��������� ������.
        //��� ��� �������� ������ ��������� ����� ��������� � ������ ���� ��������, ������� ��� ��� �������.
        private SpaceEntityParameters shardEntityParameters;
        private int shardPoolIndex;

        private List<BulletContainer> bulletContainers;

        private WeaponParameters playerPrimaryWeaponParameters;
        private int primaryProjectilePoolIndex;

        private WeaponParameters playerSecondaryWeaponParameters;
        private int secondaryProjectilePoolIndex;

        //��� ��������� ������� ��������� � ������������ �������������� ��������� ��������� ��������, � ������� �������� ��� ��������� � ����� �� ������.
        private List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters;
        private List<Spawner> spawners;

        private Vector2 cameraWorldSize;

        //������, �� ������� ������������� ��� ��������
        public event IUsedPool.CreatePoolHandle CreatePoolEvent;
        public event IUsedPool.PullObjectHandle PullObjectEvent;
        public event IUsedPool.PushObjectHandle PushObjectEvent;

        //������ �� ������� ������������� �����������. � ��������� ����� ������ ������, �� ������� ������������� ������ ����������, ������� ������������ ������ ������.
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
            //������ �������� ������ ����� ����� ������������ �� ��� ������ ���������� � ������ �� �� �����.
            cameraWorldSize = new Vector2(2 * Camera.main.orthographicSize * Camera.main.aspect * (1f + boundsEdge), 2 * Camera.main.orthographicSize * (1f + boundsEdge));

            //����� ������.
            GameObject player = GameObject.Instantiate(playerEntityParameters.spaceEntityPrefab, Vector3.zero, Quaternion.identity, manager.managerMonoBehaviour.transform);
            player.name = "Player";
            SpawnPlayerSpaceEntityEvent?.Invoke(player, playerEntityParameters, playerInput, playerPrimaryWeaponParameters, playerSecondaryWeaponParameters);

            //�������� ������� �������, � ������� �������� ���������� ��� ������. ��� ����������� ������� ����� ���������� ������� - ��������, ��� �������� ������ ��������� ������,
            //��� ��� ��� ��� �� ���� ������� � ������������ �����, �� ���� ������� ������ ����. ��� �������� ���������� ������ ������ ����, ��� ��� ��� ��������� ��� �����������.
            //���� ������ � ��������� ����� ���������� ��� �����-�� ��������, �� ����� ����� ��� �� �������� ��������� �����.
            spawners = new List<Spawner>();
            bulletContainers = new List<BulletContainer>();
            shardPoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + shardEntityParameters.spaceEntityPrefab.name, shardEntityParameters.spaceEntityPrefab, 1);

            primaryProjectilePoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + playerPrimaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab.name, playerPrimaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab, 1);
            bulletContainers.Add(new BulletContainer(primaryProjectilePoolIndex, new List<GameObject>()));

            secondaryProjectilePoolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + playerSecondaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab.name, playerSecondaryWeaponParameters.spaceEntityParameters.spaceEntityPrefab, 1);
            bulletContainers.Add(new BulletContainer(secondaryProjectilePoolIndex, new List<GameObject>()));

            //�������� ������������ �������� ��� ������� ScriptableObject � ����������� ������, ������� ������� �� ���������. ��� �� ������� ����� ������������ � ������������ ���� ��������.
            //���� �������� ��� �����-������ �������� � ����������� �������, �� ��� ��� ��� �� ������������� ��������� ������� � ��� ��������. 
            foreach (SpaceEntitySpawnParameters sp in spaceEntitySpawnParameters)
            {
                int poolIndex = CreatePoolEvent.Invoke(this.GetType().Name + "." + sp.spaceEntityParameters.spaceEntityPrefab.name, sp.spaceEntityParameters.spaceEntityPrefab, 1);
                spawners.Add(new Spawner(sp, sp.spaceEntityParameters, sp.startSpawn, poolIndex, new List<GameObject>()));
            }
            
            //������� �� ������ ������������.
            foreach (IDespawning despawner in despawners)
            {
                despawner.DespawnSpaceEntityEvent += OnDespawnSpaceEntityEvent;
                despawner.SpawnPrimaryProjectileEvent += OnSpawnPrimaryProjectileEvent;
                despawner.SpawnSecondaryProjectileEvent += OnSpawnSecondaryProjectileEvent;
            }

            //��� ��� ��� ������� ������� ����� �������, �� ��� ����� ������������ ������� ���������.
            manager.managerMonoBehaviour.StartCoroutine(spawnCoroutine());
        }

        //����� �������� ��������� � ��������������� ������ � ����� �� ��������������� ������.
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

        //�������� ������. ���������� ��� ��������� �������� � �������� ������ � ��� �� Time.DeltaTime. ���� ��� ���� ���������� �������� �������� �������, �� �� ���������������� ����
        //������� ������ � ������������ �����������.
        private IEnumerator spawnCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i].timer <= 0)
                    {
                        //�������� ����� ����� ��������� �����������, �� ������������ � ������������ ����������.
                        if (!(spawners[i].spaceEntitySpawnParameters.isSingle && spawners[i].spawnedSpaceEntities.Count > 0))
                        {
                            (Vector3, float) positionRotation = GetRandomSpawnPositionRotation();
                            //��������� ������� �� �����. ��� �����, ������������� � ������������ ���������� �� ���� ����� ���� � �� �������� ���.
                            GameObject instance = PullObjectEvent?.Invoke(spawners[i].poolIndex);
                            instance.transform.position = positionRotation.Item1;
                            instance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, positionRotation.Item2));

                            //���� � �������� � �������� ���� ������, �� ������� ��� �� ��������� ����, ����� ��������� � �� ������� ����� ������ �� ��������� � �� ������������ ��������.
                            if (instance.transform.childCount > 0)
                            {
                                instance.transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-180, 180)));
                            }
                            spawners[i].spawnedSpaceEntities.Add(instance);

                            //���� ������������ �������� ��������� ��� ����, �� ���������� �������� ParentConstraint, ����� �� ������ �� �������� ������ �� ������ �����������.
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

        //���� �����-���� �� ������������, �� ������� �������� ���� �������, �������� ����� ��������.
        private void OnDespawnSpaceEntityEvent(GameObject spaceEntity, IManager.SpaceEntityType spaceEntityType, bool isDestroyed)
        {
            //���� ��� �������� ������, �� ������� � ����������� ��������.
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
                //���� ��� �� ������, �� ������� � ����������� ���������.
                for (int i = 0; i < spawners.Count; i++)
                {
                    if (spawners[i].spaceEntitySpawnParameters.spaceEntityType == spaceEntityType)
                    {
                        if (spawners[i].spawnedSpaceEntities.Contains(spaceEntity))
                        {
                            //���� ��� ��� ��������, �� ������� ������ ���� ��� �������.
                            if (spaceEntityType == IManager.SpaceEntityType.Asteroid && isDestroyed)
                            {
                                SpawnShard(spaceEntity);
                                SpawnShard(spaceEntity);
                                SpawnShard(spaceEntity);
                            }
                            //� ���������� � ��� ������ ��� ����������� �������������.
                            int pullIndex = spawners[i].poolIndex;
                            spawners[i].spawnedSpaceEntities.Remove(spaceEntity);
                            PushObjectEvent?.Invoke(pullIndex, spaceEntity);
                        }
                    }
                }
            }
        }

        //������� ������ �������.
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

        //��������� ��������� ������� � �����������. � ����������� �� ���� �� ����� ������� ������ ������������ ��������, ��� �������������� ���, ����� ������ ����� ������� ����.
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

        //����� � ������� �������� ��������� ����������� ���������, ������� ������������ �����.
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

        //�����, � ������� �������� �������. ��� ��� ��� ��������� � ������ ����, �� ��� �������� ���� �������� - ���� ������ �� ��������� � � ����� ���.
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