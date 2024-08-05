using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
using System;


public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;

    private Bus ambienceBus;
    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance musicEventInstance;

    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // Retrieve the scene name and build index
        string sceneName = currentScene.name;
        if (!sceneName.Contains("Evening")) {
            InitializeSound(FMODEvents.instance.drumStartMusic, true);
            InitializeSound(FMODEvents.instance.battleAmbience, true);
        } else {
            InitializeSound(FMODEvents.instance.nightAmbientMusic);
            SFXVolume *= 0.25f;
        }
    }

    public void FadeForGasping()
    {
        ambienceBus.setVolume(ambienceVolume * 0.3f);
        musicBus.setVolume(musicVolume * 0.3f);
    }

    public void IncreaseAfterGasping()
    {
        ambienceBus.setVolume(ambienceVolume / 0.3f);
        musicBus.setVolume(musicVolume / 0.3f);
    }

    private void Update()
    {
        ambienceBus.setVolume(ambienceVolume);
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(SFXVolume);
    }

    private void InitializeSound(EventReference musicEventReference, Boolean threeD = false)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        if (threeD) {
            var attributes = RuntimeUtils.To3DAttributes(transform.position);
            musicEventInstance.set3DAttributes(attributes);
        }
        musicEventInstance.start();
    }

    public void UpdateMusic(EventReference musicEventReference)
    {
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    private void CleanUp()
    {
        // stop and release any created instances
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        // stop all of the event emitters, because if we don't they may hang around in other scenes
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}