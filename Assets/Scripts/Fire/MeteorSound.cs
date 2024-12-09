using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSound : MonoBehaviour
{
    private ParticleSystem _parentParticleSystem;

    private int _currentNumberOfParticles = 0;

    public AudioSource source;
    public AudioClip meteorFall;
    public AudioClip meteorImpact;

    private bool isMeteorFallPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        _parentParticleSystem = this.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_parentParticleSystem.particleCount > _currentNumberOfParticles)
        {
            source.PlayOneShot(meteorFall);
            isMeteorFallPlaying = true;

            // Start a coroutine to wait for meteorFall to finish before playing meteorImpact.
            StartCoroutine(PlayImpactAfterFall());
        }

        _currentNumberOfParticles = _parentParticleSystem.particleCount;
    }
    
    private IEnumerator PlayImpactAfterFall()
    {
        // Wait for the duration of the meteorFall clip.
        yield return new WaitForSeconds(meteorFall.length);

        // Play meteorImpact sound.
        source.PlayOneShot(meteorImpact);

        // Reset the flag to allow another meteorFall sound to trigger if needed.
        isMeteorFallPlaying = false;
    }
}
