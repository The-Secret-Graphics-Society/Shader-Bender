using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WindSway : MonoBehaviour
{
    public float freq;
    public float windForce = 0.1f;
    public float ratio = 1.0f;
    public float originY = 0.2f;
    public bool originDirection; //true is up, false is down
    public float distanceMultiplier = 2.0f;
    public float angleRadians;
    public bool calculateOrigin = true;
    public bool calculateYAxisScale = true;
    public bool calculateAngle = true;

    private float _startFreq;
    private float _startWindForce;
    private float _startRatio;
    private float _startDistanceMultiplier;
    private float yAxisScale = 1.0f;
    public float localSpaceHeight = 0.0f;
    private Matrix4x4 worldToLocal;
    private Matrix4x4 localToWorld;
    private float aimfreq;
    private float aimwindForce;
    private float aimratio;
    private float aimdistanceMultiplier;
    private IEnumerator transitionCoroutine;
    private MeshRenderer renderer;
    private Material[] materials;

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
        if (calculateOrigin || calculateYAxisScale)
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;

            if (mesh != null)
            {
                Vector3 yMax = Vector3.zero;
                Vector3 yMin = Vector3.zero;
                Vector3[] verts = mesh.vertices;

                foreach (Vector3 vert in verts)
                {
                    if (vert.y < yMin.y) yMin = vert;
                    if (vert.y > yMax.y) yMax = vert;
                }

                originY = calculateOrigin ? ((originDirection ? yMax : yMin) - transform.localPosition).y : originY;
                float yDifference = Mathf.Abs(transform.TransformPoint(yMax - yMin).y);
                if (yDifference != 0) yAxisScale = calculateYAxisScale ? yDifference : 1.0f;
            }
        }

        worldToLocal = transform.worldToLocalMatrix;
        localToWorld = transform.localToWorldMatrix;

        _startFreq = freq;
        _startWindForce = windForce;
        _startRatio = ratio;
        _startDistanceMultiplier = distanceMultiplier;

        renderer = GetComponent<MeshRenderer>();
        Vector3 centerPoint = renderer.bounds.center;
        originY = centerPoint.y - (calculateOrigin ? originY : (transform.localPosition.y - originY));
        localSpaceHeight = transform.InverseTransformPoint(centerPoint.x, originY, centerPoint.z).y;
        angleRadians = calculateAngle ? Mathf.Atan2(centerPoint.z, centerPoint.x) : angleRadians;

        materials = renderer.materials;
        UpdateMaterials();

        currentState = state.ambient;
        onStateChanged += StateChanged;

        ElementState.onAirActive += AoEActivated;
        ElementState.onElementDeactivate += AoEDeactivated;
    }

    void Update()
    {
        UpdateMaterials();
    }

    private void StateChanged(state state)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        if (currentState == state.ambient) SetLow();
        if (currentState == state.high) SetHigh();

        transitionCoroutine = SmoothTransition();
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

    private void SetHigh()
    {
        if (objectType == oType.lightstrip)
        {
            aimfreq = 44.5f;
            aimwindForce = 0.22f;
            aimratio = 0.25f;
            aimdistanceMultiplier = 1.0f;

        }
        if (objectType == oType.plant)
        {
            aimfreq = 44.5f;
            aimwindForce = 1.0f;
            aimratio = 0.25f;
            aimdistanceMultiplier = 1.0f;
        }
    }

    private void SetLow()
    {
        //aimdirection = _startDirection;
        aimfreq = _startFreq;
        aimwindForce = _startWindForce;
        aimratio = _startRatio;
        aimdistanceMultiplier = _startDistanceMultiplier;
    }

    private IEnumerator SmoothTransition()
    {
        float moveSpeed = 1.0f;
        float threshold = 0.01f;

        while (true)
        {

            freq = Mathf.MoveTowards(freq, aimfreq, moveSpeed * 25.0f * Time.deltaTime);
            windForce = Mathf.MoveTowards(windForce, aimwindForce, moveSpeed * Time.deltaTime);
            ratio = Mathf.MoveTowards(ratio, aimratio, moveSpeed * Time.deltaTime);
            distanceMultiplier = Mathf.MoveTowards(distanceMultiplier, aimdistanceMultiplier, moveSpeed * Time.deltaTime);

            if (Mathf.Abs(freq - aimfreq) < threshold &&
                Mathf.Abs(windForce - aimwindForce) < threshold &&
                Mathf.Abs(ratio - aimratio) < threshold &&
                Mathf.Abs(distanceMultiplier - aimdistanceMultiplier) < threshold)
            {
                break;
            }

            yield return null;
        }
    }

    private void UpdateMaterials()
    {
        foreach (Material mat in materials)
        {
            if (!mat.HasFloat("_CanSway")) continue;
            if (mat.GetFloat("_CanSway") < 0.5) continue;
            mat.SetFloat("_Frequency", freq);
            mat.SetFloat("_WindForce", windForce);
            mat.SetFloat("_Ratio", ratio);
            mat.SetFloat("_OriginY", originY);
            mat.SetFloat("_DistanceMultiplier", distanceMultiplier);
            mat.SetFloat("_Direction", angleRadians);
            mat.SetFloat("_OriginDirection", originDirection ? 1.0f : 0.0f);
            mat.SetFloat("_TimeScale", 1);
            mat.SetFloat("_YAxisScale", yAxisScale);
            mat.SetMatrix("_WorldToLocal", worldToLocal);
            mat.SetMatrix("_LocalToWorld", localToWorld);
        }

        renderer.materials = materials;
    }
}
