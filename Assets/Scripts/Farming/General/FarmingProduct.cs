using UnityEngine;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Farming.ObjectState.Resource;

namespace VitsehLand.Scripts.Farming.Resource
{
    public class FarmingProduct : Suckable
    {
        // Start is called before the first frame update
        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            suckableCollider = GetComponent<Collider>();

            SetState(new ResourceUnStored(this));
        }

        public override void ChangeToStored()
        {
            EndCurrentState();
            SetState(new ResourceStored(this));
        }

        public override void ChangeToUnStored()
        {
            EndCurrentState();
            SetState(new ResourceUnStored(this));
        }
    }
}