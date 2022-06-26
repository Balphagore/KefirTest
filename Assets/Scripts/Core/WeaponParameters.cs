using UnityEngine;

namespace KefirTest.Core
{
    //ScriptableObject ��� �������� ���������� ������.
    [CreateAssetMenu(fileName = "WeaponParameters", menuName = "ScriptableObjects/WeaponParameters")]
    public class WeaponParameters : ScriptableObject
    {
        public SpaceEntityParameters spaceEntityParameters;
        public float reloadTime;
        public int ammoCount;
        public float ammoRestoreTime;
    }
}