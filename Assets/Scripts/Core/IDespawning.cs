using UnityEngine;

namespace KefirTest.Core
{
    //��������� ��� ������������ ���������. ���� � ����� �������� ��������� ������������, �� ��������� ��������� ����������������� � ���� ���������� �� �� ����������.
    //��������, ��������, ��� �� ���������. ����� �������������, ��������, � IController.
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