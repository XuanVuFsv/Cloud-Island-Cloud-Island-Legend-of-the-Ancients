using UnityEngine;
using VitsehLand.Assets.Scripts.Interactive;
using VitsehLand.Assets.Scripts.UI.Crafting;
using VitsehLand.Scripts.Farming.Resource;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.UI.DisplayItem;
using VitsehLand.Scripts.Ultilities;

namespace VitsehLand.Scripts.Crafting
{
    [System.Serializable]
    public class CraftingPresenter : ActivateBehaviour
    {
        public CraftingModel model;
        public CraftingView view;

        public Transform productPos;
        public PowerManager powerManager;

        void Awake()
        {
            model.SetupInitData();
        }

        void Start()
        {
            SetupInitViewElements();

            view.slider.value = 1;
            model.currentQuantity = 1;
            view.queueQuantityDisplay.text = "You can use " + model.queueQuantity.ToString() + " slot in queue";

            view.RegisterListener(UpdateQuantity);
            view.RegisterListener(Craft);

            for (int i = 0; i < view.itemUIs.Count; i++)
            {
                view.itemUIs[i].RegisterListener(OnClickProduct);
            }

            for (int i = 0; i < model.craftQueueHandlers.Count; i++)
            {
                model.craftQueueHandlers[i].RegisterListener(CompleteCraft);
            }
        }

        public override void ExecuteActivateAction()
        {
            if (view.isActive) return;

            view.Show(true);

            view.UpdateMaterialStorage(model.unlockedStorageSlot, model.itemStorageDict);
            view.ReLoadQuantityMaterialsRequired(model.GetCurrentRecipe(), 
                model.GetQuantityByMaterialOfRecipe(model.GetCurrentRecipe()),
                model.currentQuantity);
        }

        public void SetupInitViewElements()
        {
            int i = 0;
            foreach (var productRecipe in model.productRecipes)
            {
                //Debug.Log("Product Recipe Index: " + i);

                view.itemUIs[i].collectableObjectStat = productRecipe.Value.collectableObjectStat;
                view.itemUIs[i].SetItemUI(productRecipe.Value.collectableObjectStat);
                i++;
            }    

            for (i = 0; i < view.storageCardWrappers.Count; i++)
            {
                view.storageCardWrappers[i] = view.storageParent.GetChild(i).GetComponent<MaterialCardWrapper>();
            }

            ItemUI firstItemUI = view.itemUIs[0];
            view.ShowCurrentItemInformation(firstItemUI.collectableObjectStat);
            view.ShowRecipe();

            //Debug.Log(firstItemUI);
            //Debug.Log(model.GetQuantityByMaterialOfRecipe(firstItemUI.collectableObjectStat.recipe));
            //Debug.Log("Current Quantity: " + model.currentQuantity);

            view.LoadMaterialsRequired(firstItemUI.collectableObjectStat.recipe,
                model.GetQuantityByMaterialOfRecipe(firstItemUI.collectableObjectStat.recipe),
                model.currentQuantity);

            view.UpdateMaterialStorage(model.unlockedStorageSlot, model.itemStorageDict);
            view.body.gameObject.SetActive(false);

            MyDebug.Log("Setup Done");
        }

        public void OnClickProduct(CollectableObjectStat collectableObjectStat)
        {
            Debug.Log("Click" + " " + collectableObjectStat.collectableObjectName);

            if (collectableObjectStat == null || collectableObjectStat.collectableObjectName == "Null") return;

            model.currentRecipeNameId = collectableObjectStat.collectableObjectName;

            view.ShowCurrentItemInformation(collectableObjectStat);
            view.LoadMaterialsRequired(collectableObjectStat.recipe,
                model.GetQuantityByMaterialOfRecipe(collectableObjectStat.recipe),
                model.currentQuantity);
        }

