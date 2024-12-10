using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Controls the AoE wind power by defining it redious and the duration of the attack.
/// Internally, it controlls the shader that drives the wind look
/// </summary>
public class AoEWindController : MonoBehaviour
{
    #region AoE Mechanics
    [Tooltip("The SO that interfaces with Media Pipe")]
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [Tooltip("The radious of the dome")]
    [Range(1, 5)]
    [SerializeField] private float _areaOfEffect = 0.25f;

    [Tooltip("The time in seconds the effect last")]
    [Range(1, 10)]
    [SerializeField] private float _effectDuration = 5;
    [SerializeField] private bool _effectActive = false;

    //_SampleXYZOffSet
    [Tooltip("Controls the shade of the noise")]
    [SerializeField] private Vector3 _sampleXYZOffSet = Vector3.one;
    //_BaseColor
    [Tooltip("Color of the smoke")]
    [SerializeField] private Color _baseColor = Color.white;
    //_WindSpeed
    [Tooltip("Speed of the smoke")]
    [Range(10, 80)]
    [SerializeField] private float _windSpeed = 10.0f;

    //SoundFX
    [SerializeField]
    private AudioSource _windSound;

    private delegate void AoEWindControllerDelegate();

    private event AoEWindControllerDelegate OnAoEActivated;

    #endregion AoE Mechanics

    #region Shader parameters
    private Material _windMaterial;
    private MeshRenderer _meshRenderer;

    [Tooltip("Debug only")]
    [Range(0.0f, 0.89f)]
    [SerializeField]
    private float _visibility;
    [Tooltip("How fast the wind fades in and out")]
    [Range(0.01f, 2.0f)]
    [SerializeField] private float _visibilityDelta = 0.8f;
    private float _effectCounter;
    [Tooltip("Read only")]
    [SerializeField] private bool _effectInProgress;
    private bool _hasFadedIn = false;
    private bool _hasFadedOut = false;

    [Tooltip("Enable changes on the shader at run-time")]
    [SerializeField] private bool _debugginShader = false;

    #endregion Shader parameters

    #region Unity MonoBehaviour
    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        ElementState.onAirActive += ActivateAirPower;
        ElementState.onElementDeactivate += DeactivateAirPower;
        OnAoEActivated += ActivateAoEWind;
    }

    private void OnDisable()
    {
        ElementState.onAirActive -= ActivateAirPower;
        ElementState.onElementDeactivate -= DeactivateAirPower;
        OnAoEActivated -= ActivateAoEWind;
    }

    private void Update()
    {
        if (_debugginShader) // Disables mechanics and allows to change the parameters of the shader
        {
            UpdateShaderVariables();
            return;
        }
        
        AoEMechanicsLogic();
        UpdateWindDoneCenter();
        UpdateWindDoneAoE();

    }
    #endregion Unity MonoBehaviour

    #region AoE Mechanics

    /// <summary>
    /// Set the shader variables and checks for the scriptable object
    /// </summary>
    private void Init()
    {
        if (_debugginShader)
            _visibility = 0.99f;
        else
            _visibility = 0.0f;

        //_areaOfEffect = (transform.localScale * 0.25f).magnitude;
        _windMaterial = GetComponentInChildren<MeshRenderer>().material;
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
        UpdateShaderVariables();

        _windSound = GetComponent<AudioSource>();
        _windSound.loop = false;

        if (!_poseScriptableObject)
            throw new NullReferenceException("The PoseScriptableObject is missing on the AoEWindController");
    }


    private void ActivateAirPower()
    {
        _effectActive = true;
    }

    private void DeactivateAirPower()
    {
        _effectActive = false;
    }

    /// <summary>
    /// Holds the mechanics based on the positions of the hands
    /// </summary>
    private void AoEMechanicsLogic()
    {

        if (_effectActive && !_effectInProgress)
            _effectInProgress = true;
        

        if (!_effectActive)
        {
            //Debug.Log("Wind ACTIVE");
            _effectInProgress = false;
            if (FadeOutVisibility())
            {
                _effectCounter = 0.0f;
                _hasFadedIn = false;
                _hasFadedOut = false;
                _windSound.Stop();
            }
        }
        _meshRenderer.enabled = _effectInProgress || _visibility > 0.0f;

        if (_effectInProgress)
            OnAoEActivated?.Invoke();
    }
    private void ActivateAoEWind()
    {
        FadeInVisibility();
        _effectCounter += 1.0f * Time.deltaTime;
        _windSound.Play();
        if (_effectCounter > _effectDuration)
            if (FadeOutVisibility())
            {
                _effectInProgress = false;
                _effectCounter = 0.0f;
                _hasFadedIn = false;
                _hasFadedOut = false;
            }

#if UNITY_EDITOR
        //Debug.Log($"_effectInProgress: {_effectInProgress}");
        //Debug.Log($"_visibility: {_visibility} _effectCounter: {_effectCounter}");
#endif
    }

    #endregion AoE Mechanics

    #region Shader controllers
    /// <summary>
    /// Updates fields on the shader
    /// </summary>
    private void UpdateShaderVariables()
    {
        _windMaterial.SetFloat("_Visibility", _visibility);
        _windMaterial.SetVector("_Center", transform.position);
        _windMaterial.SetVector("_SampleXYZOffSet", _sampleXYZOffSet);
        _windMaterial.SetVector("_BaseColor", _baseColor);
    }

    private void UpdateWindDoneAoE()
    {
        _windMaterial.SetFloat("_Radius", _areaOfEffect);
    }

    private void UpdateWindDoneCenter()
    {
        _windMaterial.SetVector("_Center", transform.position);
        _windMaterial.SetVector("_SampleXYZOffSet", _sampleXYZOffSet);
        _windMaterial.SetVector("_BaseColor", _baseColor);
        _windMaterial.SetFloat("_WindSpeed", _windSpeed);
    }
    private bool FadeInVisibility()
    {
        if (_hasFadedIn) return true;

        _visibility += _visibilityDelta * _visibilityDelta * Time.deltaTime;
        if (_visibility >= 0.89f)
        {
            _visibility = 0.89f;
            _hasFadedIn = true;
        }

        _windMaterial.SetFloat("_Visibility", _visibility);

        return false;

    }

    private bool FadeOutVisibility()
    {
        if (_hasFadedOut) return true;

        _visibility -= _visibilityDelta * _visibilityDelta * Time.deltaTime;
        if (_visibility <= 0)
        {
            _visibility = 0.0f;
            _hasFadedOut = true;
        }

        _windMaterial.SetFloat("_Visibility", _visibility);

        return false;
    }
    #endregion Shader controllers

}
