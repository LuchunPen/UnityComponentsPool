//Copyright(c) Luchunpen, 2019.

using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace Nano3.Unity
{
    public interface IPoolableComponent
    {
        void ItemInitialize();
        void ItemClear();
    }

    public class ComponentPool<T> : MonoBehaviour
        where T : Component
    {
        [SerializeField] protected T _template;
        public virtual T Template
        {
            get { return _template; }
            set
            {
                if (value != _template) { Clear(); }
                _template = value;
            }
        }

        [SerializeField] protected Transform _poolRoot;
        [SerializeField] protected int _maxCount = 100;

        protected Queue<T> _pool = new Queue<T>();

        protected virtual void Awake()
        {
            if (_poolRoot == null)
            {
                _poolRoot = this.transform;
            }
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        public int Count
        {
            get { return _pool.Count; }
        }

        public virtual T Pop()
        {
            T result;
            if (_pool.Count > 0)
            {
                result = _pool.Dequeue();
                result.gameObject.SetActive(true);
            }
            else
            {
                result = CreateNew();
            }

            IPoolableComponent pi = result as IPoolableComponent;
            if (pi != null)
            {
                pi.ItemInitialize();
            }

            return result;
        }

        protected virtual T CreateNew()
        {
            T result = Instantiate(_template);
            return result;
        }

        public virtual T2 Pop<T2>() where T2 : T
        {
            T2 p_template = _template as T2;
            if (p_template == null) { return null; }

            T result = Pop();
            return result as T2;
        }


        public virtual void Release(T item)
        {
            if (item == null) { return; }

            if (_pool.Count >= _maxCount)
            {
                Destroy(item.gameObject);
                return;
            }

            IPoolableComponent pi = item as IPoolableComponent;
            if (pi != null) { pi.ItemClear(); }

            item.gameObject.SetActive(false);
            item.transform.SetParent(_poolRoot);

            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;

            _pool.Enqueue(item);
        }

        public virtual void Clear()
        {
            foreach (T item in _pool)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _pool.Clear();
        }
    }
}
