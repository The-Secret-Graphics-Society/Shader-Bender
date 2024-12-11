using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class WindSway : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verts;
    private Vector3[] originalVerts;
    private float[] randomOffsets;

    public float freq;
    public float windForce = 0.1f;
    public float ratio = 1.0f;

    public float originY = 0.2f;
    public bool originDirection; //true is up, false is down

    public float distanceMultiplier = 2.0f;
    public float direction; // goes from 0 to 2*PI, wind direction

    private float _startFreq;
    private float _startWindForce;
    private float _startRatio;
    private float _startDistanceMultiplier;
    private float _startDirection;

    private float aimfreq;
    private float aimwindForce;
    private float aimratio;
    private float aimdistanceMultiplier;
    private float aimdirection;

    private IEnumerator transitionCoroutine;

    public enum oType
    {
        plant,
        lightstrip
    }
    public oType objectType;

    public Action<state> onStateChanged;
    public enum state
    {
        ambient,
        high
    }

    private state _currentState;

    public state currentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            onStateChanged?.Invoke(value);
        }
    }

    void Start()
    {
        _startFreq = freq;
        _startWindForce = windForce;
        _startRatio = ratio;
        _startDistanceMultiplier = distanceMultiplier;
        _startDirection = direction;

        mesh = GetComponent<MeshFilter>().mesh;
        mesh.MarkDynamic();
        verts = mesh.vertices;
        originalVerts = new Vector3[verts.Length];
        randomOffsets = new float[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            originalVerts[i] = verts[i];
            float randomValue = UnityEngine.Random.Range(0f, 0.01f);
            randomOffsets[i] = randomValue;
        }

        currentState = state.ambient;
        onStateChanged += stateChanged;

        ElementState.onAirActive += AoEActivated;
        ElementState.onElementDeactivate += AoEDeactivated;
    }

    private void stateChanged(state state)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        if (currentState == state.ambient) setLow();
        if (currentState == state.high) setHigh();

        transitionCoroutine = smoothTransition();
        StartCoroutine(transitionCoroutine);
    }

    private void OnDisable()
    {
        ElementState.onAirActive -= AoEActivated;
        ElementState.onElementDeactivate -= AoEDeactivated;
    }

    private void AoEActivated()
    {
        currentState = state.high;
    }

    private void AoEDeactivated()
    {
        currentState = state.ambient;
    }

    private void setHigh()
    {
        if (objectType == oType.lightstrip)
        {
            aimfreq = 44.5f;
            aimwindForce = 0.22f;
            aimratio = 0.25f;
            aimdistanceMultiplier = 1.0f;
            //aimdirection = 6.17f;

        }
        if (objectType == oType.plant)
        {
            aimfreq = 44.5f;
            aimwindForce = 1.0f;
            aimratio = 0.25f;
            aimdistanceMultiplier = 1.0f;
            //aimdirection = 6.17f;
        }
    }

    private IEnumerator smoothTransition()
    {
        float moveSpeed = 1.0f;
        float threshold = 0.01f;

        while (true)
        {

            freq = Mathf.MoveTowards(freq, aimfreq, moveSpeed * 25.0f * Time.deltaTime);
            windForce = Mathf.MoveTowards(windForce, aimwindForce, moveSpeed * Time.deltaTime);
            ratio = Mathf.MoveTowards(ratio, aimratio, moveSpeed * Time.deltaTime);
            distanceMultiplier = Mathf.MoveTowards(distanceMultiplier, aimdistanceMultiplier, moveSpeed * Time.deltaTime);
            //direction = Mathf.MoveTowards(direction, aimdirection, moveSpeed*10.0f * Time.deltaTime);

            if (Mathf.Abs(freq - aimfreq) < threshold &&
                Mathf.Abs(windForce - aimwindForce) < threshold &&
                Mathf.Abs(ratio - aimratio) < threshold &&
                Mathf.Abs(distanceMultiplier - aimdistanceMultiplier) < threshold) // &&
                                                                                   //Mathf.Abs(direction - aimdirection) < threshold)
            {
                break;
            }

            yield return null;
        }
    }

    private void setLow()
    {
        //aimdirection = _startDirection;
        aimfreq = _startFreq;
        aimwindForce = _startWindForce;
        aimratio = _startRatio;
        aimdistanceMultiplier = _startDistanceMultiplier;
    }

    private float xSwayFunction(float sinAngle, float r, float cosDir)
    {
        float intensity = cosDir * windForce * ratio * sinAngle;
        float directionalStatic = cosDir * windForce;
        return directionalStatic + intensity;
    }

    private float ySwayFunction(float sinAngle, float r, float sinDir)
    {
        float intensity = sinDir * windForce * ratio * sinAngle;
        float directionalStatic = sinDir * windForce;
        return directionalStatic + intensity;
    }

    void Update()
    {
        float newFreq = freq / 2 + Mathf.PerlinNoise(Time.time * 0.1f, 0.0f) / 2.0f;
        float cosDir = Mathf.Cos(direction);
        float sinDir = Mathf.Sin(direction);
        float timeFactor = Time.time * newFreq;
        for (int i = 0; i < verts.Length; i++)
        {
            float distanceFromOrigin = verts[i].y - originY;
            if (originDirection ? originY < verts[i].y : originY > verts[i].y) continue;
            float randomValue = randomOffsets[i];
            float sinAngle = Mathf.Sin(timeFactor + originalVerts[i].y * Mathf.PI);

            verts[i] = originalVerts[i] + distanceMultiplier * distanceFromOrigin * distanceFromOrigin * new Vector3(xSwayFunction(sinAngle, randomValue, cosDir), 0.0f, ySwayFunction(sinAngle, randomValue, sinDir));
        }

        mesh.vertices = verts;
    }
}
