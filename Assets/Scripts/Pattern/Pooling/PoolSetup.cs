using System;
using System.ComponentModel;
using UnityEngine;
using VitsehLand.Scripts.Pattern.Observer;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Weapon.Ammo;

namespace VitsehLand.Scripts.Pattern.Pooling
{

    //public class ObjectInPoolInitCount
    //{
    //    public GameObject prefab;
    //    public int count;
    //}

    public class PoolSetup : GameObserver, IPoolSetup
    {
        static readonly float DEFAULT_MAX_POOL_SIZE_MULTIPLIER = 1.25f;

        [Tooltip("Check this to mark pool has multiple object")]
        public bool isSameObject;
        [Tooltip("This prefab will be used if isSameObject true")]
        public GameObject prefab;
        //[Tooltip("This list will be used if isSameObject false")]
        //public List<ObjectInPoolInitCount> multipleDifferentObjectList = new List<ObjectInPoolInitCount>();

        public string poolManagerName;
        [SerializeField] GameEvent gameEvent;

        public Pool<ObjectInPool> pool;
        public ObjectInPool currentObject;
        CollectableObjectStatComponent stat = null;

        public float spawnInterval;
        public int amountInstantiatedWhenCalled = 1;
        [Tooltip("When using multipleDifferentObjectList, init pool size is predetermined which is number of all object instantiated base on ObjectInPoolInitCount at init. Max pool size is flexible")]
        [SerializeField] int initPoolSize, maxPoolSize;

        private void Start()
        {

        }

        /// <summary>
        /// Initialize a pool with a Pool Setup instance that has been created (in the case where the Pool Setup is assigned to a GameObject in the scene) or already has valid data set.
        /// </summary>
        public IPool InitPool()
        {
            if (prefab == null)
            {
                Debug.LogError("No prefab assigned");
                return null;
            }

            Type type = null;

            if (type == null) type = prefab.GetComponent<ObjectInPool>().GetType();

            poolManagerName = poolManagerName != "" ? poolManagerName : gameObject.name;

            if (type == typeof(BulletHoleBehaviour))
            {
                stat = (prefab.GetComponent(type) as BulletHoleBehaviour).collectableObjectStat.GetCollectableObjectStatComponent<AttackingCropStat>();

                spawnInterval = 1 / (stat as AttackingCropStat).fireRate;
                amountInstantiatedWhenCalled = (stat as AttackingCropStat).bulletCount;
            }

            //foreach (CollectableObjectStatComponent component in prefab.GetComponent<BulletHoleBehaviour>().collectableObjectStat.components)
            //{
            //    if (component is AttackingCropStat) spawnInterval = 1 / (component as AttackingCropStat).fireRate;
            //}

            if (spawnInterval != 0)
            {
                initPoolSize = ((int)((prefab.GetComponent<ObjectInPool>().lifeTime + stat?.GetLifeTime()) / spawnInterval) + 1) * amountInstantiatedWhenCalled;
            }
            maxPoolSize = (int)(initPoolSize * DEFAULT_MAX_POOL_SIZE_MULTIPLIER) + 1;

            if (spawnInterval == 0) Debug.LogWarning("You should check spawnInterval. If you target it equal to 0. Just ignore this warning!");

            pool = new Pool<ObjectInPool>(new PrefabFactory<ObjectInPool>(prefab, transform), initPoolSize);
            return pool;
        }

        /// <exception cref="ArgumentException">Thrown when init pool size less than or equal to 0 or max pool size less than init pool size.</exception>
        public IPool InitPool(string poolManagerName, int initPoolSize, GameObject prefab, GameEvent gameEvent, int maxPoolSize = 0, int amountInstantiatedWhenCalled = 1, bool hasSpawnInterval = true)
        {
            if (initPoolSize < 0 || maxPoolSize < 0)
            {
                throw new ArgumentException("Init pool size must be greater than 0.", nameof(initPoolSize));
            }

            if (maxPoolSize < 0)
            {
                throw new ArgumentException("Max pool size must be greater than or equal to init pool size.", nameof(maxPoolSize));
            }

            //if (type == null) type = prefab.GetComponent<ObjectInPool>().GetType();

            this.poolManagerName = poolManagerName != "" ? poolManagerName : gameObject.name;
            this.prefab = prefab;
            this.gameEvent = gameEvent;
            this.amountInstantiatedWhenCalled = amountInstantiatedWhenCalled;
            this.initPoolSize = initPoolSize * amountInstantiatedWhenCalled;
            this.maxPoolSize = maxPoolSize > 0 ? maxPoolSize : (int)(initPoolSize * DEFAULT_MAX_POOL_SIZE_MULTIPLIER) + 1;

            //The time between spawns will be equal to the lifetime of an object divided by the minimum number of objects that need to be spawned - initPoolSize
            //This ensures that when a call is made, there is always an object that has completed its lifetime and is ready for use
            spawnInterval = hasSpawnInterval ? prefab.GetComponent<ObjectInPool>().lifeTime / initPoolSize : 0;

            if (spawnInterval == 0) Debug.LogWarning("You should check spawnInterval. If you target it equal to 0. Just ignore this warning");

            pool = new Pool<ObjectInPool>(new PrefabFactory<ObjectInPool>(prefab, transform), initPoolSize);
            return pool;
        }

