using UnityEngine;
using VitsehLand.Scripts.Pattern.Pooling;
using VitsehLand.Scripts.Stats;

namespace VitsehLand.Scripts.Weapon.Ammo
{
    public class BulletHoleBehaviour : ObjectInPool
    {
        [Tooltip("The bullet hole is spawned or activated based on the actions of the object containing this stat")]
        public CollectableObjectStat collectableObjectStat;
        private void Start()
        {
            gameObject.SetActive(false);

            //if (collectableObjectStat != null) amountInstantiatedWhenCalled = (collectableObjectStat.GetCollectableObjectStatComponent<AttackingCropStat>() as AttackingCropStat).bulletCount;
        }

        public override void OnUsed(RaycastHit hit)
        {
            //MyDebug.Log(gameObject.name);
            gameObject.SetActive(true);

            transform.position = hit.point;
            transform.forward = hit.normal;
            transform.rotation = Quaternion.LookRotation(hit.normal);
            Invoke(nameof(Release), lifeTime);
        }

        public override void OnUsed(Vector3 point, Vector3 normal)
        {
            //MyDebug.Log(gameObject.name);
            gameObject.SetActive(true);

            transform.position = point;
            transform.forward = normal;
            transform.rotation = Quaternion.LookRotation(normal);
            Invoke(nameof(Release), lifeTime);
        }
    }
}