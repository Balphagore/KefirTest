using UnityEngine;

namespace KefirTest.Core
{
    //Интерфейс для спаунеров. Если в сцене работает несколько спаунеров, то интерфейс позволяет взаимодействовать с ними независимо от их реализаций.
    public interface ISpawning
    {
        delegate void SpawnSpaceEntityHandle(GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, IManager.SpaceEntityType spaceEntityType);
        event SpawnSpaceEntityHandle SpawnSpaceEntityEvent;
    }

    //Наследник интерфейса спаунеров для спаунера, который должен создать игрока.
    public interface ISpawningPlayer : ISpawning
    {
        delegate void SpawnPlayerSpaceEntityHandle(
            GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, PlayerInput playerInput, WeaponParameters primaryWeaponParameters, WeaponParameters secondaryWeaponParameters);
        event SpawnPlayerSpaceEntityHandle SpawnPlayerSpaceEntityEvent;
    }
}