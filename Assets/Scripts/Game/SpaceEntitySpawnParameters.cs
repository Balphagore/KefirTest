using UnityEngine;
using KefirTest.Core;

namespace KefirTest.Game
{
    //ScriptableObject ��� �������� ���������� ������ ���������.
    [CreateAssetMenu(fileName = "SpaceEntitySpawnParameters", menuName = "ScriptableObjects/SpaceEntitySpawnParameters")]
    public class SpaceEntitySpawnParameters : ScriptableObject
    {
        public IManager.SpaceEntityType spaceEntityType;
        public SpaceEntityParameters spaceEntityParameters;
        public bool isSingle;
        public float startSpawn;
        public float spawnFrequency;
    }
}