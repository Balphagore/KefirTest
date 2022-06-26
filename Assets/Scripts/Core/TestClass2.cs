using UnityEngine;

namespace KefirTest.Core
{
    //����� ����������� ������� � ����� Core. ������������ ����������� ������ � ��������� ��������� � ������ ����� �� ����� ����� �������������� ����������� ����������.
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