using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VitsehLand.Scripts.Farming.General;
using UnityEngine.Events;

namespace VitsehLand.Scripts.Farming
{
    public class LivestockBehaviour : MonoBehaviour
    {
        public Suckable product;
        public int minProduceTime, maxProduceTime;
        public bool madeProduct;

        public List<Vector3> movingList = new List<Vector3>();
        public int index = 0;
        public float speed = 1;
        float s = 0;
        public Vector3 currentTarget;

        public int food = 0;
        public int foodStack;
        public int timeToHungry;

        public UnityEvent OnHungry, OnBreed, OnFull, OnProduceEgg;
        public RectTransform hungryIcon;

        // Start is called before the first frame update
        void Start()
        {
            movingList.Add(new Vector3(transform.position.x, transform.position.y));

            index = Random.Range(0, movingList.Count - 2);
            currentTarget = movingList[movingList.Count - 1] + movingList[index];

            OnHungry.Invoke();
        }

        // Update is called once per frame
        void Update()
        {
            if (food == foodStack)
            {
                if (madeProduct == false)
                {
                    StartCoroutine(ProductEgg());
                }
            }

            if (Vector3.Distance(transform.position, currentTarget) <= 0.25f)
            {
                index = Random.Range(0, movingList.Count - 2);
                s = 0;
                currentTarget = movingList[movingList.Count - 1] + movingList[index];
            }
            s += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, currentTarget, s * speed);
            transform.LookAt(currentTarget);
            hungryIcon.LookAt(Camera.main.transform);
        }

        IEnumerator ProductEgg()
        {
            madeProduct = true;
            yield return new WaitForSeconds(Random.Range(minProduceTime, maxProduceTime));
            Instantiate(product.gameObject, transform.position, Quaternion.identity);
            OnProduceEgg.Invoke();
            madeProduct = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnTriggerEnter(Collider collider)
        {
            Crop item = collider.gameObject.GetComponent<Crop>();
            if (item == null) return;
            if (item.collectableObjectStat.collectableObjectName == "Wheat" && food < foodStack)
            {
                food++;
                OnBreed.Invoke();

                if (food == foodStack)
                {
                    OnFull.Invoke();
                    StartCoroutine(Hungry());
                }
                item.gameObject.SetActive(false);
                Destroy(item.gameObject, Random.Range(1, 10));
            }
        }

        IEnumerator Hungry()
        {
            yield return new WaitForSeconds(timeToHungry);
            OnHungry.Invoke();
            food = 0;
        }
    }
}