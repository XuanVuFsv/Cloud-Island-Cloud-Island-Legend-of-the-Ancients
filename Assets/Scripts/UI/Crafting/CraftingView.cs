using System;
using System.Collections;
using System.Collections.Generic;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VitsehLand.Scripts.Crafting;
using VitsehLand.Scripts.Inventory;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.UI.DisplayItem;

namespace VitsehLand.Assets.Scripts.UI.Crafting
{
    [System.Serializable]
    public class CraftingView : MonoBehaviour
    {
        [Header("Main elements")]
        public List<ItemUI> itemUIs = new List<ItemUI>();

        public TextMeshProUGUI currentItemName;
        public TextMeshProUGUI type;
        public TextMeshProUGUI description;
        public TextMeshProUGUI queueQuantityDisplay;

        public GameObject body;

        public bool isActive = false;
        public bool cursorAvaiable = false;

        [Header("Navigation components")]
        public GameObject products;
        public GameObject interaction;
        public GameObject queueDisplay;
        public GameObject materials;
        public GameObject information;
        public Button closeButton;

        [Header("Crafting elements")]
        public Image currentProductImage;
        public Slider slider;

        public TextMeshProUGUI quantityTitle;
        public TextMeshProUGUI cost;
        public TextMeshProUGUI time;

        public GameObject warningFullQueue;
        public GameObject warningNotEnoughPower;
        public GameObject VFX;

        public List<MaterialCardWrapper> materialCardWrappers = new List<MaterialCardWrapper>();

        [Header("Storage components")]
        public Transform storageParent;
        public List<MaterialCardWrapper> storageCardWrappers = new List<MaterialCardWrapper>();

        public enum QuanityChangedActionType
        {
            Button = 0,
            Slider = 1
        }

        // Only one presenter can listen at a time
        public event Action<int, QuanityChangedActionType> OnQuantityChanged = delegate { };
        public event Action OnCrafted = delegate { };
        public event Action OnClosed = delegate { };

        #region UI Display & Control

        public void Show(bool show)
        {
            body.SetActive(show);
            ShowRecipe();
            isActive = show;

            if (!cursorAvaiable)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                cursorAvaiable = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursorAvaiable = false;
            }
        }

        public void ShowCurrentItemInformation(CollectableObjectStat cropStat)
        {
            currentItemName.text = cropStat.collectableObjectName;
            type.text = cropStat.filteredType.ToString() + " - Level " + cropStat.requiredLevel.ToString();
            description.text = cropStat.description;
            currentProductImage.sprite = cropStat.icon;

            cost.text = "Cost: " + cropStat.cost.ToString();
            time.text = "Time: " + cropStat.totalProducingTime.ToString() + "s";
        }

        public void CloseCraftingUI()
        {
            //CraftingManager.Instance.DeactivateCurrentPresenter();

            slider.value = 1;
            OnQuantityChanged?.Invoke(1, QuanityChangedActionType.Slider);
            OnClosed?.Invoke();

            if (cursorAvaiable && isActive)
            {
                Show(false);
            }
        }
        #endregion

        #region Main Action Components
        public void ShowRecipe()
        {
            products.SetActive(true);
            interaction.SetActive(true);
            queueDisplay.SetActive(false);
            materials.SetActive(false);
            information.SetActive(true);
        }

        public void ShowQueue()
        {
            products.SetActive(false);
            interaction.SetActive(false);
            queueDisplay.SetActive(true);
            materials.SetActive(false);
            information.SetActive(false);
        }

        public void ShowMaterials()
        {
            products.SetActive(false);
            interaction.SetActive(false);
            queueDisplay.SetActive(false);
            materials.SetActive(true);
            information.SetActive(false);
        }
        #endregion

        #region Single Listener

        /// <summary>
        /// Set quantity listener - replaces any existing listener
        /// </summary>
        public void SetQuantityListener(Action<int, QuanityChangedActionType> listener)
        {
            OnQuantityChanged = listener;
        }

        /// <summary>
        /// Set craft listener - replaces any existing listener
        /// </summary>
        public void SetCraftListener(Action listener)
        {
            OnCrafted = listener;
        }

        /// <summary>
        /// Set close listener - replaces any existing listener
        /// </summary>
        /// <param name="listener"></param>
        public void SetCloseListener(Action listener)
        {
            OnClosed = listener;
        }

        /// <summary>
        /// Clear all listeners when machine becomes inactive
        /// </summary>
        public void ClearListeners()
        {
            OnQuantityChanged = null;
            OnCrafted = null;
            OnClosed = null;
        }
        #endregion

        #region Modified UI Events Handlers
        public void PlusQuantity()
        {
            OnQuantityChanged?.Invoke(1, QuanityChangedActionType.Button);
        }

