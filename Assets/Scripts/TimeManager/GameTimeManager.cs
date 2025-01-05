using System.Collections;
using TMPro;
using UnityEngine;
using VitsehLand.Scripts.Pattern.Observer;
using VitsehLand.Scripts.Pattern.Singleton;
using VitsehLand.Scripts.Player;

namespace VitsehLand.Scripts.TimeManager
{
    public class GameTimeManager : Singleton<GameTimeManager>
    {
        [System.Serializable]
        public class TimeLightingSetting
        {
            public Color startAmbientClolor;
            public Color targetAmbientColor;

            public Color startFogClolor;
            public Color targetFogColor;

            public float startDensityFog;
            public float targetDensityFog;

            public Color startTintClolor;
            public Color targetTintColor;

            public float startExposure;
            public float targetExposure;

            public float lerpDuration;
            public int hoursWhenChangeSkyBox;
        }

        public TimeLightingSetting sunrise;
        public TimeLightingSetting day;
        public TimeLightingSetting sunset;
        public TimeLightingSetting night;
        public TimeLightingSetting currentTimeLightingSetting;

        public Material skyboxMaterial;

        public float seconInRealLifeVsIngame;

        [SerializeField]
        private float tempSeconds;

        [SerializeField]
        TextMeshProUGUI minutesText;
        [SerializeField]
        private int minutes;
        public int Minutes
        {
            get
            {
                return minutes;
            }

            set
            {
                minutes = value; OnMinutesChange?.Invoke(value);
            }
        }

        [SerializeField]
        TextMeshProUGUI hoursText;
        [SerializeField]
        private int hours;
        public int Hours
        {
            get
            {
                return hours;
            }

            set
            {
                hours = value; OnHoursChange?.Invoke(value);
            }
        }

        [SerializeField]
        TextMeshProUGUI daysText;
        [SerializeField]
        private int days;
        public int Days
        {
            get
            {
                return days;
            }

            set
            {
                days = value; OnDaysChange?.Invoke(value);
            }
        }
        // Define an event to be triggered when the property changes
        public event System.Action<int> OnMinutesChange;
        public event System.Action<int> OnHoursChange;
        public event System.Action<int> OnDaysChange;

        public bool isNight = false;

        public HealthController player;

        public GameEvent atNightEvent;

        // Start is called before the first frame update
        void Start()
        {
            SetupTimeLighting(sunrise);
            skyboxMaterial = RenderSettings.skybox;
            OnMinutesChange += HandleMinutesChange;
            OnHoursChange += HandleHoursChange;
            OnDaysChange += HandleDaysChange;
        }

        // Update is called once per frame
        void Update()
        {
            if (seconInRealLifeVsIngame == 0) return;

            if (tempSeconds >= 60 / seconInRealLifeVsIngame)
            {
                tempSeconds = 0;
                Minutes++;
            }
            tempSeconds += Time.deltaTime;
        }

        private void HandleMinutesChange(int value)
        {
            if (value >= 60)
            {
                Hours++;
                Minutes = 0;
            }

            if (hours >= 24)
            {
                Days++;
                Hours = 0;
            }

            if (minutes >= 10) minutesText.text = minutes.ToString();
            else minutesText.text = "0" + minutes.ToString();
        }

        public void Pause()
        {
            seconInRealLifeVsIngame = 0;
        }

        public void UnPause()
        {
            seconInRealLifeVsIngame = 90;
        }

        private void HandleHoursChange(int value)
        {
            if (value == sunrise.hoursWhenChangeSkyBox)
            {
                isNight = false;
                player.health = 100;
                StartCoroutine(LerpSkyBox(sunrise));
            }
            else if (value == day.hoursWhenChangeSkyBox)
            {
                StartCoroutine(LerpSkyBox(day));
            }
            else if (value == sunset.hoursWhenChangeSkyBox)
            {
                StartCoroutine(LerpSkyBox(sunset));
            }
            else if (value == night.hoursWhenChangeSkyBox)
            {
                isNight = true;
                StartCoroutine(LerpSkyBox(night));
            }

            if (hours >= 10) hoursText.text = hours.ToString();
            else hoursText.text = "0" + hours.ToString();

            if (isNight)
            {
                //EnemySpawner.Instance.Spawn(seconInRealLifeVsIngame);
                atNightEvent.Notify(seconInRealLifeVsIngame);
            }
        }

        private void HandleDaysChange(int value)
        {
            daysText.text = days.ToString();
            if (days >= 10) daysText.text = days.ToString();
            else daysText.text = "0" + days.ToString();
        }

        void SetupTimeLighting(TimeLightingSetting t)
        {
            RenderSettings.ambientLight = t.targetAmbientColor;

            RenderSettings.fogColor = t.targetFogColor;
            RenderSettings.fogDensity = t.targetDensityFog;

            RenderSettings.skybox.SetColor("_Tint", t.targetTintColor);
            RenderSettings.skybox.SetFloat("_Exposure", t.targetExposure);

            currentTimeLightingSetting = t;
        }

        private IEnumerator LerpSkyBox(TimeLightingSetting t)
        {
            float elapsedTime = 0f;
            float lerpDuration = t.lerpDuration * 60 / seconInRealLifeVsIngame;
            float _linearValue = 0f;

            while (elapsedTime < lerpDuration)
            {
                _linearValue = elapsedTime / lerpDuration;

                // Interpolate between startExposure and targetExposure
                Color ambient = Color.Lerp(t.startAmbientClolor, t.targetAmbientColor, _linearValue);
                RenderSettings.ambientLight = ambient;

                Color fog = Color.Lerp(t.startFogClolor, t.targetFogColor, _linearValue);
                RenderSettings.fogColor = fog;

                float density = Mathf.Lerp(t.startDensityFog, t.targetDensityFog, _linearValue);
                RenderSettings.fogDensity = density;

                Color tint = Color.Lerp(t.startTintClolor, t.targetTintColor, _linearValue);
                RenderSettings.skybox.SetColor("_Tint", tint);

                float exposure = Mathf.Lerp(t.startExposure, t.targetExposure, _linearValue);
                RenderSettings.skybox.SetFloat("_Exposure", exposure);

                // Increment elapsed time
                elapsedTime += Time.deltaTime;

                // Wait for the next frame
                yield return null;
            }
            SetupTimeLighting(t);
        }
    }
}