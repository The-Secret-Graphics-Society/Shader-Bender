using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEruption : MonoBehaviour
{
    public ParticleSystem fireParticles;
    private ParticleSystem system;
    public float rad = 3f;
    public float innerRad = 1f;
    public int particleCount = 1;
    private bool interaction;
    public float eruptDuration = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        interaction = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            interaction = true;
        }
        // When the interaction criteria is met, we spawn in the fireEruption
        if (interaction)
        {
            StartCoroutine(EruptinTorusShape());
            Debug.Log("Started coroutine: EruptinTorusShape");
            interaction = false;
        }
    }

    private IEnumerator EruptinTorusShape()
    {
        Debug.Log("Started Coroutine");
        for (int i = 0; i < particleCount; i++)
        {
            // Any parameters we assign in emitParams will override the current particle systems when we call Emit.
            var emitParams = new ParticleSystem.EmitParams();

            // Generate random positions on a flat torus shape
            Vector2 randomPoint = Random.insideUnitCircle.normalized * Random.Range(innerRad, rad);

            // Set particle position (flat on XZ plane)
            emitParams.position = new Vector3(randomPoint.x, 0f, randomPoint.y) + transform.position;

            // Emit the particle
            fireParticles.Emit(emitParams, 1);
            Debug.Log($"Particle emitted at position: {emitParams.position}");

            // Calculate a random delay
            float randomDelay = Random.Range(0f, eruptDuration / particleCount);
            yield return new WaitForSeconds(randomDelay);
        }
    }
}
