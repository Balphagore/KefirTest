using UnityEngine;

namespace KefirTest.Core
{
    //Интерфейс для менеджеров. В разных сценах могут быть различные реализации менеджеров, интерфейс позволяет обращаться к ним независимо от особенностей этих реализаций.
    public interface IManager
    {
        //Менеджер и интерфейс являются на сцене единственными монобехами. Все классы, обрабатывающие логику подписываются на Update менеджера, так как не имеют собственного.
        delegate void UpdateHandler();
        event UpdateHandler UpdateEvent;

        //Чтобы запускать корутины спаунеров им нужен монобех.
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