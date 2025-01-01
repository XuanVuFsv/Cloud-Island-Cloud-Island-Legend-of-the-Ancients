using System.Collections.Generic;
using UnityEngine;
using VitsehLand.Scripts.Enemy;
using VitsehLand.Scripts.Pattern.Pooling;
using VitsehLand.Scripts.Pattern.Strategy;
using VitsehLand.Scripts.Player;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Ultilities;
using VitsehLand.Scripts.Weapon.Ammo;
using VitsehLand.Scripts.Weapon.General;

namespace VitsehLand.Scripts.Weapon.Primary
{
    public class ShootingHandler : MonoBehaviour, IPrimaryWeaponStrategy
    {
        public ShootingInputData shootingInputData;
        public RaycastWeapon raycastWeapon;
        public WeaponStatsController weaponStatsController;
        RaycastHit hit;

        private void Start()
        {
            raycastWeapon = GetComponent<RaycastWeapon>();
            weaponStatsController = GetComponent<WeaponStatsController>();
        }

        public void SetInputData(object _inputData)
        {
            shootingInputData = _inputData as ShootingInputData;
        }

        public ShootingInputData GetShootingInputData()
        {
            return shootingInputData;
        }

        public void HandleLeftMouseClick()
        {
            if (weaponStatsController.itemInInventory.collectableObjectStat.weaponSlot != ActiveWeapon.WeaponSlot.AttackGun) return;
            ShootingHandle();
            shootingInputData.source.PlayOneShot(shootingInputData.source.clip);
        }

        public void HandleRightMouseClick()
        {
            if (shootingInputData.collectableObjectStatController.zoomType == CollectableObjectStat.ZoomType.NoZoom) return;
            PlayAimAnimation();
        }

        public void ShootingHandle()
        {
            if (shootingInputData.collectableObjectStatController.collectableObjectStat.weaponSlot != ActiveWeapon.WeaponSlot.AttackGun) return;
            shootingInputData.shootController.ApplyAttackAnimation();
            if (shootingInputData.shootingHandleType == CollectableObjectStat.ShootingHandleType.Raycast)
            {
                //Debug.Log("Shoot");
                RaycastHandle();
            }
            else if (shootingInputData.shootingHandleType == CollectableObjectStat.ShootingHandleType.InstantiateBullet)
            {
                InstantiateBulletHandle();
            }
        }

