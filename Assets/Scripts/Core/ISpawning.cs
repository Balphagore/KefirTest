using UnityEngine;

namespace KefirTest.Core
{
    //��������� ��� ���������. ���� � ����� �������� ��������� ���������, �� ��������� ��������� ����������������� � ���� ���������� �� �� ����������.
    public interface ISpawning
    {
        delegate void SpawnSpaceEntityHandle(GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, IManager.SpaceEntityType spaceEntityType);
        event SpawnSpaceEntityHandle SpawnSpaceEntityEvent;
    }

    //��������� ���������� ��������� ��� ��������, ������� ������ ������� ������.
    public interface ISpawningPlayer : ISpawning
    {
        delegate void SpawnPlayerSpaceEntityHandle(
            GameObject spaceEntity, SpaceEntityParameters spaceEntityParameters, PlayerInput playerInput, WeaponParameters primaryWeaponParameters, WeaponParameters secondaryWeaponParameters);
        event SpawnPlayerSpaceEntityHandle SpawnPlayerSpaceEntityEvent;
    }
}