using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        verts = mesh.vertices;
        originalVerts = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            originalVerts[i] = verts[i]; 
        }

        initTestPosition = transform.TransformPoint(verts[0]);
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

    void Update()
    {
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
