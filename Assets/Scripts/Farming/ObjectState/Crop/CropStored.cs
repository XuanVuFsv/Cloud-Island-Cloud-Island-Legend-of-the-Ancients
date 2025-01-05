using UnityEngine;
using VitsehLand.Assets.Scripts.Weapon.Collector;

namespace VitsehLand.Scripts.Farming.ObjectState.Crop
{
    public class CropStored : ObjectState
    {
        public CropStored(global::Crop plant) : base(plant)
        {

        }

        public override void Start()
        {
            //Debug.Log("Start Stored");
            if ((objectMachine as global::Crop).orginalPlant)
            {
                (objectMachine as global::Crop).orginalPlant.wholePlants.Remove(objectMachine as global::Crop);
            }

            (objectMachine as global::Crop).isFromHarvesting = false;

            //GemManager.Instance.AddGem((objectMachine as Plant).ammoStats.gemEarnWhenHaverst);

            (objectMachine as global::Crop).startDestroyedTimer = false;
            //(objectMachine as Plant).StopCoroutine("DestroyTimer");
            (objectMachine as global::Crop).inCrafting = false;

            (objectMachine as global::Crop).gameObject.transform.position = CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position;

            (objectMachine as global::Crop).rigid.velocity = Vector3.zero;
            (objectMachine as global::Crop).suckableCollider.isTrigger = false;
            (objectMachine as global::Crop).rigid.useGravity = true;

            (objectMachine as global::Crop).gameObject.SetActive(false);
            (objectMachine as global::Crop).seedOuterEffect.SetActive(true);
            (objectMachine as global::Crop).wholeOuterEffect.SetActive(false);

            (objectMachine as global::Crop).transform.parent = null;
        }

        public override void End()
        {
            if ((objectMachine as global::Crop).state == null) return;
            //Debug.Log("End Stored");
        }
    }
}