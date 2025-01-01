using UnityEngine;
using VitsehLand.GameCamera.Shaking;
using VitsehLand.Scripts.Pattern.Observer;
using VitsehLand.Scripts.Player;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Weapon.Ammo;

namespace VitsehLand.Scripts.Weapon.Primary
{
    [System.Serializable]
    public class ShootingInputData
    {
        public ShootingInputData(ShootController _shootController, CollectableObjectStat.ShootingHandleType _shootingHandleType, CollectableObjectStatController _collectableObjectStatController, AudioSource _source, Transform _raycastOrigin, Transform _fpsCameraTransform, GameEvent _hitEvent, CameraShake _cameraShake, Transform _bulletSpawnPoint, float _lifeTime, int _layerMask)
        {
            shootController = _shootController;
            shootingHandleType = _shootingHandleType;
            collectableObjectStatController = _collectableObjectStatController;
            source = _source;
            raycastOrigin = _raycastOrigin;
            fpsCameraTransform = _fpsCameraTransform;
            hitEvent = _hitEvent;
            cameraShake = _cameraShake;
            bulletSpawnPoint = _bulletSpawnPoint;
            lifeTime = _lifeTime;
            layerMask = _layerMask;
        }

        public ShootController shootController;

        public CollectableObjectStat.ShootingHandleType shootingHandleType;
        public CollectableObjectStatController collectableObjectStatController;
        public AudioSource source;
        public Transform raycastOrigin;
        public Transform fpsCameraTransform;
        public GameEvent hitEvent;
        public CameraShake cameraShake;
        public Transform bulletSpawnPoint;
        public float lifeTime;
        public int layerMask;
    }
}