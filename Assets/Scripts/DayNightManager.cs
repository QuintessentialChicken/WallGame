using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DayNightManager : MonoBehaviour
{
    [Header("Just put this in the scene and center it at (0,0,0)\nThen set time of day.")]
    [SerializeField]
    private TimeOfDay timeOfDay;
    [Space]
    [SerializeField]
    private LightingSettings day;
    [SerializeField]
    private LightingSettings evening;
    [SerializeField]
    private LightingSettings night;
    [Space]
    [SerializeField]
    private Light dirLight;
    [Space]
    [SerializeField]
    public RawImage fadeScreen;

    public static DayNightManager instance;

    private TimeOfDay setTimeOfDay;

    private Dictionary<TimeOfDay, LightingSettings> map;

    void Start()
    {
        StartCoroutine(nameof(FadeIn));
        
        if (map == null) SetUpDictionary();

        ChangeTimeOfDay(timeOfDay);
        UpdateSky();
        instance = this;
    }

    public void ChangeTimeOfDay(TimeOfDay time)
    {
        timeOfDay = time;
    }

    private IEnumerator ChangeToDay()
    {
        StopCoroutine(nameof(ChangeToNight));
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            fadeScreen.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene("Day_Siege");
        yield return null;
    }

    private IEnumerator ChangeToNight()
    {
        StopCoroutine(nameof(ChangeToDay));
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            fadeScreen.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene("Evening_Upgrades");
        yield return null;
    }

    /*private IEnumerator ChangeToRating()
    {
        StopCoroutine(nameof(ChangeToDay));
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            fadeScreen.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene("Rating");
        yield return null;
    }*/

    private IEnumerator ChangeToTutorial()
    {
        StopCoroutine(nameof(ChangeToNight));
        StopCoroutine(nameof(ChangeToDay));
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            fadeScreen.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene("Tutorial");
        yield return null;
    }
    /*
    public void RequestChangeToDay()
    {
        StartCoroutine(nameof(ChangeToDay));
    }

    public void RequestChangeToNight()
    {
        StartCoroutine(nameof(ChangeToNight));
    }

    public void RequestChangeToTutorial()
    {
        StartCoroutine(nameof(ChangeToTutorial));
    }*/

    private IEnumerator FadeIn()
    {
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            fadeScreen.color = new Color(0, 0, 0, 1-t);
            yield return null;
        }
    }

    private bool changing = false;

    public void RequestChangeTo(TimeOfDay timeOfDay)
    {
        if (changing) return;
        changing = true;
        switch (timeOfDay)
        {
            case TimeOfDay.Day_Siege:
                StartCoroutine(nameof(ChangeToDay));
                break;
            case TimeOfDay.Evening_Upgrades:
                StartCoroutine(nameof(ChangeToNight));
                break;
            case TimeOfDay.Tutorial:
                StartCoroutine(nameof(ChangeToTutorial));
                break;
        }
    }

    

    private void SetUpDictionary()
    {
        map = new Dictionary<TimeOfDay, LightingSettings>
        {
            { TimeOfDay.Day_Siege, day },
            { TimeOfDay.Evening_Upgrades, evening },
            { TimeOfDay.Night_Rating, night },
            { TimeOfDay.Tutorial, day }
        };
    }

    public void ChangeToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnDrawGizmos()
    {
        if (Application.IsPlaying(this)) return;
        if (map == null) SetUpDictionary();
        //if (setTimeOfDay != timeOfDay)
            UpdateSky();
    }

    // Update is called once per frame
    void Update()
    {
        if (setTimeOfDay != timeOfDay)
            UpdateSky();
    }

    private void UpdateSky()
    {
        LightingSettings settings;
        
        if (map.TryGetValue(timeOfDay, out settings))
        { 
            dirLight.intensity = settings.dirLightIntensity;
            dirLight.color = settings.dirLightColor;
            dirLight.transform.rotation = Quaternion.Euler(settings.dirRotation);
            UnityEngine.RenderSettings.skybox = settings.skyMaterial;

            //settings.timeSpecificProps?.SetActive(true);
        }
        
        setTimeOfDay = timeOfDay;
    }

    public enum TimeOfDay
    {
        Tutorial = 100,
        Day_Siege = 0, Evening_Upgrades = 1, Night_Rating = 2
    }

    [System.Serializable]
    public class LightingSettings
    {
        //public GameObject timeSpecificProps;
        public Material skyMaterial;
        public float dirLightIntensity;
        public Color dirLightColor;
        public Vector3 dirRotation;
    }
}
