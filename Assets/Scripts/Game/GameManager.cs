using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KefirTest.Core;

namespace KefirTest.Game
{
    //Полноценный менеджер, который передает зависимости и инициализирует классы, которые обрабатывают логику. Так же рассылает им ивент своего Update для синхронизации.
    public class GameManager : MonoBehaviour, IManager
    {
        [SerializeField]
        private float boundsEdge;

        [Header("References")]
        //Так как для тестирования я добавил на сцену еще спаунер, то параметры спауна сущностей для спауна разбиты на две части. Каждая отправляется соответствующему спаунеру.
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
        //Так как осколки астероидов спаунятся только в случае уничтожения астероида, то спаунеру передаются только параметры самой сущности. Параметров спауна осколков нет.
        [SerializeField]
        private SpaceEntityParameters shardEntityParameters;
        [SerializeField]
        private GameInterface gameInterface;

        public MonoBehaviour managerMonoBehaviour { get; set; }

        public event IManager.UpdateHandler UpdateEvent;

        private void Start()
        {
            managerMonoBehaviour = this;

            //Создание категорий, в которые попадают разные классы, чтобы отправить тем, кто подписывается на их ивенты.
            //Например если в сцене один спаунер и несколько контроллеров, то спаунеру отправится список всех контроллеров.
            List<ISpawning> spawners = new List<ISpawning>();
            List<IUsedPool> poolUsers = new List<IUsedPool>();
            List<IDespawning> despawners = new List<IDespawning>();

            //Конструкторы классов с логикой.

            //Спаунеры. Один полноценный и один для тестирования.
            SpaceEntitiesSpawner spaceEntitiesSpawner = new SpaceEntitiesSpawner(this, playerPrimaryWeaponParameters, playerSecondaryWeaponParameters, shardEntityParameters, spaceEntitySpawnParameters1);
            spawners.Add(spaceEntitiesSpawner);
            poolUsers.Add(spaceEntitiesSpawner);
            TestClass2 testClass2 = new TestClass2(this, spaceEntitySpawnParameters2);
            spawners.Add(testClass2);
            poolUsers.Add(testClass2);

            //Пул объектов.
            ObjectsPool objectsPool = new ObjectsPool(this);

            //Контроллеры. Один полноценный и один для тестирования.
            SpaceEntitiesController spaceEntitiesController = new SpaceEntitiesController(this);
            despawners.Add(spaceEntitiesController);
            TestClass1 testClass1 = new TestClass1(this);
            despawners.Add(testClass1);

            //Управление игрока.
            PlayerInput playerInput = new PlayerInput(this);

            //Инициализация всех классов и рассылка им списков тех, на чьи ивенты они подписываются.
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

        //Так как у классов, не наследуемых от монобеха нет собственного Upfate, то они все подписаны на Update менеджера.
        private void Update()
        {
            UpdateEvent?.Invoke();
        }
    }
}