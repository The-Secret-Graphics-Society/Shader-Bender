using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSoundController : MonoBehaviour
{
    [Header("Scriptable Object")]
    [SerializeField] private PoseScriptableObject poseScriptableObject;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] activationClips;
    [SerializeField] private AudioClip[] deactivationClips;
    [SerializeField] private AudioClip fireLoopClip;
    [SerializeField] private AudioClip flameThrowerLoopClip;

    [Header("Settings")]
    [SerializeField] private float moveSpeedThreshold = 0.3f;
    [SerializeField] private float minMovePitch = 1.0f;
    [SerializeField] private float maxMovePitch = 2.0f;
    [SerializeField] private float maxHandPitchSpeed = 3.0f;
    [SerializeField] private float speedSmoothFactor = 0.5f;

    private AudioSource fireAudioSource;
    private AudioSource flameThrowerAudioSource;

    /*
    private AudioSource oneShotAudioSource;
    private AudioSource dripAudioSource;
    private AudioSource moveAudioSource;
    */
    private bool fireActive = false;
    private float handSpeed = 0f;

    void Awake()
    {
        if (poseScriptableObject == null) Debug.LogError("Pose scriptable object not assigned to the PoseManager!");

        fireAudioSource = gameObject.AddComponent<AudioSource>();
        fireAudioSource.loop = true;
        fireAudioSource.playOnAwake = false;
        fireAudioSource.clip = fireLoopClip;
        fireAudioSource.spatialBlend = 0f;
        fireAudioSource.volume = 0.8f;

        flameThrowerAudioSource = gameObject.AddComponent<AudioSource>();
        flameThrowerAudioSource.loop = true;
        flameThrowerAudioSource.playOnAwake = false;
        flameThrowerAudioSource.clip = flameThrowerLoopClip;
        flameThrowerAudioSource.spatialBlend = 0f;
        flameThrowerAudioSource.volume = 0.8f;

        meteorAudioSource = gameObject.AddComponent<AudioSource>();
        meteorAudioSource.loop = true;
        meteorAudioSource.playOnAwake = false;
        meteorAudioSource.clip = meteorLoopClip;
        meteorAudioSource.spatialBlend = 0f;
        meteorAudioSource.volume = 0.8f;
    }

    void Update()
    {
        if (fireActive)
        {
            if (!fireAudioSource.isPlaying) fireAudioSource.Play();
        }
        else
        {
            if (fireAudioSource.isPlaying) fireAudioSource.Stop();
        }
    }

    private void ActivateFireSounds()
    {
        fireActive = true;

        if (activationClips != null && activationClips.Length > 0)
        {
            AudioClip clip = activationClips[Random.Range(0, activationClips.Length)];
            fireAudioSource.PlayOneShot(clip);
        }
    }

    private void DeactivateFireSounds()
    {
        fireActive = false;

        if (deactivationClips != null && deactivationClips.Length > 0)
        {
            AudioClip clip = deactivationClips[Random.Range(0, deactivationClips.Length)];
            fireAudioSource.PlayOneShot(clip);
        }

        if (fireAudioSource.isPlaying) fireAudioSource.Stop();
    }

    private void OnEnable()
    {
        ElementState.onFireActive += ActivateFireSounds;
        ElementState.onElementDeactivate += DeactivateFireSounds;
    }

    private void OnDisable()
    {
        ElementState.onFireActive -= ActivateFireSounds;
        ElementState.onElementDeactivate -= DeactivateFireSounds;
    }
}