        public void UpdateItemStorageDatas()
        {
            foreach (MaterialCardWrapper card in view.materialCardWrappers)
            {
                card.quantity -= card.requiredQuantity;
                model.SetItemStorage(card.collectableObjectStat.collectableObjectName, card.quantity);
                
                if (card.quantity == 0)
                {
                    model.RemoveItemStorage(card.collectableObjectStat.collectableObjectName);
                }
            }

            view.ReLoadQuantityMaterialsRequired(model.GetCurrentRecipe(),
                model.GetQuantityByMaterialOfRecipe(model.GetCurrentRecipe()),
                model.currentQuantity);
            view.UpdateMaterialStorage(model.unlockedStorageSlot, model.itemStorageDict);
        }

        public void Craft()
        {
            if (!CheckCraftCondition())
            {
                Debug.Log("Not enough material or Energy");
                return;
            }

            StartCraft();
        }

        public bool CheckCraftCondition()
        {
            //foreach (MaterialCardWrapper card in view.materialCardWrappers)
            //{
            //    if (card.requiredQuantity > card.quantity)
            //    {
            //        return false;
            //    }
            //}

            if (view.materialCardWrappers[0].collectableObjectStat.name == view.materialCardWrappers[view.materialCardWrappers.Count - 1].collectableObjectStat.name)
            {
                if (view.materialCardWrappers[0].requiredQuantity * 2 > view.materialCardWrappers[view.materialCardWrappers.Count - 1].quantity)
                {
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < view.materialCardWrappers.Count; i++)
                {
                    if (view.materialCardWrappers[i].requiredQuantity > view.materialCardWrappers[i].quantity)
                    {
                        return false;
                    }
                }
            }

            if (powerManager.currentPower < 10 * model.currentQuantity)
            {
                StartCoroutine(view.ShowWarningNotEnoughPower());
                return false;
            }

            return true;
        }

        public void StartCraft()
        {
            if (model.queueActiveQuantity < model.queueQuantity)
            {
                MyDebug.Log("Start craft a " + model.currentQuantity.ToString() + " " + model.GetCurrentRecipe().name);
                
                int index = model.FindFirstCraftSlotReady();
                if (index >= 0)
                {
                    UpdateItemStorageDatas();
                    model.craftQueueHandlers[index].UIContainer.SetActive(true);

                    model.craftQueueHandlers[index].collectableObjectStat = model.GetCurrentRecipe().collectableObjectStat;
                    model.craftQueueHandlers[index].product = model.GetCurrentRecipe().product;

                    int totalTime = model.currentQuantity * (int)model.GetCurrentRecipe().collectableObjectStat.totalProducingTime;
                    model.craftQueueHandlers[index].Craft(totalTime, productPos);
                    model.queueActiveQuantity++;

                    view.VFX.SetActive(true);
                    powerManager.UsePower(model.currentQuantity * 10);
                }
                else
                {
                    Debug.Log("Something Wrong");
                }
            }
            else
            {
                StartCoroutine(view.ShowWarning());
                Debug.Log("Full Slot");
            }
        }

        void CompleteCraft()
        {
            model.queueActiveQuantity--;
            view.VFX.SetActive(true);
        }

        public bool AddItemStorage(CollectableObjectStat collectableObjectStat, int quantity)
        {
            return model.AddItemStorage(collectableObjectStat, quantity);
        }

        public void UpdateQuantity(int value, CraftingView.QuanityChangedActionType actionType)
        {
            if (actionType == CraftingView.QuanityChangedActionType.Button)
            {
                int quantity = model.currentQuantity + value;
                if (quantity > model.maxQuantity || quantity <= 0) return;

                model.currentQuantity = quantity;

                view.slider.value = model.currentQuantity;
                view.quantityTitle.text = "Quantity: " + model.currentQuantity.ToString();
                view.ReLoadQuantityMaterialsRequired(model.GetCurrentRecipe(),
                    model.GetQuantityByMaterialOfRecipe(model.GetCurrentRecipe()),
                    model.currentQuantity);
            }
            else
            {
                model.currentQuantity = value;
                view.quantityTitle.text = "Quantity: " + model.currentQuantity.ToString();

                view.ReLoadQuantityMaterialsRequired(model.GetCurrentRecipe(),
                    model.GetQuantityByMaterialOfRecipe(model.GetCurrentRecipe()),
                    model.currentQuantity);
            }
        }
    }
}