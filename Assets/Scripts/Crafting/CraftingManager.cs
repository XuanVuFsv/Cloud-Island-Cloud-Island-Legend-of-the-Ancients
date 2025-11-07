using UnityEngine;
using VitsehLand.Scripts.Pattern.Singleton;

namespace VitsehLand.Scripts.Crafting
{
    [System.Serializable]
    public class CraftingManager : Singleton<CraftingManager>
    {
        public CraftingPresenter currentActivePresenter;

        public bool IsPresenterActive(CraftingPresenter presenter)
        {
            return currentActivePresenter == presenter;
        }

        public bool HasActivePresenter => currentActivePresenter != null;

        public void ActivatePresenter(CraftingPresenter presenter)
        {
            if (presenter == null)
            {
                Debug.LogWarning("Trying to activate null presenter");
                return;
            }

            // Already active
            //if (IsPresenterActive(presenter)) return;

            // Deactivate current
            if (currentActivePresenter != null)
            {
                currentActivePresenter.DeactivateView();
            }

            // Set new active
            currentActivePresenter = presenter;
            presenter.ActivateView();
        }

        public void DeactivatePresenter(CraftingPresenter presenter)
        {
            Debug.Log("Deactivating presenter: " + presenter.name + " " + IsPresenterActive(presenter));
            if (IsPresenterActive(presenter))
            {
                currentActivePresenter = null;
                presenter.DeactivateView();
            }
        }
    }
}