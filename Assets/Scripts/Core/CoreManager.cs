using System.Collections.Generic;
using UnityEngine;

namespace KefirTest.Core
{
    //Тестовый класс подменяющий менеджер, для проверки правильности работы скриптов из Core
    public class CoreManager : MonoBehaviour, IManager
    {
        [SerializeField]
        private float boundsEdge;
        [SerializeField]
        private GameObject player;
        [SerializeField]
        private SpaceEntityParameters playerEntityParameters;
        [SerializeField]
        private WeaponParameters weaponParameters;
        [SerializeField]
        private WeaponParameters weaponParameters2;
        [SerializeField]
        private GameObject bullet1;
        [SerializeField]
        private GameObject bullet2;
        [SerializeField]
        private SpaceEntityParameters bulletEntityParameters;
        [SerializeField]
        private GameObject asteroid;
        [SerializeField]
        private SpaceEntityParameters asteroidEntityParameters;
        [SerializeField]
        private GameObject enemy;
        [SerializeField]
        private SpaceEntityParameters enemyEntityParameters;

        public MonoBehaviour managerMonoBehaviour { get; set; }

        public event IManager.UpdateHandler UpdateEvent;


        private void Start()
        {
            managerMonoBehaviour = this;

            List<ISpawning> spawners = new List<ISpawning>();

            TestClass1 testClass1 = new TestClass1();
            spawners.Add(testClass1);

            TestClass2 testClass2 = new TestClass2();
            spawners.Add(testClass2);

            SpaceEntitiesController spaceEntitesController = new SpaceEntitiesController(this);

            PlayerInput playerInput = new PlayerInput(this);

            spaceEntitesController.Initialize(spawners, boundsEdge);
            testClass1.Initialize(player, playerEntityParameters, playerInput, weaponParameters, weaponParameters2, bullet1, bullet2, bulletEntityParameters);
            testClass2.Initialize(asteroid, asteroidEntityParameters, enemy, enemyEntityParameters);
            playerInput.Initialize();
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }
    }
}