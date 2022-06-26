using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KefirTest.Core
{
    //���������� ���������. ������������ ������ ���������, ��������� ���������� � ���������� �� ��, ����� ��� ����������/�������� �� ������� ������.
    //� ����� ����� ��������� ��� �� ��������� ����� ������������������ ������������, �������� ���������� ������, ���������� ���������� � ������.
    public class SpaceEntitiesController : IDespawning
    {
        private IManager manager;
        private List<SpaceEntity> spaceEntities = new List<SpaceEntity>();
        private Player playerSpaceEntity;
        //�������� ������ ��������� �� ������� ������ �������� ��� ������ Bounds. �������� ��� ������� ��������, ����� ����� ������ ��������� �������� ���������.
        private Bounds bounds;

        //������ �� ������� ������������� ����� ����������.
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
            //������� �� ������� �������� ����������� �������� � ����������� �� ���������� � ������� �������� ����.
            Vector2 cameraWorldSize = new Vector2(2 * Camera.main.orthographicSize * Camera.main.aspect, 2 * Camera.main.orthographicSize);
            bounds = new Bounds(Vector3.zero, new Vector3(cameraWorldSize.x * (1f + boundsEdge), cameraWorldSize.y * (1f + boundsEdge), 1));

            //������� �� ������ ���������, ���������� �� �� ����������.
            foreach (ISpawning spawner in spawners)
            {
                spawner.SpawnSpaceEntityEvent += OnSpawnSpaceEntityEvent;
                if (spawner is ISpawningPlayer)
                {
                    //���� �� ��������� ������� ������, ������� �� ���� �����, ��� ��� ������ ���������� ������������ ������ ������.
                    ISpawningPlayer playerSpawner = (ISpawningPlayer)spawner;
                    playerSpawner.SpawnPlayerSpaceEntityEvent += OnSpawnPlayerSpaceEntityEvent;
                }
            }
            //������� �� Update ���������, ����� ���������������� � ��� ������.
            manager.UpdateEvent += OnUpdateEvent;
        }

        //�����, ���������� �������� ������ ��� ������ ����������� � ���������. ����� ������� ������������, �������� ��������� ������ � ��������� �����.
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

            //������� �� ������ ���������� ������.
            playerInput.PlayerRotateEvent += OnPlayerRotationEvent;
            playerInput.PlayerAccelerateEvent += OnPlayerAccelerationEvent;
            playerInput.PlayerPrimaryFireEvent += OnPlayerPrimaryFireEvent;
            playerInput.PlayerSecondaryFireEvent += PlayerSecondaryFireEvent;
        }

        //� �� ����������.
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

        //��� ��������, ������� �������� ����������� ������� ����������� � ����� ���� � �� ������ �������������� � OnUpdateEvent.
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
            //��������� � ������� ������ �� �������� �������� �����������.
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, 0), new Vector3(bounds.max.x, bounds.max.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, 0), new Vector3(bounds.max.x, bounds.min.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, 0), new Vector3(bounds.min.x, bounds.min.y, 0), Color.red);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, 0), new Vector3(bounds.min.x, bounds.max.y, 0), Color.red);

            //������� ���� ���������, ����������� ����� �����������.
            for (int i = 0; i < spaceEntities.Count; i++)
            {
                List<Collider2D> results = new List<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D().NoFilter();
                switch (spaceEntities[i].spaceEntityType)
                {
                    //������ �������� ����������� �������.
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

                        //����� �� ������� �������� ����� ���������� � �� �������� �� ������� �� ����� ��� ������ ����������.
                        UpdatePlayerInformationEvent?.Invoke(playerSpaceEntity.spaceEntityTransform.position, playerSpaceEntity.rotation, playerSpaceEntity.acceleration, playerSpaceEntity.secondaryAmmoCount, playerSpaceEntity.secondaryAmmoRestoreTimer);

                        //��������� ������������ ������ � ������� ����������.
                        if (spaceEntities[i].spaceEntityRigidbody.OverlapCollider(filter, results) > 0)
                        {
                            //��� ��������, ������� ��������� �� ������� ������������ �� ���� � �������� ������ �� ������.
                            IEnumerable<SpaceEntity> enemies = spaceEntities.Where(item => item.spaceEntityType == IManager.SpaceEntityType.Enemy);
                            foreach (Enemy enemy in enemies)
                            {
                                enemy.target = null;
                            }

                            //���������� ������������ ������ � �������� ������� ���������� ������� �� ����� �������� ����.
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            PlayerDestroyEvent?.Invoke(playerSpaceEntity.score);
                            playerSpaceEntity = null;

                            //��������� ��������, � ������� ���������� ����� � �� �������. 
                            SpaceEntity spaceEntity = spaceEntities.First(item => item.spaceEntityRigidbody == results[0].attachedRigidbody);
                            spaceEntity.spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntity.spaceEntityGameObject, spaceEntity.spaceEntityType, true);
                            spaceEntities.Remove(spaceEntities[i]);
                            spaceEntities.Remove(spaceEntity);
                        }
                        else
                        {
                            //���� ����� ������� �� ������� ������ - ������������� ��� � ��������������� ����������.
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
                    //������ ��������� ���������� (���������, �� �������, ��� ���-���� ���, ����������� ������������).
                    case IManager.SpaceEntityType.Asteroid:

                        //�������� � ����������� ������� Up ����������.
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //������� ��� ������ �� ������� ������.
                        if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                        {
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);
                            spaceEntities.Remove(spaceEntities[i]);
                        }

                        break;
                    //������ ��������� ������ (������� ����� � ����, �������� � ������� ��� ������������ ��� ���-���� ���, ����������� ������������).
                    case IManager.SpaceEntityType.Enemy:

                        //����������� � �������� �� ������.
                        Transform target = ((Enemy)spaceEntities[i]).target;
                        if (target != null)
                        {
                            spaceEntities[i].spaceEntityTransform.up = target.position - spaceEntities[i].spaceEntityTransform.position;
                        }
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //������� ��� ������ �� ������� ������.
                        if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                        {
                            spaceEntities[i].spaceEntityGameObject.SetActive(false);
                            DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);
                            spaceEntities.Remove(spaceEntities[i]);
                        }

                        break;
                    //������ ��������� �������� (���� � ����� ��� ���-���� ���, ����������� ������������) ����� ������������� � Projectile.
                    case IManager.SpaceEntityType.Bullet:

                        //������������� �������� � ����������� ������� Up ����������. 
                        spaceEntities[i].spaceEntityTransform.Translate(Vector2.up * Time.deltaTime * spaceEntities[i].speed);

                        //���� �������� ������������ � �����-���� ������ ���������.
                        if (spaceEntities[i].spaceEntityRigidbody.OverlapCollider(filter, results) > 0)
                        {
                            //���� �������� ������������ ��� ������������, ���, ��������, ����.
                            if (spaceEntities[i].isDestroyable)
                            {
                                //����������� � ������� ��������.
                                spaceEntities[i].spaceEntityGameObject.SetActive(false);
                                DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType, false);

                                //����������� � ������� �������� � ������� ���������� ������
                                SpaceEntity spaceEntity = spaceEntities.First(item => item.spaceEntityRigidbody == results[0].attachedRigidbody);
                                spaceEntity.spaceEntityGameObject.SetActive(false);
                                DespawnSpaceEntityEvent?.Invoke(spaceEntity.spaceEntityGameObject, spaceEntity.spaceEntityType, true);

                                spaceEntities.Remove(spaceEntities[i]);
                                spaceEntities.Remove(spaceEntity);

                                //���������� ������ ���� � ���� ������.
                                if (playerSpaceEntity != null)
                                {
                                    playerSpaceEntity.score++;
                                }
                            }
                            //���� �������� �� ������������ ��� ������������, ���, ��������, �����.
                            else
                            {
                                //��� ����������, �� �������������� � ����������� ������ �������� � �������� ���������� ������. ��� �� 
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
                            //������� ������� ���� �� ������� �� ������� ������.
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

        //����� ��� �������� ���� ���������� ���������. ��� ���������� ��������� � �� �������. ����� ������� �������� ����������� �� ����� ������.
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

        //����� ��� �������� ���������� ��������� ������. ����������� �� ������� ���������, �� ��� �� ������ ���� �� ������� ���������.
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

        //����� ��� �������� ���� ���������� ������. ����� ������� ���������, ���� ������� ��������� ����� ��� ���������� ������ � ������� ��� ��������.
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