using UnityEngine;
using VitsehLand.Assets.Scripts.Interactive;

namespace VitsehLand.Scripts.UI.Interactive
{
    public class SimpleInteractiveUI : MonoBehaviour
    {
        public ActivateBehaviour activateBehaviour;
        public GameObject guildDisplayUI;

        public Transform interactiveElement;

        [SerializeField]
        private bool enableUI = false;
        [SerializeField]
        private bool canInteract = false;
        [SerializeField]
        private bool isLookAtUI = false;

        [Tooltip("Limit angle when player look at monitor screen. Calculating by two vector: vector from center of monitor to player camera and normal vector of monitor screen")]
        public float limitAngleToInteractBuildButton;

        private void Update()
        {
            if (canInteract && isLookAtUI && Input.GetKeyDown(KeyCode.E))
            {
                guildDisplayUI.SetActive(false);
                enableUI = false;
                activateBehaviour.ExecuteActivateAction();
            }
        }

        public bool CheckLookAtMonitorScreen(Vector3 lookAtVector, Vector3 monitorScreenVector, float limitAngle)
        {
            float dotValue = Vector3.Dot(lookAtVector, monitorScreenVector);
            float angle = Mathf.Acos(dotValue / (lookAtVector.magnitude * monitorScreenVector.magnitude)) * Mathf.Rad2Deg;

            if (dotValue > 0)
            {
                if (angle <= limitAngle) return true;
                return false;
            }
            return false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            canInteract = true;
            isLookAtUI = CheckLookAtMonitorScreen(Camera.main.transform.forward, interactiveElement.transform.position - Camera.main.transform.position, limitAngleToInteractBuildButton);
            if (!isLookAtUI)
            {
                enableUI = false;
                guildDisplayUI.SetActive(false);
            }
            if (isLookAtUI && !enableUI)
            {
                enableUI = true;
                guildDisplayUI.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            canInteract = false;
            isLookAtUI = false;
            enableUI = false;
            guildDisplayUI.SetActive(false);
        }

        /// <summary> Zoom in and zoom out element animation. </summary>
        //public void PressElementAnimation(RectTransform rect, Vector3 endScale, float cycleTime)
        //{
        //    interactiveElementTween = rect.DOScale(endScale, cycleTime).SetLoops(1, LoopType.Restart).SetEase(Ease.OutBack).OnComplete(() => {
        //        EndZoomInOutAnimation(interactiveElement, new Vector3(startScale, startScale, 1), LoopType.Yoyo, animationSpeed);
        //    });
        //}

        /// <summary> Zoom in and zoom out element animation. </summary>
        //public void StartZoomInOutAnimation(RectTransform rect, Vector3 endScale, LoopType loopType, float cycleTime)
        //{
        //    interactiveElementTween = rect.DOScale(endScale, cycleTime).SetLoops(1, loopType).SetEase(Ease.Linear);
        //}

        /// <summary> End Zoom in and zoom out element animation. </summary>
        //public void EndZoomInOutAnimation(RectTransform rect, Vector3 endScale, LoopType loopType, float cycleTime)
        //{
        //    interactiveElementTween = rect.DOScale(endScale, cycleTime).SetLoops(1, loopType).SetEase(Ease.Linear);
        //}
    }
}