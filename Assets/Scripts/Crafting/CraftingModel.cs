using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Inventory;
using VitsehLand.Scripts.Stats;

namespace VitsehLand.Scripts.Crafting
{
    public class CraftingModel : SerializedMonoBehaviour
    {
        #region Data Storage Variables
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 20)]
        [InfoBox("List of product's recipes player currently can craft.")]
        [DictionaryDrawerSettings(KeyLabel = "Product Name", ValueLabel = "RecipeData")]
        public Dictionary<string, RecipeData> productRecipes = new Dictionary<string, RecipeData>();

        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        [InfoBox("List of items has been stored in crafting machine.")]
        [DictionaryDrawerSettings(KeyLabel = "Item Name", ValueLabel = "ItemStorageData")]
        public Dictionary<string, ItemStorageData> itemStorageDict = new Dictionary<string, ItemStorageData>();

        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        [InfoBox("List of CraftQueueHandler references that handle the craft progress.")]
        public List<CraftQueueHandler> craftQueueHandlers = new List<CraftQueueHandler>();

        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        [InfoBox("Products that were crafted will be stored here.")]
        public List<Suckable> products = new List<Suckable>();
        #endregion


        #region Other Variables
        [BoxGroup("Quantity")]
        [Tooltip("Max products per one-time crafting")]
        public int maxQuantity;

        [BoxGroup("Quantity")]
        [Tooltip("Quantity the user has currently chosen for one-time crafting.")]
        public int currentQuantity;

        [BoxGroup("Storage Slot")]
        [Tooltip("Quantity of storage slots has been unlocked and ready to store.")]
        public int unlockedStorageSlot = 3;

        [BoxGroup("Storage Slot")]
        [Tooltip("Max quantity of storage slots user can use to store materials.")]
        public int maxStorageSlot = 9;

        [BoxGroup("Queue Info")]
        [Tooltip("Quantity of queue slots has been unlocked and ready to use.")]
        public int queueQuantity = 1;

        [BoxGroup("Queue Info")]
        [Tooltip("Max quantity of queue slots user can use to craft products.")]
        public int maxQueueQuantity = 4;

        [BoxGroup("Queue Info")]
        [Tooltip("Quantity of queue slots being used.")]
        public int queueActiveQuantity = 0;

        [PropertySpace(SpaceBefore = 20)]
        public string currentRecipeNameId;
        #endregion

        public void SetupInitData()
        {
            currentRecipeNameId = productRecipes.ElementAt(0).Key;

            for (int i = 0; i < craftQueueHandlers.Count; i++)
            {
                craftQueueHandlers[i].UIContainer.SetActive(false);
            }
        }

        public RecipeData GetCurrentRecipe()
        {
            return productRecipes[currentRecipeNameId];
        }

        /// <summary>
        /// Return a list containing the quantities of materials needed for the specific recipe.
        /// </summary>
        public List<int> GetQuantityByMaterialOfRecipe(RecipeData recipe)
        {
            return recipe.items.Select(item => GetItemStorageQuantityByName(item.collectableObjectName)).ToList();
        }

        public int FindFirstCraftSlotReady()
        {
            for (int i = 0; i < queueQuantity; i++)
            {
                if (craftQueueHandlers[i].isReady) return i;
            }
            return -1;
        }

        public int GetItemStorageQuantityByName(string name)
        {
            if (itemStorageDict.Count == 0)
            {
                Debug.Log("Empty Storage");
                return 0;
            }
            if (!itemStorageDict.ContainsKey(name))
            {
                Debug.Log(name + " not exist");
                return 0;
            }
            return itemStorageDict[name].quantity;

        }

        public void SetItemStorage(string name, int value)
        {
            Debug.Log(name);
            itemStorageDict[name].quantity = value;
        }

        public int GetItemStorage(string name)
        {
            if (!itemStorageDict.ContainsKey(name)) return 0;
            return itemStorageDict[name].quantity;
        }

        public void RemoveItemStorage(string name)
        {
            itemStorageDict.Remove(name);
        }

        public bool CheckStorage()
        {
            if (itemStorageDict.Count == unlockedStorageSlot) return false;
            return true;
        }

        public bool AddItemStorage(CollectableObjectStat collectableObjectStat, int quantity)
        {
            if (itemStorageDict.ContainsKey(collectableObjectStat.collectableObjectName))
            {
                itemStorageDict[collectableObjectStat.collectableObjectName].quantity += quantity;
                return true;
            }
            else if (CheckStorage())
            {
                itemStorageDict.Add(collectableObjectStat.collectableObjectName, new ItemStorageData(collectableObjectStat.collectableObjectName, collectableObjectStat, ItemStorageData.StorageLocation.CraftMachine, /*collectableObjectStat.cropPrefab*/null, quantity));
                return true;
            }
            else if (!CheckStorage()) return false;
            return false;
        }
    }
}