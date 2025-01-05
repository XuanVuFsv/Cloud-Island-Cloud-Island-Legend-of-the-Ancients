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
        public CollectableObjectStat collectableObjectStat;
        public Suckable product;

        public GameObject UIContainer;
        public Image productImage;
        public TextMeshProUGUI timeDisplay;

        public float totalTime;
        public bool isReady = true;

        public event Action OnCraftCompleted = delegate { };

        // Update is called once per frame
        void Update()
        {
            if (!isReady)
            {
                totalTime = totalTime - Time.deltaTime;
                timeDisplay.text = ((int)totalTime).ToString();
            }
        }

        public void RegisterListener(Action listener)
        {
            OnCraftCompleted += listener;
        }

        public IEnumerator CraftProduct(int time, Transform pos)
        {
            productImage.sprite = collectableObjectStat.icon;
            totalTime = time;

            MyDebug.Log("Start waiting " + (time / product.collectableObjectStat.totalProducingTime).ToString() + " energy");
            isReady = false;

            yield return new WaitForSeconds(time);

            for (int i = 0; i < time / product.collectableObjectStat.totalProducingTime; i++)
            {
                //Debug.Log("Init");
                GameObject newGameObject = Instantiate(product.gameObject, pos.position + UnityEngine.Random.value * 0.25f * Vector3.one, Quaternion.identity);

                //Debug.Log(newGameObject.name);
            }

            isReady = true;
            totalTime = 0;

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