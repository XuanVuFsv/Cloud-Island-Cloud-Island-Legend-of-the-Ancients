using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace VitsehLand.Scripts.Stats
{
    [System.Serializable]
    public class AttackingCropStat : CollectableObjectStatComponent
    {
        public enum ShootingHandleType
        {
            None = -1,
            Raycast = 0,
            InstantiateBullet = 1,
            Both = 2
        }

        public enum BulletEffectComponent
        {
            None = -1,
            PlaceHole = 0,
            InstantiateTrail = 1,
            Both = 2
        }

        public enum ZoomType
        {
            NoZoom = 0,
            CanZoom = 1,
            HasScope = 2
        }

        public ShootingHandleType shootingHandleType;
        [Tooltip("Type of effect will be instantiated upon shooting")]
        public BulletEffectComponent bulletEffectComponent;
        [InfoBox("HasSCope option will be used for AttackingCrop and will function as a sniper gun mechanic")]
        public ZoomType zoomType;

        [Tooltip("Instantiate this object when shooting if ShootingHandleType is InstantiateBullet or Both")]
        public GameObject bulletObject;
        public GameObject trailTracer;
        public ParticleSystem hitEffectPrefab;

        public int force;
        public int range;
        public float fireRate;
        [SerializeField]
        private float lifeTime; 

        public int damage;
        public int damageDealToSpecificEnemy;

        public float reloadSpeed;

        public int amplitudeGainImpulse;
        public float multiplierRecoilOnAim;
        public float multiplierForAmmo;
        public List<Vector2> recoilPattern;


        [Tooltip("Number of bullet or raycast will be instantiated when player shoot. Example 1 for rifle type(berry), sniper type(carambola) and 5 for shotgun type(tomato)")]
        public int bulletCount = 1;
        [InfoBox("A list of directions that the bullet will take after being fired. This is used to simulate the deviation in bullet trajectory caused by recoil, similar to the behavior of sniper bullets. The directions are randomized with each shot.")]
        public List<Vector3> bulletDirectionPattern;

        public override float GetLifeTime()
        {
            return shootingHandleType == ShootingHandleType.InstantiateBullet ? lifeTime : 0;
        }
    }
}