using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LightningParticleSoundSystem : MonoBehaviour
{
    private ParticleSystem _parentParticleSystem;

    private int _currentNumberOfParticles = 0;

    public AudioSource source;
    public AudioClip BornSound;
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
            source.PlayOneShot(BornSound);
        }

        _currentNumberOfParticles = _parentParticleSystem.particleCount;
    }
}
