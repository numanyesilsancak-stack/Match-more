using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public sealed class Pool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _root;
        private readonly Stack<T> _stack = new Stack<T>(128);

        public Pool(T prefab, Transform root, int prewarm)
        {
            _prefab = prefab;
            _root = root;
            for (int i = 0; i < prewarm; i++)
            {
                var item = Object.Instantiate(_prefab, _root);
                item.gameObject.SetActive(false);
                _stack.Push(item);
            }
        }

        public T Get()
        {
            if (_stack.Count > 0)
            {
                var item = _stack.Pop();
                item.gameObject.SetActive(true);
                return item;
            }
            return Object.Instantiate(_prefab, _root);
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(_root, false);
            _stack.Push(item);
        }
    }
}
