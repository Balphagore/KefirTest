using UnityEngine;
using System;
using System.Collections.Generic;

namespace KefirTest.Game
{
    public class ObjectsPool
    {
        private GameManager gameManager;
        private List<Pool> pools = new List<Pool>();

        public ObjectsPool(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Initialize(List<IUsedPool> poolUsers)
        {
            foreach (var pool in poolUsers)
            {
                pool.CreatePoolEvent += OnCreatePoolEvent;
                pool.PullObjectEvent += OnPullEvent;
                pool.PushObjectEvent += OnPushObjectEvent;
            }
        }

        private int OnCreatePoolEvent(string name, GameObject prefab, int size)
        {
            GameObject poolHolder = new GameObject(name);
            poolHolder.transform.SetParent(gameManager.transform);
            Pool pool = new Pool(name, pools.Count, prefab, size, poolHolder.transform, new Stack<GameObject>());
            pools.Add(pool);
            for (int j = 0; j < size; j++)
            {
                GameObject poolObject = GameObject.Instantiate(prefab, poolHolder.transform);
                poolObject.name = j.ToString();
                poolObject.SetActive(false);
                pool.Stack.Push(poolObject);
            }
            return pools.Count - 1;
        }

        private GameObject OnPullEvent(int poolIndex)
        {
            GameObject pulledObject;
            Pool pool = pools[poolIndex];
            if (pools[poolIndex].Stack.Count > 0)
            {
                pulledObject = pool.Stack.Peek();
                pool.Stack.Pop();
                pulledObject.SetActive(true);
            }
            else
            {
                pulledObject = GameObject.Instantiate(pool.Prefab, pool.PoolHolder.transform);
                pulledObject.name = pool.PoollSize.ToString();
                pool.PoollSize++;
            }
            return pulledObject;
        }

        private void OnPushObjectEvent(int poolIndex, GameObject pushedObject)
        {
            Pool pool = pools[poolIndex];
            pool.Stack.Push(pushedObject);
        }

        [Serializable]
        public class Pool
        {
            [SerializeField]
            private string id;
            [SerializeField]
            private int index;
            [SerializeField]
            private GameObject prefab;
            [SerializeField]
            private int poollSize;
            [SerializeField]
            private Transform poolHolder;
            private Stack<GameObject> stack;

            public Pool(string id, int index, GameObject prefab, int poollSize, Transform poolHolder, Stack<GameObject> stack)
            {
                this.id = id;
                this.index = index;
                this.prefab = prefab;
                this.poollSize = poollSize;
                this.poolHolder = poolHolder;
                this.stack = stack;
            }

            public Stack<GameObject> Stack { get => stack; set => stack = value; }
            public GameObject Prefab { get => prefab; set => prefab = value; }
            public Transform PoolHolder { get => poolHolder; set => poolHolder = value; }
            public int PoollSize { get => poollSize; set => poollSize = value; }
        }
    }
}