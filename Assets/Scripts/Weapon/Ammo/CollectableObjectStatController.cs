using UnityEngine;
using VitsehLand.GameCamera.Shaking;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Ultilities;
using VitsehLand.Scripts.Weapon.General;

namespace VitsehLand.Scripts.Weapon.Ammo
{
    [System.Serializable]
    public class CollectableObjectStatController : MonoBehaviour
    {
        public CollectableObjectStat collectableObjectStat;

        //public bool canAttack;

        public CollectableObjectStat.ZoomType zoomType;

        //public int dropOffDsitance;

        //public int decreseDamageRate;
        //public int damageHead;
        //public int damageBody;
        //public int damageArmsLegs;
        public int damage;
        public int damageDealToSpecificEnemy;
        public int range;
        public int force;
        public int amplitudeGainImpulse;

        public float fireRate;
        public float reloadSpeed;
        public float multiplierRecoilOnAim;
        public float multiplierForAmmo;

        public bool hasAssignCropData = false;

        public RaycastWeapon raycastWeapon;
        public WeaponStatsController weaponStatsController;
        public ParticleSystem hitEffectPrefab;
        public GameObject trailTracer;
        public GameObject bulletObject;

        public int bulletCount;
        public int ammoHolding;

        public WaitForSeconds reloadTimer;

        [SerializeField]
        CameraShake cameraShake;

        // Start is called before the first frame update
        void Awake()
        {

        }

        private void Start()
        {
            cameraShake = gameObject.GetComponent<CameraShake>();
            raycastWeapon = GetComponent<RaycastWeapon>();
            weaponStatsController = GetComponent<WeaponStatsController>();

            if (collectableObjectStat)
            {
                AssignCollectableObjecttData();
            }
        }

        void SetCropStats(CollectableObjectStat newCollectableObjectStat)
        {
            collectableObjectStat = newCollectableObjectStat;
        }

        public void AssignCollectableObjecttData()
        {
            MyDebug.Log(collectableObjectStat.collectableObjectName);

            //if (hasAssignCropData) return;
            //GetComponent<RaycastWeapon>().currentShootingMechanic = collectableObjectStat.shootingMechanic;
            //canAttack = collectableObjectStat.canAttack;

            zoomType = collectableObjectStat.zoomType;

            //dropOffDsitance = collectableObjectStat.dropOffDsitance;
            //decreseDamageRate = collectableObjectStat.decreseDamageRate;

            //damageHead = collectableObjectStat.damageHead;
            //damageBody = collectableObjectStat.damageBody;
            //damageArmsLegs = collectableObjectStat.damageArmsLegs;
            damage = collectableObjectStat.damage;
            damageDealToSpecificEnemy = collectableObjectStat.damageDealToSpecificEnemy;
            range = collectableObjectStat.range;
            force = collectableObjectStat.force;
            amplitudeGainImpulse = collectableObjectStat.amplitudeGainImpulse;
            //Debug.Log("amplitudeGainImpulse: " + amplitudeGainImpulse);
            //amplitudeGainImpulse = (collectableObjectStat.GetCollectableObjectStatComponent<AttackingCropStat>() as AttackingCropStat).amplitudeGainImpulse;
            MyDebug.Log("amplitudeGainImpulse: " + amplitudeGainImpulse);
            //MyDebug.LogCaller();

            multiplierRecoilOnAim = collectableObjectStat.multiplierRecoilOnAim;
            multiplierForAmmo = collectableObjectStat.multiplierForAmmo;

            fireRate = collectableObjectStat.fireRate;
            reloadSpeed = collectableObjectStat.reloadSpeed;

            bulletCount = collectableObjectStat.bulletCount;
            ammoHolding = collectableObjectStat.ammmoHolding;

            reloadTimer = new WaitForSeconds(reloadSpeed * 0);

            hitEffectPrefab = collectableObjectStat.hitEffectPrefab;
            trailTracer = collectableObjectStat.trailTracer;
            bulletObject = collectableObjectStat.bulletObject;

            cameraShake.AssignRecoilPattern(collectableObjectStat.recoildPattern);

            weaponStatsController.maxQuantityStored = collectableObjectStat.maxQuantityStored;
            raycastWeapon.SetAsWeaponStrategy();
        }
    }
}