        void RaycastHandle()
        {
            if (shootingInputData.collectableObjectStatController.bulletCount == 1)
            {
                //Enemy enemy;
                shootingInputData.cameraShake.GenerateRecoil(shootingInputData.collectableObjectStatController.zoomType);
                //Debug.Log("Shoot");

                if (Physics.Raycast(shootingInputData.raycastOrigin.position, shootingInputData.fpsCameraTransform.forward, out hit, shootingInputData.collectableObjectStatController.range, shootingInputData.layerMask))
                {
                    //Debug.Log(hit.transform);
                    //hitEffectPrefab.transform.position = hit.point;
                    //hitEffectPrefab.transform.forward = hit.normal;
                    //hitEffectPrefab.Emit(5);

                    Debug.Log("Shooting " + hit.transform.name + " Pool: " + "Pool_" + shootingInputData.collectableObjectStatController.collectableObjectStat.collectableObjectName + "_Setup");
                    PoolingManager.Instance.Get("Pool_" + shootingInputData.collectableObjectStatController.collectableObjectStat.name + "_Setup");
                    //PoolingManager.Instance.Get("Pool_Tomato_Setup");

                    shootingInputData.hitEvent.Notify(hit);
                    //currentHitObject = hit.collider.name;
                    //enemy = hit.transform.GetComponent<Enemy>();
                    //    if (enemy != null)
                    //    {
                    //        Destroy(enemy.gameObject);
                    //    }

                    //Tracer here
                    //Damage Handle here

                    #region Minigame Test
                    //tracer.transform.position = hit.point;
                    if (hit.transform.gameObject.tag == "Wall")
                    {
                        GameObject wall = hit.transform.gameObject;
                        //GemManager.Instance.AddGem(weaponStatsController.currentCropStatsController.collectableObjectStat.gemEarnWhenKillEnemy);
                        //Debug.Log(weaponStatsController.currentCropStatsController.collectableObjectStat.gemEarnWhenKillEnemy);
                        WallSpawner.Instance.DestroyWall(wall.GetComponent<WallBehaviour>().index);
                    }

                    if (hit.transform.gameObject.tag == "Enemy")
                    {
                        EnemyController enemy = hit.transform.GetComponent<EnemyController>();
                        //GemManager.Instance.AddGem(enemy.enemyStats.gemRewardForPlayerWhenKilled);
                        enemy.TakeDamage(weaponStatsController.currentCollectableObjectStatController.collectableObjectStat.damage);
                        //Debug.Log(weaponStatsController.currentCropStatsController.collectableObjectStat.gemEarnWhenKillEnemy);
                    }
                    #endregion
                }
                //else tracer.transform.position += _fpsCameraTransform.forward * range;
                weaponStatsController.UseAmmo(1);
            }
            else
            {
                //Debug.Log("Shoot");
                List<RaycastHit> raycastHits = new List<RaycastHit>();

                //int i = 0;
                bool destroyedObstacle = false;
                //Enemy enemy;

                foreach (Vector3 pattern in shootingInputData.collectableObjectStatController.collectableObjectStat.bulletDirectionPattern)
                {
                    Vector3 localDirection = Vector3.forward + pattern;
                    Vector3 direction = shootingInputData.fpsCameraTransform.TransformDirection(localDirection).normalized;

                    shootingInputData.cameraShake.GenerateRecoil(shootingInputData.collectableObjectStatController.zoomType);
                    if (Physics.Raycast(shootingInputData.raycastOrigin.position, direction, out hit, shootingInputData.collectableObjectStatController.range, shootingInputData.layerMask))
                    {
                        raycastHits.Add(hit);
                        MyDebug.Log("Shooting " + hit.transform.name + " Pool: " + "Pool_" + shootingInputData.collectableObjectStatController.collectableObjectStat.collectableObjectName + "_Setup");
                        PoolingManager.Instance.Get("Pool_" + shootingInputData.collectableObjectStatController.collectableObjectStat.name + "_Setup");
                        //PoolingManager.Instance.Get("Pool_Tomato_Setup");

                        shootingInputData.hitEvent.Notify(hit);
                        //enemy = hit.transform.GetComponent<Enemy>();
                        #region Minigame Test
                        //tracer.transform.position = hit.point;
                        if (!destroyedObstacle && hit.transform.gameObject.tag == "Wall")
                        {
                            GameObject wall = hit.transform.gameObject;
                            //GemManager.Instance.AddGem(weaponStatsController.currentCollectableObjectStatController.collectableObjectStat.gemEarnWhenKillEnemy);
                            //Debug.Log(weaponStatsController.currentCropStatsController.collectableObjectStat.gemEarnWhenKillEnemy);
                            WallSpawner.Instance.DestroyWall(wall.GetComponent<WallBehaviour>().index);
                            destroyedObstacle = true;
                        }

                        if (!destroyedObstacle && hit.transform.gameObject.tag == "Enemy")
                        {
                            EnemyController enemy = hit.transform.GetComponent<EnemyController>();
                            //GemManager.Instance.AddGem(enemy.enemyStats.gemRewardForPlayerWhenKilled);
                            enemy.TakeDamage(weaponStatsController.currentCollectableObjectStatController.collectableObjectStat.damage);
                            //Debug.Log(weaponStatsController.currentCropStatsController.collectableObjectStat.gemEarnWhenKillEnemy);
                        }
                        #endregion
                    }
                }

                weaponStatsController.UseAmmo(1);
            }
        }

        void InstantiateBulletHandle()
        {
            Vector3 direction = shootingInputData.fpsCameraTransform.forward;

            if (shootingInputData.collectableObjectStatController.collectableObjectStat.zoomType == CollectableObjectStat.ZoomType.HasScope)
            {
                Debug.Log("Shoot");
                Vector3 localDirection = Vector3.forward + shootingInputData.cameraShake.GetCurrentPatternVector();
                direction = shootingInputData.fpsCameraTransform.TransformDirection(localDirection).normalized;
            }

            BulletBehaviour newBullet = Instantiate(shootingInputData.collectableObjectStatController.collectableObjectStat.bulletObject, shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<BulletBehaviour>();
            newBullet.timeLife = shootingInputData.lifeTime;
            newBullet.TriggerBullet(shootingInputData.collectableObjectStatController.collectableObjectStat.collectableObjectName, shootingInputData.collectableObjectStatController.force, direction);
            shootingInputData.cameraShake.GenerateRecoil(shootingInputData.collectableObjectStatController.zoomType);

            weaponStatsController.UseAmmo(1);
        }

        public void PlayAimAnimation()
        {
            MyDebug.LogCaller();

            ShootController shootController = shootingInputData.shootController;
            if (!shootController.rigController) return;

            shootController.inAim = !shootController.inAim;
            shootController.aimEvent.Notify(shootController.inAim);

            if (shootController.inAim) shootController.aimEvent.Notify(shootingInputData.collectableObjectStatController.multiplierRecoilOnAim);

            shootController.rigController.SetBool("inAim", shootController.inAim);
        }
    }
}