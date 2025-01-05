using UnityEngine;
using VitsehLand.Scripts.Manager;

namespace VitsehLand.Scripts.Farming.ObjectState.Crop
{
    public class CropWhole : ObjectState
    {
        public CropWhole(global::Crop plant) : base(plant)
        {

        }

        public override void Start()
        {
            //Debug.Log("Start Whole");
            GemManager.Instance.AddGem((objectMachine as global::Crop).collectableObjectStat.gemEarnWhenHaverst);

            (objectMachine as global::Crop).startDestroyedTimer = false;
            //StopCoroutine("DestroyTimer");
            (objectMachine as global::Crop).inCrafting = false;

            (objectMachine as global::Crop).rigid = (objectMachine as global::Crop).GetComponent<Rigidbody>();

            (objectMachine as global::Crop).suckableCollider.isTrigger = true;
            (objectMachine as global::Crop).rigid.isKinematic = false;
            (objectMachine as global::Crop).rigid.useGravity = false;

            (objectMachine as global::Crop).growingBody.SetActive(false);
            (objectMachine as global::Crop).gameObject.SetActive(true);
            (objectMachine as global::Crop).defaultModelPlant.SetActive(true);

            (objectMachine as global::Crop).isFromHarvesting = true;

            SetWholePlantEffect();
        }

        public override void End()
        {
            if ((objectMachine as global::Crop).state == null) return;

            Debug.Log("Harvest");
            //(objectMachine as Plant).plantData.orginalBody = null;
            (objectMachine as global::Crop).transform.parent = null;

            (objectMachine as global::Crop).suckableCollider.isTrigger = false;
            (objectMachine as global::Crop).rigid.useGravity = true;

            (objectMachine as global::Crop).orginalPlant.wholePlantCount--;
            if ((objectMachine as global::Crop).orginalPlant.wholePlantCount == 0)
            {
                (objectMachine as global::Crop).orginalPlant.state.ResetCropStats();
            }
            (objectMachine as global::Crop).onTree = false;
        }

        public void SetWholePlantEffect()
        {
            (objectMachine as global::Crop).seedOuterEffect.SetActive(false);
            (objectMachine as global::Crop).wholeOuterEffect.SetActive(true);
        }
    }
}