using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VitsehLand.Scripts.Pattern.Singleton;
using VitsehLand.Scripts.TimeManager;
namespace VitsehLand.Scripts.Player
{

    //Logic in this class need to refactor by using event system and put logic UI, Sound to other class
    public class HealthController : Singleton<HealthController>
    {
        public TextMeshProUGUI healthText;
        public float health;
        public bool inRestart = false;
        public GameObject loadUI;
        public RectTransform progressBar;
        public RectTransform healthBar;

        public UnityEvent OnRestart;

        private void Start()
        {
            healthText.text = health.ToString();
        }

        private void Update()
        {
            if (GameTimeManager.Instance.Hours == 7 && GameTimeManager.Instance.Minutes == 0)
            {
                health = 100;
                if (healthBar == null)
                {
                    Debug.Log(gameObject.name);
                }
                else
                {
                    healthBar.localScale = Vector3.one;
                    healthText.text = health.ToString();
                }
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (healthBar) healthBar.localScale = new Vector3(health / 100, 1, 1);

            if (health <= 0)
            {
                health = 0;
                if (gameObject.tag == "Player" && !inRestart)
                {
                    inRestart = true;
                    StartCoroutine(ReStart());
                    OnRestart.Invoke();
                }
            }

            healthText.text = health.ToString();
        }

        IEnumerator ReStart()
        {
            progressBar.localScale = new Vector3(0, 1, 1);
            loadUI.SetActive(true);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            asyncOperation.allowSceneActivation = false;
            float progress = 0;

            while (!asyncOperation.isDone)
            {
                progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
                progressBar.localScale = new Vector3(progress, 1, 1);

                if (progress >= 0.9f)
                {
                    progressBar.localScale = new Vector3(1, 1, 1);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}