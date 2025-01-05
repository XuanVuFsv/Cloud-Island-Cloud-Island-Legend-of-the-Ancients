using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VitsehLand.GameCamera;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Player;
using VitsehLand.Scripts.Ultilities;
using VitsehLand.Scripts.Weapon.Ammo;
using VitsehLand.Scripts.UI.Weapon;
using VitsehLand.GameCamera.Shaking;
using VitsehLand.Scripts.Inventory;
using Nethereum.ABI.EIP712;



#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace VitsehLand.Scripts.Weapon.General
{
    public class ActiveWeapon : MonoBehaviour
    {
        private static ActiveWeapon instance;

        public enum WeaponSlot
        {
            None = -1,
            AttackGun = 0,
            HandGun = 1,
            AxieCollector = 2,
        }

        public enum WeaponAction
        {
            Throw = 0,
            Pickup = 1,
            Switch = 2,
            View = 3
        }

        public MovementController movementController;

        public static WeaponPickup[] equippedWeapon = new WeaponPickup[3];

        public UnityEngine.Animations.Rigging.Rig handIk;
        public Cinemachine.CinemachineVirtualCamera playerCamera;

        public WeaponPickup defaultWeapon0, defaultWeapon1, defaultWeapon2;
        public Transform[] weaponActivateSlots;
        public Transform weaponPivot;
        public Transform rightHandHolder, leftHandHolder;
        public Transform gunCamera;
        public List<WeaponPickup> triggerWeaponList = new List<WeaponPickup>();
        public List<AmmoPickup> triggerAmmoList = new List<AmmoPickup>();

        public float minDistanceToWeapon = 5;
        public int activeWeaponIndex = 3;
        public bool isHoldWeapon = false;

        [SerializeField]
        Animator rigController;
        [SerializeField]
        GunCameraController gunCameraController;
        [SerializeField]
        Transform[] equippedWeaponParent = new Transform[3];

        InputController inputController;
        ShootController shootController;
        Transform droppedWeapon;

        [SerializeField] int nearestWeaponIndex, nearestAmmoIndex;
        [SerializeField] string nearestWeapon, nearestAmmo;

        void MakeInstance()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else instance = this;
        }

        public static ActiveWeapon Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            shootController = GetComponent<ShootController>();
            inputController = GetComponent<InputController>();
            movementController = GetComponent<MovementController>();

            EquipWeapon(WeaponAction.Pickup, defaultWeapon0, true);
            SetupNewWeapon(defaultWeapon0.weaponStat);

            AttachWeapon(defaultWeapon1, weaponActivateSlots[1], 1);
            AttachWeapon(defaultWeapon2, weaponActivateSlots[2], 2);
            //AttachWeapon(defaultWeapon3, weaponActivateSlots[3], 3);

            gunCameraController.SetHasScope(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                transform.position = new Vector3(4.11f, 0.77f, 26.72f);
            }
            //if (Input.GetKeyDown(KeyCode.X))
            //{
            //    //SceneManager.LoadScene("LoadingScene");
            //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //}

            //if (GetNearestWeapon()) nearestWeapon = GetNearestWeapon().gameObject.transform.parent.name;
            //if (GetNearestAmmo()) nearestAmmo = GetNearestAmmo().gameObject.transform.name;

            rigController.SetBool("isHoldWeapon", isHoldWeapon);

            //if (Input.GetKeyDown(KeyCode.KeypadEnter)) Time.timeScale = 1;

            //AmmoPickupDetect();

            #region Drop and Pick Weapon
            //if (Input.GetKeyDown(KeyCode.G))
            //{
            //    if (activeWeaponIndex != 2)
            //    {
            //        DropWeapon(WeaponAction.Throw, activeWeaponIndex);

            //        if (!GetWeapon(0) || !GetWeapon(1))
            //        {
            //            SwitchWeapon(equippedWeapon[2]);
            //        }
            //        else
            //        {
            //            if (activeWeaponIndex == 0) SwitchWeapon(equippedWeapon[1]);
            //            else SwitchWeapon(equippedWeapon[0]);
            //        }
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    PickWeapon();
            //}
            #endregion

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AttackGun || equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AxieCollector)
                {
                    equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().SwitchAmmo(-1);
                }
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AttackGun || equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AxieCollector)
                {
                    equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().SwitchAmmo(1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Debug.Log("1");
                SwitchWeapon(equippedWeapon[0]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //Debug.Log("2");
                SwitchWeapon(equippedWeapon[1]);

            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //Debug.Log("3");
                SwitchWeapon(equippedWeapon[2]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SwitchSlot(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SwitchSlot(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SwitchSlot(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SwitchSlot(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SwitchSlot(8);
            }
        }

        public void SwitchSlot(int number)
        {
            int slotIndex = number - 4;
            if (slotIndex < 0 || slotIndex > 4 || slotIndex == InventoryController.Instance.activeSlotIndex) return;

            if (equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AttackGun || equippedWeapon[activeWeaponIndex].weaponSlot == WeaponSlot.AxieCollector)
            {
                equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().SwitchAmmo(slotIndex - InventoryController.Instance.activeSlotIndex);
            }
        }

        public WeaponPickup GetWeaponPickupByIndex(int index)
        {
            return equippedWeapon[index];
        }

        public WeaponPickup GetActiveWeaponPickup()
        {
            return equippedWeapon[activeWeaponIndex];
        }

        public static WeaponPickup GetWeapon(int index)
        {
            if (index < 0 || index > 2)
            {
                return null;
            }
            return equippedWeapon[index];
        }

        void PickWeapon()
        {
            if (triggerWeaponList.Count == 0) return;

            WeaponPickup pickedWeapon;
            if (triggerWeaponList.Count > 1)
            {
                pickedWeapon = triggerWeaponList[nearestWeaponIndex];
            }
            else
            {
                pickedWeapon = triggerWeaponList[0];
            }

            if (!pickedWeapon.canPickup) return;

            triggerWeaponList.Remove(pickedWeapon);

            int pickedWeaponSlot = (int)pickedWeapon.weaponSlot;
            bool isExistWeaponSlot = GetWeapon(pickedWeaponSlot);
            DropWeapon(WeaponAction.Pickup, pickedWeaponSlot);
            EquipWeapon(WeaponAction.Pickup, pickedWeapon, false, isExistWeaponSlot);
            SetupNewWeapon(pickedWeapon.weaponStat);
        }

        void SwitchWeapon(WeaponPickup activateWeapon)
        {
            //only run this funtion when player swich to a weapon not null and different weapon type
            if (activateWeapon)
            {
                if (activeWeaponIndex == (int)activateWeapon.weaponSlot) return;
            }
            else return;

            if (shootController.isReloading)
            {
                //Debug.Log("Switch");
                shootController.AttachMagazine();
                shootController.StopAllCoroutines();
                shootController.StopCoroutine("Reload");
            }
            else shootController.ResetMagazine();

            shootController.UpdateEndedReloadStats(false);

            if (isHoldWeapon) DropWeapon(WeaponAction.Switch, (int)equippedWeapon[activeWeaponIndex].weaponSlot);

            EquipWeapon(WeaponAction.Switch, activateWeapon, true);
            SetupNewWeapon(activateWeapon.weaponStat);

            rigController.SetInteger("weaponIndex", activeWeaponIndex);
            gunCameraController.SetHasScope(activateWeapon.GetComponent<CollectableObjectStatController>().collectableObjectStat.zoomType == CollectableObjectStat.ZoomType.HasScope);
            GetComponent<MovementController>().SetMultiplierSpeed(1);
        }

        public void EquipWeapon(WeaponAction action, WeaponPickup newWeapon, bool runAnimation, bool isExistWeaponSlot = false)
        {
            int weaponSlotIndex = (int)newWeapon.weaponSlot;

            equippedWeapon[weaponSlotIndex] = newWeapon;
            equippedWeaponParent[weaponSlotIndex] = equippedWeapon[weaponSlotIndex].transform.parent;

            bool sameWeaponSlot = activeWeaponIndex == weaponSlotIndex;

            if (action == WeaponAction.Switch || runAnimation || isExistWeaponSlot)
            {
                //newWeapon.GetComponent<CameraShake>().Subscribe();
                shootController.currentWeaponStatsController = newWeapon.GetComponent<WeaponStatsController>();
                shootController.currentWeaponStatsController.currentCollectableObjectStatController = newWeapon.GetComponent<CollectableObjectStatController>();

                //shootController.currentWeaponStatsController.UpdateAmmoUI();
                //shootController.currentWeaponStatsController.refferedToShootController = true;

                if (action == WeaponAction.Switch || runAnimation)
                {
                    activeWeaponIndex = weaponSlotIndex;
                    newWeapon.transform.parent.gameObject.SetActive(true);
                }

                if (sameWeaponSlot || runAnimation)
                {
                    equippedWeaponParent[weaponSlotIndex].parent = weaponPivot;
                    SetWeaponAnimation();
                }
            }

            if (activeWeaponIndex == weaponSlotIndex)
            {
                newWeapon.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                newWeapon.transform.parent.gameObject.SetActive(false);
            }

            if (action == WeaponAction.Pickup)
            {
                //Set current weapon and its parent
                newWeapon.GetComponent<CameraShake>().SetUpWeaponRecoilForNewWeapon(playerCamera, rigController);

                //UI, Layer, Physic
                //if (newWeapon.weaponUI) newWeapon.weaponUI.gameObject.SetActive(false);
                SetupUtilities.SetLayers(equippedWeaponParent[weaponSlotIndex], "Local Player", "Local Player", "Effect");
                //equippedWeaponParent[weaponSlotIndex].gameObject.GetComponent<Rigidbody>().isKinematic = true;

                //Set parent for weapon, change noParent value and run animation
                newWeapon.noParent = false;
            }

            equippedWeaponParent[weaponSlotIndex].parent = weaponActivateSlots[weaponSlotIndex];
            equippedWeaponParent[weaponSlotIndex].localPosition = new Vector3(0, -0.5f, 0.5f);
            isHoldWeapon = true;
            //Debug.Log(isHoldWeapon);

            //if (activeWeaponIndex == 2) holdWeapon.Notify(false);
            //else holdWeapon.Notify(true);
        }

        public void DropWeapon(WeaponAction action, int weaponSlotIndex)
        {
            bool isExsitWeaponSlot = true;

            //Don't run this function when don't hold any weapon or throw a null weapon
            if (!equippedWeapon[weaponSlotIndex])
            {
                isExsitWeaponSlot = false;
            }
            else
            {
                //equippedWeapon[weaponSlotIndex].GetComponent<CameraShake>().UnSubscribe();
                //shootController.currentWeaponStatsController.currentCropStatsController = null;
                //shootController.currentWeaponStatsController.ofActiveAmmo = false;
                shootController.currentWeaponStatsController = null;

                //Switch
                if (action == WeaponAction.Switch)
                {
                    rigController.SetFloat("multiplier", -1);
                    rigController.Play("Base Layer.Equip " + equippedWeapon[activeWeaponIndex].weaponStat.name, 0, 0f);
                    rigController.SetFloat("multiplier", 1);
                    equippedWeaponParent[activeWeaponIndex].gameObject.SetActive(false);
                    return;
                }
                //Throw or Pickup
                //Set parent for weapon and change noParent value, layers            
                equippedWeapon[weaponSlotIndex].GetComponent<CameraShake>().rigController = null;
                equippedWeapon[weaponSlotIndex].noParent = true;

                droppedWeapon = equippedWeaponParent[weaponSlotIndex].transform;

                SetupUtilities.SetLayers(equippedWeaponParent[weaponSlotIndex], "Ignore Player", "Only Player", null);

                //equippedWeaponParent[weaponSlotIndex].GetComponent<Rigidbody>().isKinematic = false;
                //equippedWeaponParent[weaponSlotIndex].GetComponent<Rigidbody>().AddForce(transform.forward * 5, ForceMode.Impulse);

                isHoldWeapon = false;
                //Debug.Log(isHoldWeapon);

                //if (!equippedWeapon[weaponSlotIndex].weaponUI)
                //{
                //    equippedWeapon[weaponSlotIndex].CreateWeaponUI();
                //}
            }

            if (action == WeaponAction.Throw || action == WeaponAction.Pickup && isExsitWeaponSlot)
            {
                droppedWeapon.gameObject.SetActive(true);
                droppedWeapon.parent = null;
                equippedWeaponParent[weaponSlotIndex] = null;
                equippedWeapon[weaponSlotIndex] = null;
            }
        }

        public void AttachWeapon(WeaponPickup attachedWeapon, Transform attachedWeaponParent, int attachedWeaponSlotIndex)
        {
            attachedWeapon.transform.parent.gameObject.SetActive(false);
            SetupUtilities.SetLayers(attachedWeapon.transform.parent, "Local Player", "Local Player", "Effect");
            attachedWeapon.noParent = false;

            attachedWeapon.GetComponent<CameraShake>().rigController = rigController;
            equippedWeapon[attachedWeaponSlotIndex] = attachedWeapon;
            equippedWeaponParent[attachedWeaponSlotIndex] = attachedWeapon.transform.parent;

            StartCoroutine(SetWeaponParent(attachedWeapon, attachedWeaponParent));
            //Debug.Log("OnStart " + attachedWeapon.transform.parent.name);
            attachedWeapon.GetComponent<WeaponStatsController>().OnStart();
        }

        public void SetupNewWeapon(WeaponStat weaponStat)
        {
            WeaponSystemUI.Instance.weaponNameText.text = GetCurrentWeaponName();

            equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().SetupWeaponStats(weaponStat);

            shootController.magazineObject = equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().magazineObject;
            //shootController.magazineObject = equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().magazineObject;
            shootController.raycastWeapon = equippedWeapon[activeWeaponIndex].GetComponent<RaycastWeapon>();

            //if (activeWeaponIndex == 2 || activeWeaponIndex == 3) gunCameraController.SetHoldWeaponAnimation(false, (int)weaponStat.weaponSlot);
            //else gunCameraController.SetHoldWeaponAnimation(true, (int)weaponStat.weaponSlot);
            gunCameraController.SetHoldWeaponAnimation(true, (int)weaponStat.weaponSlot);
            //equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().ofActiveAmmo = equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().weaponSlot == InventoryController.Instance.GetCurrentItem().cropStat.weaponSlot;
        }

        void SetWeaponAnimation()
        {
            Vector3 droppedWeaponPosition = Vector3.zero;
            if (droppedWeapon)
            {
                droppedWeaponPosition = droppedWeapon.position;
                rigController.Rebind();
                droppedWeapon.position = droppedWeaponPosition;
            }
            else rigController.Rebind();

            rigController.Play("Base Layer.Equip " + equippedWeapon[activeWeaponIndex].weaponStat.name, 0, 0f);
        }

        IEnumerator SetWeaponParent(WeaponPickup weapon, Transform weaponParent)
        {
            float timeToSetupWeaponParent = 1;

            if (defaultWeapon2.GetComponent<WeaponPickup>().weaponStat.weaponAnimation)
            {
                timeToSetupWeaponParent = defaultWeapon2.GetComponent<WeaponPickup>().weaponStat.weaponAnimation.length;
            }
            yield return new WaitForSeconds(timeToSetupWeaponParent);
            //Debug.Log(weapon);
            //Debug.Log(weapon.transform.parent);
            //Debug.Log(weapon.transform.parent.parent);

            weapon.transform.parent.parent = weaponParent;
        }

        void OnTriggerEnter(Collider other)
        {
            //WeaponPickup weaponPickup = other.GetComponent<WeaponPickup>();
            //if (weaponPickup)
            //{
            //    if(!triggerWeaponList.Contains(weaponPickup)) triggerWeaponList.Add(weaponPickup);
            //}

            //AmmoPickup ammoPickup = other.GetComponent<AmmoPickup>();
            //if (ammoPickup && !ammoPickup.hasParent)
            //{
            //    if (!triggerAmmoList.Contains(ammoPickup)) triggerAmmoList.Add(ammoPickup);
            //}
        }

        void OnTriggerExit(Collider other)
        {
            if (triggerWeaponList.Contains(other.GetComponent<WeaponPickup>()))
            {
                //minDistanceToWeapon = 5;
                triggerWeaponList.Remove(other.GetComponent<WeaponPickup>());
            }

            if (triggerAmmoList.Contains(other.GetComponent<AmmoPickup>()))
            {
                //minDistanceToWeapon = 5;
                triggerAmmoList.Remove(other.GetComponent<AmmoPickup>());
            }
        }

        public WeaponPickup GetNearestWeapon()
        {
            WeaponPickup weapon = null;
            float min = 5;
            for (int i = 0; i < triggerWeaponList.Count; i++)
            {
                float distance = Vector3.Distance(triggerWeaponList[i].transform.parent.position, transform.position);
                if (distance <= min/* && triggerWeaponList[i].inWeaponViewport*/)
                {
                    min = distance;
                    weapon = triggerWeaponList[i];
                    nearestWeaponIndex = i;
                }
            }
            return weapon;
        }

        public AmmoPickup GetNearestAmmo()
        {
            AmmoPickup ammo = null;
            float min = 5;
            for (int i = 0; i < triggerAmmoList.Count; i++)
            {
                float distance = Vector3.Distance(triggerAmmoList[i].transform.position, transform.position);
                if (distance <= min)
                {
                    min = distance;
                    ammo = triggerAmmoList[i];
                    nearestAmmoIndex = i;
                }
            }
            return ammo;
        }

        public string GetCurrentWeaponName()
        {
            return equippedWeapon[activeWeaponIndex].weaponStat.name;
        }
#if UNITY_EDITOR
        [ContextMenu("Save Weapon Pose")]
        void SaveWeaponPose()
        {
            GameObjectRecorder recorder = new GameObjectRecorder(transform.GetChild(0).gameObject);
            recorder.BindComponentsOfType<Transform>(weaponPivot.gameObject, false);
            recorder.BindComponentsOfType<Transform>(equippedWeaponParent[activeWeaponIndex].gameObject, false);
            recorder.BindComponentsOfType<Transform>(gunCamera.gameObject, false);
            recorder.BindComponentsOfType<Transform>(leftHandHolder.gameObject, false);
            recorder.BindComponentsOfType<Transform>(rightHandHolder.gameObject, false);
            recorder.TakeSnapshot(0.0f);
            recorder.SaveToClip(equippedWeapon[activeWeaponIndex].GetComponent<WeaponStatsController>().weaponAnimation);
        }
#endif
    }
}