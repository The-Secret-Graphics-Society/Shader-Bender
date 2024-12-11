using UnityEngine;

public class WaterSoundController : MonoBehaviour
{
    [Header("Scriptable Object")]
    [SerializeField] private PoseScriptableObject poseScriptableObject;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] activationClips;
    [SerializeField] private AudioClip[] deactivationClips;
    [SerializeField] private AudioClip dripLoopClip;
    [SerializeField] private AudioClip moveLoopClip;

    [Header("Settings")]
    [SerializeField] private float moveSpeedThreshold = 0.3f;
    [SerializeField] private float minMovePitch = 1.0f;
    [SerializeField] private float maxMovePitch = 2.0f;
    [SerializeField] private float maxHandPitchSpeed = 3.0f;
    [SerializeField] private float speedSmoothFactor = 0.5f;

    private AudioSource oneShotAudioSource;
    private AudioSource dripAudioSource;
    private AudioSource moveAudioSource;
    private bool waterActive = false;
    private float handSpeed = 0f;

    void Awake()
    {
        if (poseScriptableObject == null) Debug.LogError("Pose scriptable object not assigned to the PoseManager!");

        oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        oneShotAudioSource.playOnAwake = false;
        oneShotAudioSource.loop = false;
        oneShotAudioSource.spatialBlend = 0f;
        oneShotAudioSource.volume = 0.4f;

        dripAudioSource = gameObject.AddComponent<AudioSource>();
        dripAudioSource.loop = true;
        dripAudioSource.playOnAwake = false;
        dripAudioSource.clip = dripLoopClip;
        dripAudioSource.spatialBlend = 0f;
        dripAudioSource.volume = 0.4f;

        moveAudioSource = gameObject.AddComponent<AudioSource>();
        moveAudioSource.loop = true;
        moveAudioSource.playOnAwake = false;
        moveAudioSource.clip = moveLoopClip;
        moveAudioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (waterActive)
        {
            if (!dripAudioSource.isPlaying) dripAudioSource.Play();
            if (!moveAudioSource.isPlaying) moveAudioSource.Play();

            float rawHandSpeed = Mathf.Max(poseScriptableObject.leftHandSpeed, poseScriptableObject.rightHandSpeed);
            handSpeed = Mathf.Lerp(handSpeed, rawHandSpeed, Time.deltaTime * speedSmoothFactor);

            // Calculate volume based on hand speed:
            // 0 speed -> volume = 0
            // speed at threshold -> volume = 0.6
            // speed at maxHandPitchSpeed -> volume = 1.0
            float targetVolume;
            if (handSpeed <= 0f)
            {
                targetVolume = 0f;
            }
            else if (handSpeed < moveSpeedThreshold)
            {
                targetVolume = Mathf.Lerp(0f, 0.6f, handSpeed / moveSpeedThreshold);
            }
            else
            {
                float t = Mathf.InverseLerp(moveSpeedThreshold, maxHandPitchSpeed, handSpeed);
                targetVolume = Mathf.Lerp(0.6f, 1.0f, t);
            }

            // Calculate pitch based on hand speed:
            // If below or equal to threshold, pitch = minMovePitch
            // If above threshold, pitch = lerp(minMovePitch, maxMovePitch)
            float targetPitch;
            if (handSpeed <= moveSpeedThreshold)
            {
                targetPitch = minMovePitch;
            }
            else
            {
                float t = Mathf.InverseLerp(moveSpeedThreshold, maxHandPitchSpeed, handSpeed);
                targetPitch = Mathf.Lerp(minMovePitch, maxMovePitch, t);
            }

            moveAudioSource.volume = targetVolume;
            moveAudioSource.pitch = targetPitch;
            dripAudioSource.volume = 0.4f + targetVolume / 2;
        }

        else
        {
            if (dripAudioSource.isPlaying) dripAudioSource.Stop();
            if (moveAudioSource.isPlaying) moveAudioSource.Stop();
        }
    }

    private void ActivateWaterSounds()
    {
        waterActive = true;

        if (activationClips != null && activationClips.Length > 0)
        {
            AudioClip clip = activationClips[Random.Range(0, activationClips.Length)];
            oneShotAudioSource.PlayOneShot(clip);
        }
    }

    private void DeactivateWaterSounds()
    {
        if (!waterActive) return;
        waterActive = false;

        if (deactivationClips != null && deactivationClips.Length > 0)
        {
            AudioClip clip = deactivationClips[Random.Range(0, deactivationClips.Length)];
            oneShotAudioSource.PlayOneShot(clip);
        }

        if (dripAudioSource.isPlaying) dripAudioSource.Stop();
        if (moveAudioSource.isPlaying) moveAudioSource.Stop();
    }

    private void OnEnable()
    {
        ElementState.onWaterActive += ActivateWaterSounds;
        ElementState.onElementDeactivate += DeactivateWaterSounds;
    }

    private void OnDisable()
    {
        ElementState.onWaterActive -= ActivateWaterSounds;
        ElementState.onElementDeactivate -= DeactivateWaterSounds;
    }
}
