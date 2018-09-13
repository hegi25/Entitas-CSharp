using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Entitas {

    public static class EntitasCache {

        static readonly ObjectCache _cache = new ObjectCache();

        public static List<IComponent> GetIComponentList() { return _cache.Get<List<IComponent>>(); }
        public static void PushIComponentList(List<IComponent> list) { list.Clear(); _cache.Push(list); }

        public static List<int> GetIntList() { return _cache.Get<List<int>>(); }
        public static void PushIntList(List<int> list) { list.Clear(); _cache.Push(list); }

        public static HashSet<int> GetIntHashSet() { return _cache.Get<HashSet<int>>(); }
        public static void PushIntHashSet(HashSet<int> hashSet) { hashSet.Clear(); _cache.Push(hashSet); }

        public static void Reset() {
            _cache.Reset();
        }
    }
    public class ObjectPool<T>
    {
        private readonly Func<T> _factoryMethod;
        private readonly Action<T> _resetMethod;
        private readonly ConcurrentStack<T> _objectPool;

        public ObjectPool(Func<T> factoryMethod, Action<T> resetMethod = null)
        {
            this._factoryMethod = factoryMethod;
            this._resetMethod = resetMethod;
            this._objectPool = new ConcurrentStack<T>();
        }

        public T Get()
        {
            if (this._objectPool.TryPop(out var pool))
                return pool;
            return this._factoryMethod();
        }

        public void Push(T obj)
        {
            if (this._resetMethod != null)
                this._resetMethod(obj);
            this._objectPool.Push(obj);
        }
    }
    
    public class ObjectCache
    {
        private readonly ConcurrentDictionary<Type, object> _objectPools;

        public ObjectCache()
        {
            this._objectPools = new ConcurrentDictionary<Type, object>();
        }

        public ObjectPool<T> GetObjectPool<T>() where T : new()
        {
            Type key = typeof (T);
            object obj;
            if (!this._objectPools.TryGetValue(key, out obj))
            {
                obj = (object) new ObjectPool<T>((Func<T>) (() => Activator.CreateInstance<T>()), (Action<T>) null);
                this._objectPools.TryAdd(key, obj);
            }
            return (ObjectPool<T>) obj;
        }

        public T Get<T>() where T : new()
        {
            return this.GetObjectPool<T>().Get();
        }

        public void Push<T>(T obj) where T : new()
        {
            this.GetObjectPool<T>().Push(obj);
        }

        public void RegisterCustomObjectPool<T>(ObjectPool<T> objectPool)
        {
            this._objectPools.TryAdd(typeof (T), (object) objectPool);
        }

        public void Reset()
        {
            this._objectPools.Clear();
        }
    }
}
