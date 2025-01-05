using System.Collections.Generic;
using UnityEngine;
using VitsehLand.Assets.Scripts.Weapon.Collector;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Farming.Resource;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Weapon.Ammo;
using VitsehLand.Scripts.Weapon.General;

namespace VitsehLand.Scripts.Inventory
{
    [System.Serializable]
    public class Item
    {
        public CollectableObjectStat collectableObjectStat;
        public List<Suckable> totalPlant = new List<Suckable>();
        public Suckable suckableSample;
        public int index;
        public int count;
        public int ammountAmmoUsedByAttackWeapon = 0;
        public bool isActive = true;

        public Item(CollectableObjectStat collectableObjectStat, int count, bool isActive, Suckable ammoObject, int index)
        {
            this.collectableObjectStat = collectableObjectStat;
            AddAmmo(count, ammoObject);
            this.isActive = isActive;
            this.index = index;
        }

        public bool IsFull()
        {
            return count < collectableObjectStat.maxCount;
        }

        public int AddAmmo(int newCount, Suckable ammoObject)
        {
            bool isCrop = ammoObject is Crop;
            bool isPower = ammoObject is PowerContainer;
            bool isNaturalResource = ammoObject is NaturalResource;
            bool isProduct = ammoObject is FarmingProduct;

            //if (count == 0)
            //{
            //    if (isCrop) suckableSample = ammoObject.gameObject;
            //    else suckableSample = (ammoObject as AmmoPickup).suckableSample;
            //}

            int currentCount = 1;

            if (isCrop)
            {
                if ((ammoObject as Crop).isFromHarvesting == false)
                {
                    currentCount = count + newCount;
                }
                else currentCount = count + newCount * collectableObjectStat.ammmoHolding;
            }
            else currentCount = count + newCount * collectableObjectStat.ammmoHolding;
            ammoObject.cropContain = 1;
            //Debug.Log(currentCount);

            if ((isCrop || isPower || isNaturalResource || isProduct) && currentCount <= collectableObjectStat.maxCount)
            {
                //Debug.Log("add");
                totalPlant.Add(ammoObject);
            }
            else if (totalPlant.Count == 0 && !(isCrop || isPower || isNaturalResource))
            {
                Debug.Log("Just set plant sample");
                suckableSample = (ammoObject as AmmoPickup).suckableSample;
            }

            if (currentCount <= collectableObjectStat.maxCount)
            {
                count = currentCount;
                //Debug.Log(isPlant);

                if (isCrop || isPower || isNaturalResource || isProduct)
                {
                    ammoObject.GetComponent<Suckable>().ChangeToStored();
                    suckableSample = Object.Instantiate(ammoObject.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<Suckable>();
                }
                //else if (isPower)
                //{
                //    ammoObject.GetComponent<PowerContainer>().ChangeToStored();
                //    suckableSample = GameObject.Instantiate(ammoObject.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<Suckable>();
                //}
                //else if (isNaturalResource)
                //{
                //    ammoObject.GetComponent<PowerContainer>().ChangeToStored();
                //    suckableSample = GameObject.Instantiate(ammoObject.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<Suckable>();
                //}
                else
                {
                    Object.Destroy(ammoObject.gameObject);
                }
                return 0;
            }
            else
            {
                count = collectableObjectStat.maxCount;
                if (!isCrop) Object.Destroy(ammoObject.gameObject);
                return currentCount - collectableObjectStat.maxCount;
            }
        }

        public void UseAmmo(int newCount, ActiveWeapon.WeaponSlot fromWeaponSlot)
        {
            int currentCount = count - newCount;
            //Debug.Log(currentCount);
            if (currentCount < 0)
            {
                //suckableSample.RemoveUseGameEvent();
                return;
            }
            else
            {
                count = currentCount;

                int lastIndex = totalPlant.Count - 1;
                if (lastIndex >= 0)
                {
                    if (fromWeaponSlot == ActiveWeapon.WeaponSlot.AxieCollector)
                    {
                        totalPlant[lastIndex].ChangeToUnStored();
                        totalPlant[lastIndex].transform.position = CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position;

                        //(totalPlant[lastIndex] as Plant).orginalBody = InventoryController.Instance.gameObject;
                        totalPlant[lastIndex].MoveOut();
                    }

                    totalPlant.RemoveAt(lastIndex);
                }
                else
                {
                    if (fromWeaponSlot == ActiveWeapon.WeaponSlot.AxieCollector)
                    {
                        //bool isPlant = suckableSample is Plant;
                        //bool isPower = suckableSample is PowerContainer;

                        Suckable newSuckableObject;

                        if (suckableSample is Crop) newSuckableObject = Object.Instantiate(suckableSample.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<Crop>();
                        else if (suckableSample is PowerContainer) newSuckableObject = Object.Instantiate(suckableSample.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<PowerContainer>();
                        else if (suckableSample is FarmingProduct) newSuckableObject = Object.Instantiate(suckableSample.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<FarmingProduct>();
                        else newSuckableObject = Object.Instantiate(suckableSample.gameObject, CollectHandler.Instance.shootingInputData.bulletSpawnPoint.position, Quaternion.identity).GetComponent<NaturalResource>();

                        newSuckableObject.gameObject.SetActive(true);
                        //(newPlant as Plant).plantData.orginalBody = InventoryController.Instance.gameObject;
                        //newPlant.ChangeToSeed();
                        newSuckableObject.MoveOut();
                    }
                }
            }
        }

        public void ResetItem(Item resetItem)
        {
            collectableObjectStat = resetItem.collectableObjectStat;
            totalPlant = null;
            suckableSample = null;
            count = 0;
        }
    }
}