        /// <summary>
        /// Initializes a pool with the spawn interval greater than 0. Assign init pool size or max pool size if you want to set spawn interval equal to 0.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when spawnInterval is less than or equal to 0.</exception>
        public IPool InitPool(string poolManagerName, float spawnInterval, GameObject prefab, GameEvent gameEvent, int amountInstantiatedWhenCalled = 1, float maxPoolSizeMultiplier = 0)
        {
            if (spawnInterval <= 0)
            {
                throw new ArgumentException("Spawn interval must be greater than 0. If you want to set spawn interval equal to 0, you need set init pool size and max pool size first", nameof(spawnInterval));
            }

            //if (type == null) type = prefab.GetComponent<ObjectInPool>().GetType();

            this.poolManagerName = poolManagerName != "" ? poolManagerName : gameObject.name;
            this.prefab = prefab;
            this.gameEvent = gameEvent;

            this.spawnInterval = spawnInterval;
            this.amountInstantiatedWhenCalled = amountInstantiatedWhenCalled;

            this.initPoolSize = ((int)(prefab.GetComponent<ObjectInPool>().lifeTime / spawnInterval) + 1) * amountInstantiatedWhenCalled;
            this.maxPoolSize = (int)(maxPoolSizeMultiplier == 0 ? (initPoolSize * DEFAULT_MAX_POOL_SIZE_MULTIPLIER) : initPoolSize * maxPoolSizeMultiplier) + 1;

            pool = new Pool<ObjectInPool>(new PrefabFactory<ObjectInPool>(prefab, transform), initPoolSize);
            return pool;
        }

        //public IPool InitPool(string poolManagerName, int maxPoolSize, List<ObjectInPoolInitCount> list, GameEvent gameEvent)
        //{
        //    this.multipleDifferentObjectList = list;
        //    this.poolManagerName = poolManagerName;
        //    this.gameEvent = gameEvent;

        //    int count = 0;
        //    foreach (ObjectInPoolInitCount infor in list)
        //    {
        //        count += infor.count;
        //    }
        //    this.initPoolSize = count;
        //    this.maxPoolSize = maxPoolSize;
        //    pool = new Pool<ObjectInPool>(new PrefabFactory<ObjectInPool>(prefab, transform), initPoolSize);
        //    return pool;
        //}

        public int GetMaxPoolSize() { return maxPoolSize; }

        public int GetPoolSize() { return pool.poolSize; }

        public string GetName() { return poolManagerName; }

        public void Get()
        {
            //Debug.Log("Get in PoolSetup");
            //Debug.Log(currentObject);
            currentObject = pool.Get();
        }

        public void Release()
        {
            pool.Release();
        }

        public void Reset()
        {

        }

        public void Dispose()
        {
            foreach (Transform poolObject in transform)
            {
                poolObject.GetComponent<ObjectInPool>().Dispose();
                //poolObject.GetComponent<BulletHoleBehaviour>().Dispose();
            }
            pool.Dispose();
        }

        public override void Execute(IGameEvent gEvent, RaycastHit hit)
        {
            //if (!currentObject)
            //{
            //    RemoveGameEvent();
            //    return;
            //}
            //MyDebug.Log("Call OnUsed");
            currentObject?.OnUsed(hit); // fix null in memory
        }


        public override void Execute(IGameEvent gEvent, Vector3 point, Vector3 normal)
        {
            //if (!currentObject)
            //{
            //    RemoveGameEvent();
            //    return;
            //}
            //Debug.Log(this);
            //Debug.Log(gameObject.transform.parent);
            //MyDebug.Log("Call OnUsed");
            currentObject?.OnUsed(point, normal); // fix null in memory
        }

        public void AddGameEvent()
        {
            AddGameEventToObserver(gameEvent);
        }

        public void RemoveGameEvent()
        {
            RemoveGameEventFromObserver(gameEvent);
        }

        void OnDestroy()
        {
            RemoveGameEvent();
        }

        void OnDisable()
        {
            RemoveGameEvent();
        }
    }
}