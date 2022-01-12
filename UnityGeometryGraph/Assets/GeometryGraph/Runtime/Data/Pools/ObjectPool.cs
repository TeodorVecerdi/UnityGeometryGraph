using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    [Serializable]
    public abstract class ObjectPool<T> {
        [SerializeField] private float growthFactor;

        private Queue<T> pool = new();
        private int size;

        private int initialPoolSize;

        public int PooledCount => pool.Count;
        public int FreeCount => size - pool.Count;

        protected ObjectPool(int initialPoolSize, float growthFactor) {
            this.initialPoolSize = initialPoolSize;
            this.growthFactor = growthFactor;
            
            size = 0;
            Allocate(initialPoolSize);
        }

        protected abstract T CreatePooled();
        protected abstract void DestroyPooled(T obj);

        protected virtual void OnGet(T pooled) {
        }

        protected virtual void OnReturn(T pooled) {
        }

        public T Get() {
            if (pool == null) {
                pool = new Queue<T>();
                size = 0;
                Allocate(initialPoolSize);
            }

            if (pool.Count == 0) {
                Grow();
            }

            T item = pool.Dequeue();
            OnGet(item);
            return item;
        }

        public void Return(T item) {
            OnReturn(item);
            pool.Enqueue(item);
        }

        public void Cleanup() {
            while (pool?.Count > 0) {
                DestroyPooled(pool.Dequeue());
            }
        }

        private void Allocate(int amount) {
            size += amount;
            for (int i = 0; i < amount; i++) {
                pool.Enqueue(CreatePooled());
            }
        }

        private void Grow() {
            if (growthFactor <= 1.0f) growthFactor = 1.5f;
            int newItems = Mathf.CeilToInt(size * growthFactor) - size;
            Allocate(newItems);
        }
    }
}