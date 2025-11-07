using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Manager;
using VitsehLand.Scripts.Stats;
using VitsehLand.Scripts.Ultilities;

namespace VitsehLand.Scripts.Crafting
{
    public class CraftQueueHandler : MonoBehaviour
    {
        #region Main Craft Queue UI Elements In Crafting UI
        public CollectableObjectStat collectableObjectStat;
        public Suckable product;

        public GameObject UIContainer;
        public Image productImage;
        #endregion

        #region  Craft Queue State Variables
        public TextMeshProUGUI timeDisplay;

        public float duration;
        public float remaining;
        public long startTimestamp;
        public bool isActive = false;
        public bool isReady = true;

        public event Action OnCraftCompleted = delegate { };
        #endregion

        // Update is called once per frame
        void Update()
        {
            if (!isReady && isActive)
            {
                //Debug.Log(gameObject.name);
                remaining -= Time.deltaTime;
                timeDisplay.text = ((int)remaining).ToString();
            }
        }

        public void RegisterListener(Action listener)
        {
            OnCraftCompleted += listener;
        }

        public float GetRemainingTime()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float elapsed = currentTime - startTimestamp;
            remaining = Mathf.Max(0, duration - elapsed);

            return remaining;
        }

        public bool IsCompleted()
        {
            return GetRemainingTime() <= 0;
        }

        public void SetDisplay(bool isActivated)
        {
            // Already in the desired states
            if (isActive == isActivated) return;

            if (isActivated)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (IsCompleted())
            {
                Hide();
                return;
            }

            isActive = true;
            Debug.Log("Show" + transform.parent.name + " " + gameObject.name);

            // Validate before assigning icon
            if (collectableObjectStat != null ? collectableObjectStat.icon : null != null)
            {
                productImage.sprite = collectableObjectStat.icon;
                UIContainer.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[CraftQueueHandler] Missing icon for {gameObject.name}");
            }
        }

        private void Hide()
        {
            isActive = false;
            Debug.Log("Hide" + transform.parent.name + " " + gameObject.name);

            UIContainer.SetActive(false);
            productImage.sprite = null;
        }

        public IEnumerator CraftProduct(int time, Transform pos)
        {
            productImage.sprite = collectableObjectStat.icon;
            duration = time;
            remaining = time;
            startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int productCount = (int)(time / product.collectableObjectStat.totalProducingTime);

            MyDebug.Log("Start waiting " + (productCount).ToString() + " energy");
            isReady = false;
            isActive  = true;

            yield return new WaitForSeconds(time);

            for (int i = 0; i < productCount; i++)
            {
                //GameObject newGameObject;
                //Debug.Log("Init");
                _ = Instantiate(product.gameObject, pos.position + UnityEngine.Random.value * 0.25f * Vector3.one, Quaternion.identity);

                //Debug.Log(newGameObject.name);
            }

            isReady = true;
            isActive = false;
            duration = 0;
            remaining = 0;

            MyDebug.Log("Done");
            GemManager.Instance.AddGem(product.collectableObjectStat.gemEarnWhenHaverst);

            UIContainer.SetActive(false);
            OnCraftCompleted();
        }

        public void Craft(int time, Transform pos)
        {
            StartCoroutine(CraftProduct(time, pos));
        }
    }
}