using UnityEngine;

namespace KefirTest.Core
{
    //ScriptableObject для хранения параметров сущностей.
    [CreateAssetMenu(fileName = "SpaceEntityParameters", menuName = "ScriptableObjects/SpaceEntityParameters")]
    public class SpaceEntityParameters : ScriptableObject
    {
        public GameObject spaceEntityPrefab;
        public IManager.SpaceEntityType SpaceEntityType;
        public bool isDestroyable;
        public float entitySpeed;
    }
}