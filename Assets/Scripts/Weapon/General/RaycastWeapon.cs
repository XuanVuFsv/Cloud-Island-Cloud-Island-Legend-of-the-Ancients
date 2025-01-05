using Cysharp.Threading.Tasks;
using UnityEngine;
using VitsehLand.Assets.Scripts.GameCamera.Shaking;
using VitsehLand.Assets.Scripts.Weapon.Collector;
using VitsehLand.GameCamera.Shaking;
using VitsehLand.Scripts.Pattern.Observer;
using VitsehLand.Scripts.Pattern.Strategy;
using VitsehLand.Scripts.Player;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Weapon.Ammo;
using VitsehLand.Scripts.Weapon.HandGun;
using VitsehLand.Scripts.Weapon.Primary;

namespace VitsehLand.Scripts.Weapon.General
{

    // The RaycastWeapon as Context defines the interface of interest to clients.
    public class RaycastWeapon : MonoBehaviour
    {
        public IWeaponStrategy weaponHandler;

        public CollectableObjectStatController collectableObjectStatController;
        public ShootController shootController;
        CameraShake cameraShake;
        GunCameraShake gunCameraShake;

        public GameObject bulletPrefab, magazineObject;

        public GameEvent hitEvent;

        public Transform raycastOrigin;
        public Transform fpsCameraTransform;

        public Transform bulletSpawnPoint, casingSpawnPoint;

        public ParticleSystem hitEffectPrefab;
        public ParticleSystem muzzleFlash;

        //public AnimationClip weaponAnimation;

        WeaponStat weaponStat;
        int layerMask;

        private void Start()
        {
            cameraShake = GetComponent<CameraShake>();
            weaponStat = GetComponent<WeaponStatsController>().weaponStat;

            fpsCameraTransform = Camera.main.transform;
            layerMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Ignore Player") | 1 << LayerMask.NameToLayer("Only Player"));

            if (gameObject.activeInHierarchy) SetAsWeaponStrategy();
        }

        public async UniTaskVoid SetAsInputData(LayerMask _layerMask)
        {
            await UniTask.WaitUntil(() => collectableObjectStatController.collectableObjectStat != null);

            var attackingCropStat = (collectableObjectStatController.collectableObjectStat.GetCollectableObjectStatComponent<AttackingCropStat>() as AttackingCropStat);
            var lifetime = attackingCropStat == null ? 0f : attackingCropStat.GetLifeTime();

            ShootingInputData shootingInputData = new ShootingInputData(shootController, 
                collectableObjectStatController.collectableObjectStat.shootingHandleType, 
                collectableObjectStatController, 
                shootController.source, 
                raycastOrigin, 
                fpsCameraTransform, 
                hitEvent, 
                cameraShake,
                bulletSpawnPoint, 
                lifetime, 
                _layerMask);

            weaponHandler.SetInputData(shootingInputData);
        }

        public void SetAsWeaponStrategy()
        {
            if (gameObject.activeInHierarchy == false) return;

            if (GetComponent<ShootingHandler>())
            {
                weaponHandler = GetComponent<ShootingHandler>();
                var _setInput = SetAsInputData(layerMask);
            }
            else if (GetComponent<ActionHandler>())
            {
                weaponHandler = GetComponent<ActionHandler>();
                var setInput = SetAsInputData(layerMask);
            }
            else
            {
                weaponHandler = gameObject.GetComponent<CollectHandler>();
                var setInput = SetAsInputData(layerMask = 1 << LayerMask.NameToLayer("Suckable"));
            }
        }

        /// <summary>
        /// Run this method to shoot and select what type of bullet <see cref="CollectableObjectStat.ShootingHandleType"/> and how to bullet interact to other objects
        /// </summary>
        public void HandleLeftMouseClick()
        {
            if (muzzleFlash) muzzleFlash.Emit(1);
            weaponHandler.HandleLeftMouseClick();
        }

        /// <summary>
        /// Run this method to aim. An ammo type can aim or not and how a gun aim with different ammo will depend on their ammo type. Check this:
        /// <see cref="CropStats.ZoomType"></see>
        /// </summary>
        public void HandleRightMouseClick()
        {
            weaponHandler.HandleRightMouseClick();
        }

        public void StopFiring()
        {
            cameraShake.ResetRecoil();
        }

        public CameraShake GetCameraShake()
        {
            return cameraShake;
        }

        public GunCameraShake GetGunCameraShake()
        {
            return gunCameraShake;
        }

        private void OnDrawGizmos()
        {
            if (Application.isEditor) return;
            Gizmos.color = Color.green;
            Vector3 direction = fpsCameraTransform.forward;

            if (collectableObjectStatController.collectableObjectStat.zoomType == CollectableObjectStat.ZoomType.HasScope)
            {
                Vector3 localDirection = Vector3.forward + cameraShake.GetCurrentPatternVector();
                direction = fpsCameraTransform.TransformDirection(localDirection).normalized;
            }
            //foreach (Vector3 pattern in currentShootingMechanic.bulletDirectionPattern)
            //{
            //    Vector3 localDirection = Vector3.forward + pattern;
            //    Vector3 direction = fpsCameraTransform.TransformDirection(localDirection).normalized;

            //    Gizmos.DrawRay(raycastOrigin.position, direction);
            //}

            Gizmos.DrawRay(raycastOrigin.position, direction);
            Gizmos.DrawLine(raycastOrigin.position, raycastOrigin.position + direction * collectableObjectStatController.range);
        }

        private void OnEnable()
        {

        }
    }
}