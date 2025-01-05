using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        public event Action<int, QuanityChangedActionType> OnQuantityChanged = delegate { };
        public event Action OnCraftButtonClick = delegate { };

        // Start is called before the first frame update
        void Start()
        {

        }

        public void CloseCraftingUI()
        {
            if (cursorAvaiable && isActive)
            {
                Show(false);
            }
        }

        public void Show(bool show)
        {
            body.gameObject.SetActive(show);
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

        #region Main Action Components
        public void ShowRecipe()
        {
            products.gameObject.SetActive(true);
            interaction.gameObject.SetActive(true);
            queueDisplay.gameObject.SetActive(false);
            materials.gameObject.SetActive(false);
            information.gameObject.SetActive(true);
        }

        public void ShowQueue()
        {
            products.gameObject.SetActive(false);
            interaction.gameObject.SetActive(false);
            queueDisplay.gameObject.SetActive(true);
            materials.gameObject.SetActive(false);
            information.gameObject.SetActive(false);
        }

        public void ShowMaterials()
        {
            products.gameObject.SetActive(false);
            interaction.gameObject.SetActive(false);
            queueDisplay.gameObject.SetActive(false);
            materials.gameObject.SetActive(true);
            information.gameObject.SetActive(false);
        }
        #endregion

        #region Crafting Components
        public void RegisterListener(Action<int, QuanityChangedActionType> listener)
        {
            OnQuantityChanged += listener;
        }

        public void PlusQuantity()
        {
            OnQuantityChanged(1, QuanityChangedActionType.Button);
        }

        public void MinusQuantity()
        {
            OnQuantityChanged(-1, QuanityChangedActionType.Button);
        }

        public void OnSliderChanged()
        {
            OnQuantityChanged((int)slider.value, QuanityChangedActionType.Slider);
        }
        #endregion

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

        public void RegisterListener(Action listener)
        {
            OnCraftButtonClick += listener;
        }

        public void Craft()
        {
            OnCraftButtonClick();
        }

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
    }
}