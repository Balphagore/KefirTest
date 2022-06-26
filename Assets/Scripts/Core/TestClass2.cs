using UnityEngine;

namespace KefirTest.Core
{
    //Класс подменяющий спаунер в сцене Core. Пробрасывает контроллеру ссылки и параметры астероида и одного врага на сцене чтобы протестировать минимальный функционал.
    public class TestClass2 : ISpawning
    {
        public TestClass2()
        {
        }

        public event ISpawning.SpawnSpaceEntityHandle SpawnSpaceEntityEvent;

        public void Initialize(GameObject asteroid, SpaceEntityParameters asteroidParameters, GameObject enemy, SpaceEntityParameters enemyParameters)
        {
            SpawnSpaceEntityEvent?.Invoke(asteroid, asteroidParameters, IManager.SpaceEntityType.Asteroid);
            SpawnSpaceEntityEvent?.Invoke(enemy, enemyParameters, IManager.SpaceEntityType.Enemy);
        }
    }
}