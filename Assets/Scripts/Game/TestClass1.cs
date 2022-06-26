using System.Collections.Generic;
using UnityEngine;
using KefirTest.Core;

namespace KefirTest.Game
{
    //Тестовый контроллер, который контроллирует отдельный вид сущностей, параллельно с другими контроллерами. Тут это просто пустой геймобджект,
    //который каждый кадр телепортируется в случайное место и поворачивается на случайный угол. Контроллер получает от менеджера все спаунеры на сцене и подписывается на их ивенты.
    //Соответственно он может при помощи дополнительных условий обрабатывать конкретные ивенты. Например реагировать только на спаун сущностей определенного типа.
    //По сути в такой контроллер можно вынести логику игрока, чтобы сделать основной контроллер компактнее. Он будет полноценно работать.
    public class TestClass1 : IDespawning
    {
        private IManager manager;
        private List<TestSpaceEntity> spaceEntities = new List<TestSpaceEntity>();
        private Bounds bounds;
        Vector2 cameraWorldSize;

        public TestClass1(IManager manager)
        {
            this.manager = manager;
        }

        public event IDespawning.DespawnSpaceEntityHandle DespawnSpaceEntityEvent;
        public event IDespawning.SpawnPrimaryProjectileHandle SpawnPrimaryProjectileEvent;
        public event IDespawning.SpawnSecondaryProjectileHandle SpawnSecondaryProjectileEvent;

        public void Initialize(List<ISpawning> spawners)
        {
            cameraWorldSize = new Vector2(2 * Camera.main.orthographicSize * Camera.main.aspect, 2 * Camera.main.orthographicSize);
            bounds = new Bounds(Vector3.zero, new Vector3(cameraWorldSize.x * 1.1f, cameraWorldSize.y * 1.1f, 1));
            foreach (ISpawning spawner in spawners)
            {
                spawner.SpawnSpaceEntityEvent += OnSpawnSpaceEntityEvent;
            }
            manager.UpdateEvent += OnUpdateEvent;
        }

        private void OnUpdateEvent()
        {
            for (int i = 0; i < spaceEntities.Count; i++)
            {
                spaceEntities[i].spaceEntityTransform.position = new Vector3(Random.Range(-cameraWorldSize.x, cameraWorldSize.x), Random.Range(-cameraWorldSize.y, cameraWorldSize.y), 0);
                if (spaceEntities[i].spaceEntityType != IManager.SpaceEntityType.Player)
                {
                    if (!bounds.Contains(spaceEntities[i].spaceEntityTransform.position))
                    {
                        DespawnSpaceEntityEvent?.Invoke(spaceEntities[i].spaceEntityGameObject, spaceEntities[i].spaceEntityType,false);
                        spaceEntities.Remove(spaceEntities[i]);
                    }
                }
            }
        }

        private void OnSpawnSpaceEntityEvent(GameObject testSpaceEntity, SpaceEntityParameters spaceEntityParameters, IManager.SpaceEntityType testSpaceEntityType)
        {
            switch (testSpaceEntityType)
            {
                case IManager.SpaceEntityType.TestType:
                    spaceEntities.Add(new TestSpaceEntity(testSpaceEntity, testSpaceEntityType, testSpaceEntity.GetComponent<Transform>()));
                    break;
            }
        }

        public class TestSpaceEntity
        {
            public GameObject spaceEntityGameObject;
            public IManager.SpaceEntityType spaceEntityType;
            public Transform spaceEntityTransform;
            public TestSpaceEntity(GameObject spaceEntityGameObject, IManager.SpaceEntityType spaceEntityType, Transform spaceEntityTransform)
            {
                this.spaceEntityGameObject = spaceEntityGameObject;
                this.spaceEntityType = spaceEntityType;
                this.spaceEntityTransform = spaceEntityTransform;
            }
        }
    }
}