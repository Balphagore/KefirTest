using UnityEngine;

namespace KefirTest.Core
{
    //ScriptableObject для хранения параметров орудий.
    [CreateAssetMenu(fileName = "WeaponParameters", menuName = "ScriptableObjects/WeaponParameters")]
    public class WeaponParameters : ScriptableObject
    {
        public SpaceEntityParameters spaceEntityParameters;
        public float reloadTime;
        public int ammoCount;
        public float ammoRestoreTime;
    }
}