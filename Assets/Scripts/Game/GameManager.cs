using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KefirTest.Core;

namespace KefirTest.Game
{
    //����������� ��������, ������� �������� ����������� � �������������� ������, ������� ������������ ������. ��� �� ��������� �� ����� ������ Update ��� �������������.
    public class GameManager : MonoBehaviour, IManager
    {
        [SerializeField]
        private float boundsEdge;

        [Header("References")]
        //��� ��� ��� ������������ � ������� �� ����� ��� �������, �� ��������� ������ ��������� ��� ������ ������� �� ��� �����. ������ ������������ ���������������� ��������.
        [SerializeField]
        private List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters1 = new List<SpaceEntitySpawnParameters>();
        [SerializeField]
        private List<SpaceEntitySpawnParameters> spaceEntitySpawnParameters2 = new List<SpaceEntitySpawnParameters>();
        [SerializeField]
        private SpaceEntityParameters playerEntityParameters;
        [SerializeField]
        private WeaponParameters playerPrimaryWeaponParameters;
        [SerializeField]
        private WeaponParameters playerSecondaryWeaponParameters;
        //��� ��� ������� ���������� ��������� ������ � ������ ����������� ���������, �� �������� ���������� ������ ��������� ����� ��������. ���������� ������ �������� ���.
        [SerializeField]
        private SpaceEntityParameters shardEntityParameters;
        [SerializeField]
        private GameInterface gameInterface;

        public MonoBehaviour managerMonoBehaviour { get; set; }

        public event IManager.UpdateHandler UpdateEvent;

        private void Start()
        {
            managerMonoBehaviour = this;

            //�������� ���������, � ������� �������� ������ ������, ����� ��������� ���, ��� ������������� �� �� ������.
            //�������� ���� � ����� ���� ������� � ��������� ������������, �� �������� ���������� ������ ���� ������������.
            List<ISpawning> spawners = new List<ISpawning>();
            List<IUsedPool> poolUsers = new List<IUsedPool>();
            List<IDespawning> despawners = new List<IDespawning>();

            //������������ ������� � �������.

            //��������. ���� ����������� � ���� ��� ������������.
            SpaceEntitiesSpawner spaceEntitiesSpawner = new SpaceEntitiesSpawner(this, playerPrimaryWeaponParameters, playerSecondaryWeaponParameters, shardEntityParameters, spaceEntitySpawnParameters1);
            spawners.Add(spaceEntitiesSpawner);
            poolUsers.Add(spaceEntitiesSpawner);
            TestClass2 testClass2 = new TestClass2(this, spaceEntitySpawnParameters2);
            spawners.Add(testClass2);
            poolUsers.Add(testClass2);

            //��� ��������.
            ObjectsPool objectsPool = new ObjectsPool(this);

            //�����������. ���� ����������� � ���� ��� ������������.
            SpaceEntitiesController spaceEntitiesController = new SpaceEntitiesController(this);
            despawners.Add(spaceEntitiesController);
            TestClass1 testClass1 = new TestClass1(this);
            despawners.Add(testClass1);

            //���������� ������.
            PlayerInput playerInput = new PlayerInput(this);

            //������������� ���� ������� � �������� �� ������� ���, �� ��� ������ ��� �������������.
            objectsPool.Initialize(poolUsers);
            spaceEntitiesController.Initialize(spawners, boundsEdge);
            testClass1.Initialize(spawners);
            spaceEntitiesSpawner.Initialize(despawners, playerEntityParameters, playerInput, boundsEdge);
            testClass2.Initialize(despawners);
            playerInput.Initialize();
            gameInterface.Initialize(spaceEntitiesController);

            gameInterface.RestartGameEvent += OnRestartGameEvent;
        }

        private void OnRestartGameEvent()
        {
            SceneManager.LoadScene(0);
        }

        //��� ��� � �������, �� ����������� �� �������� ��� ������������ Upfate, �� ��� ��� ��������� �� Update ���������.
        private void Update()
        {
            UpdateEvent?.Invoke();
        }
    }
}