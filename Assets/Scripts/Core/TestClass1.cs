using UnityEngine;

namespace KefirTest.Core
{
    //Класс подменяющий спаунер в сцене Core. Пробрасывает контроллеру ссылки и параметры игрока и двух пуль на сцене чтобы протестировать минимальный функционал.
    public class TestClass1 : ISpawningPlayer
    {
        public TestClass1()
        {
        }

        public event ISpawning.SpawnSpaceEntityHandle SpawnSpaceEntityEvent;
        public event ISpawningPlayer.SpawnPlayerSpaceEntityHandle SpawnPlayerSpaceEntityEvent;

        public void Initialize(GameObject player, SpaceEntityParameters playerEntityParameters, PlayerInput playerInput, WeaponParameters weaponParameters, WeaponParameters weaponParameters2, GameObject bullet1, GameObject bullet2, SpaceEntityParameters bulletEntityParameters)
        {
            SpawnPlayerSpaceEntityEvent?.Invoke(player, playerEntityParameters, playerInput, weaponParameters, weaponParameters2);
            SpawnSpaceEntityEvent?.Invoke(bullet1, bulletEntityParameters, IManager.SpaceEntityType.Bullet);
            SpawnSpaceEntityEvent?.Invoke(bullet2, bulletEntityParameters, IManager.SpaceEntityType.Bullet);
        }
    }
}