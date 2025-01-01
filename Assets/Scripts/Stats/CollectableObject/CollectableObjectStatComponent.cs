using VitsehLand.Scripts.Farming.General;

namespace VitsehLand.Scripts.Stats
{
    [System.Serializable]
    public class CollectableObjectStatComponent
    {
        public virtual float GetLifeTime()
        {
            return 0;
        }
    }
}