        public void MinusQuantity()
        {
           OnQuantityChanged?.Invoke(-1, QuanityChangedActionType.Button);
        }

        public void UpdateSliderValue()
        {
            OnQuantityChanged?.Invoke((int)slider.value, QuanityChangedActionType.Slider);
        }

        public void Craft()
        {
            OnCrafted?.Invoke();
        }

        #endregion

        #region Material Management
        public void LoadMaterialsRequired(RecipeData recipeData, List<int> quantityMaterials, int quantity)
        {
            //Debug.Log("Load " + recipeData.collectableObjectStat.collectableObjectName);
            //Debug.Log("materialCardWrappers.Count: " + materialCardWrappers.Count);

            for (int i = 0; i < materialCardWrappers.Count; i++)
            {
                materialCardWrappers[i].gameObject.SetActive(true);
                materialCardWrappers[i].collectableObjectStat = recipeData.items[i];
                materialCardWrappers[i].image.sprite = recipeData.items[i].icon;

                materialCardWrappers[i].quantity = quantityMaterials[i];
                materialCardWrappers[i].requiredQuantity = recipeData.ammountPerSlots[i]
                    * quantity;

                materialCardWrappers[i].quantityText.text = materialCardWrappers[i].requiredQuantity.ToString()
                + "/" + materialCardWrappers[i].quantity.ToString();

                if (materialCardWrappers[0].collectableObjectStat.name == materialCardWrappers[2].collectableObjectStat.name)
                {
                    if (materialCardWrappers[i].requiredQuantity * 2 > materialCardWrappers[i].quantity)
                    {
                        materialCardWrappers[i].quantityText.color = Color.red;
                    }
                    else
                    {
                        materialCardWrappers[i].quantityText.color = Color.white;
                    }
                    return;
                }

                if (materialCardWrappers[i].requiredQuantity > materialCardWrappers[i].quantity)
                {
                    materialCardWrappers[i].quantityText.color = Color.red;
                }
                else
                {
                    materialCardWrappers[i].quantityText.color = Color.white;
                }
            }
        }

        public void ReLoadQuantityMaterialsRequired(RecipeData currentRecipe, List<int> quantityMaterials, int quantity)
        {
            for (int i = 0; i < materialCardWrappers.Count; i++)
            {
                materialCardWrappers[i].requiredQuantity = currentRecipe.ammountPerSlots[i]
                    * quantity;

                materialCardWrappers[i].quantity = quantityMaterials[i];

                materialCardWrappers[i].quantityText.text = materialCardWrappers[i].requiredQuantity.ToString()
                + "/" + materialCardWrappers[i].quantity.ToString();

                if (materialCardWrappers[i].requiredQuantity > materialCardWrappers[i].quantity)
                {
                    materialCardWrappers[i].quantityText.color = Color.red;
                }
                else
                {
                    materialCardWrappers[i].quantityText.color = Color.white;
                }
            }
        }
        #endregion

        #region Storage Management
        public void ResetMaterialStorage(int unlockedStorageSlot)
        {
            for (int i = 0; i < storageCardWrappers.Count; i++)
            {
                if (i >= unlockedStorageSlot) continue;
                else if (i < unlockedStorageSlot)
                {
                    storageCardWrappers[i].image.gameObject.SetActive(false);
                    storageCardWrappers[i].quantityText.text = "";
                    storageCardWrappers[i].GetComponent<Image>().color = new Color32(0, 0, 0, 100);
                    continue;
                }
            }
        }

        public void UpdateMaterialStorage(int unlockedStorageSlot, Dictionary<string, ItemStorageData> itemStorageDict)
        {
            ResetMaterialStorage(unlockedStorageSlot);

            int i = 0;
            foreach (var item in itemStorageDict)
            {
                if (i < unlockedStorageSlot)
                {
                    storageCardWrappers[i].image.gameObject.SetActive(true);
                    storageCardWrappers[i].GetComponent<Image>().color = new Color32(0, 0, 0, 100);
                    storageCardWrappers[i].image.sprite = item.Value.collectableObjectStat.icon;
                    storageCardWrappers[i].quantityText.text = item.Value.quantity.ToString();
                    storageCardWrappers[i].collectableObjectStat = item.Value.collectableObjectStat;
                    i++;
                }
                else break;
            }
        }
        #endregion

        #region Warning System
        public IEnumerator ShowWarning()
        {
            warningFullQueue.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            warningFullQueue.SetActive(false);
        }

        public IEnumerator ShowWarningNotEnoughPower()
        {
            warningNotEnoughPower.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            warningNotEnoughPower.SetActive(false);
        }
        #endregion
    }
}