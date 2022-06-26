using UnityEngine;

namespace KefirTest.Core
{
    //��������� ��� ����������. � ������ ������ ����� ���� ��������� ���������� ����������, ��������� ��������� ���������� � ��� ���������� �� ������������ ���� ����������.
    public interface IManager
    {
        //�������� � ��������� �������� �� ����� ������������� ����������. ��� ������, �������������� ������ ������������� �� Update ���������, ��� ��� �� ����� ������������.
        delegate void UpdateHandler();
        event UpdateHandler UpdateEvent;

        //����� ��������� �������� ��������� �� ����� �������.
        MonoBehaviour managerMonoBehaviour { get; }

        public enum SpaceEntityType
        {
            Player,
            Asteroid,
            Enemy,
            Bullet,
            TestType
        }
    }
}