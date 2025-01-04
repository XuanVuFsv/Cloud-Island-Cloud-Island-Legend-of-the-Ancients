using UnityEngine;
using VitsehLand.Scripts.Farming.Resource;

namespace VitsehLand.Scripts.Farming.ObjectState.Resource
{
    public class ResourceUnStored : ObjectState
    {
        public ResourceUnStored(PowerContainer power) : base(power)
        {

        }

        public ResourceUnStored(NaturalResource resource) : base(resource)
        {

        }

        public ResourceUnStored(FarmingEssence essence) : base(essence)
        {

        }

        public ResourceUnStored(FarmingProduct product) : base(product)
        {

        }

        public override void Start()
        {
            //Debug.Log("Start Seed");

            objectMachine.rigid = objectMachine.GetComponent<Rigidbody>();

            objectMachine.suckableCollider.isTrigger = false;
            objectMachine.rigid.isKinematic = false;
            objectMachine.rigid.useGravity = true;

            objectMachine.gameObject.SetActive(true);
        }

        public override void End()
        {
            if (objectMachine.state == null) return;
            //Debug.Log("End Seed");
        }
    }
}