using UnityEngine;

namespace KefirTest.Game
{
    public interface IUsedPool
    {
        public delegate int CreatePoolHandle(string id, GameObject prefab, int size);
        public event CreatePoolHandle CreatePoolEvent;

        public delegate GameObject PullObjectHandle(int poolIndex);
        public event PullObjectHandle PullObjectEvent;

        public delegate void PushObjectHandle(int poolIndex, GameObject pushedObject);
        public event PushObjectHandle PushObjectEvent;
    }
}