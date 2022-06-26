using UnityEngine;

namespace KefirTest.Core
{
    //Интерфейс для контроллеров сущностей. Если в сцене работает несколько контроллеров, то интерфейс позволяет взаимодействовать с ними независимо от их реализаций.
    //Название, наверное, уже не актуально. Можно переименовать, например, в IController.
    public interface IDespawning
    {
        delegate void DespawnSpaceEntityHandle(GameObject spaceEntity, IManager.SpaceEntityType spaceEntityType, bool isDestroyed);
        event DespawnSpaceEntityHandle DespawnSpaceEntityEvent;

        delegate void SpawnPrimaryProjectileHandle(Transform playerTransform);
        event SpawnPrimaryProjectileHandle SpawnPrimaryProjectileEvent;

        delegate void SpawnSecondaryProjectileHandle(Transform playerTransform);
        event SpawnSecondaryProjectileHandle SpawnSecondaryProjectileEvent;
    }
}