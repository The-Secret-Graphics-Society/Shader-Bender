using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WindSway : MonoBehaviour
{
    Mesh mesh;
    Vector3[] verts;
    Vector3[] originalVerts;

    Vector3 initTestPosition; 

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
    
    public state currentState {
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
        verts = mesh.vertices;
        originalVerts = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            originalVerts[i] = verts[i]; 
        }

        initTestPosition = transform.TransformPoint(verts[0]);
        currentState = state.ambient;
        onStateChanged += stateChanged;
        
        ElementState.onAirActive += AoEActivated;
        ElementState.onElementDeactivate += AoEDeactivated;

    }

    private void stateChanged(state state)
    {
        if(transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        
        if(currentState == state.ambient) setLow();
        if(currentState == state.high) setHigh();
        
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
        Debug.Log("AoEActivated");
        currentState = state.high;
    }
    
    private void AoEDeactivated()
    {
        Debug.Log("AoEDeactivated");
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
            
            freq = Mathf.MoveTowards(freq, aimfreq, moveSpeed*25.0f * Time.deltaTime);
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



    /*void OnDisable()
    {
        GameObject[] handles = GameObject.FindGameObjectsWithTag("handle");
        foreach (GameObject handle in handles)
        {
            DestroyImmediate(handle);
        }
    }*/

    float xSwayFunction(int i, float r, float f)
    {
        float  intensity =Mathf.Cos(direction) * windForce * ratio * Mathf.Sin(Time.time * f + originalVerts[i].y * Mathf.PI);
        float directionalStatic = Mathf.Cos(direction) * windForce;
        return directionalStatic + intensity;
    }
    float ySwayFunction(int i, float r, float f)
    {
        float  intensity =  Mathf.Sin(direction) * windForce * ratio * Mathf.Sin(Time.time * f + originalVerts[i].y * Mathf.PI);
        float directionalStatic = Mathf.Sin(direction) * windForce;
        return directionalStatic + intensity;
    }
    // void OnEnable(){
    //     onStateChanged += 
    // }
    // void OnDisable(){
    //     
    // }

    void Update()
    {
        
        // if (Input.GetKeyDown("space"))
        // {
        //     if (currentState == state.ambient)
        //     {
        //         currentState = state.high;
        //     }
        //     else if (currentState == state.high)
        //     {
        //         currentState = state.ambient;
        //     }
        // }
        
        float newFreq =  freq / 2  + Mathf.PerlinNoise(Time.time * 0.1f, 0.0f) /2.0f;
        for (int i = 0; i < verts.Length; i++)
        {
            float distanceFromOrigin = verts[i].y - originY;
            if ((originDirection ? originY < verts[i].y : originY > verts[i].y)) continue;
            
            UnityEngine.Random.InitState((int)((verts[i].x + verts[i].y)*64));
            float randomValue = UnityEngine.Random.Range(0, 0.01f);
            
            verts[i] = originalVerts[i] + distanceMultiplier * Mathf.Pow(distanceFromOrigin, 2) * new Vector3(xSwayFunction(i, randomValue, newFreq), 0.0f, ySwayFunction(i, randomValue, newFreq)); 
        }
        
        //verts[0] += new Vector3(0.1f, 0.0f, 0.0f) * Time.deltaTime;
        mesh.vertices = verts;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
    }
}
