using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSoundController : MonoBehaviour
{
    [Header("Scriptable Object")]
    [SerializeField] private PoseScriptableObject poseScriptableObject;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip fireLoopClip;
    [SerializeField] private AudioClip flameThrowerLoopClip;

    private AudioSource fireAudioSource;
    private AudioSource flameThrowerAudioSource;

    private bool fireActive = false;

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
        fireAudioSource.PlayOneShot(fireLoopClip);
    }

    private void DeactivateFireSounds()
    {
        fireActive = false;
        fireAudioSource.PlayOneShot(flameThrowerLoopClip);
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