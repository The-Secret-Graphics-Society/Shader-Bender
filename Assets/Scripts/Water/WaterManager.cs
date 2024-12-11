using UnityEngine;
using System.Collections;

public class WaterManager : MonoBehaviour
{
    [Header("Hand Details")]
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [SerializeField] private ParticleSystem rightHandWater;
    [SerializeField] private ParticleSystem leftHandWater;
    [SerializeField] private GameObject rightHandCaustics;
    [SerializeField] private GameObject leftHandCaustics;
    [SerializeField, Tooltip("If we should play the water in both hands")]
    private bool waterInBothHands = true;

    [Header("Shader Inputs")]
    [SerializeField, Tooltip("Material which receives water dripping.")]
    private Material groundMaterial;
    [SerializeField, Tooltip("Material representing the caustic volume.")]
    private Material causticsMaterial;
    [SerializeField, Tooltip("If the floor has extra Y-axis rotation, not implemented right now.")]
    private float floorYRotation = 0f;
    [SerializeField, Tooltip("Material representing the caustic volume.")]
    private Transform mainLight;
    [SerializeField, Tooltip("Time for the caustics to fully enable or disable.")]
    private float causticActivationTime = 0.3f;
    [SerializeField, Tooltip("Caustic strength, needs to be set here.")]
    private float maxCausticStrength = 1.0f;

    // Private members
    private Material customRenderMaterial;
    private CustomRenderTexture wetnessTexture;
    private Coroutine causticCoroutine;
    private bool isWaterActive = false;
    private float causticTransformHeight = 0f;
    public float refractiveIndex = 1.33f; // Refractive index of water

    void Awake()
    {
        customRenderMaterial = new(Shader.Find("Custom/UpdateWetnessMap"));
        wetnessTexture = new CustomRenderTexture(1024, 1024, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        wetnessTexture.autoGenerateMips = false;
        wetnessTexture.updateMode = CustomRenderTextureUpdateMode.Realtime;
        wetnessTexture.material = customRenderMaterial;
        wetnessTexture.enableRandomWrite = true;
        wetnessTexture.doubleBuffered = true;
        wetnessTexture.Create();

        customRenderMaterial.SetTexture("_WetnessMap", wetnessTexture);
        customRenderMaterial.SetVector("_WorldOrigin", new Vector3(0f, 0f, 0f));
        customRenderMaterial.SetVector("_WorldScale", new Vector3(10f, 0f, 10f));
        customRenderMaterial.SetFloat("_WorldRotationY", floorYRotation * Mathf.Deg2Rad);

        if (groundMaterial != null) groundMaterial.SetTexture("_WetnessMap", wetnessTexture);
        if (causticsMaterial != null && mainLight != null) causticsMaterial.SetVector("_LightPosition", mainLight.position);
        if (causticsMaterial != null) causticsMaterial.SetFloat("_CausticsStrength", 0f);
        if (rightHandCaustics != null) causticTransformHeight = rightHandCaustics.transform.position.y;
    }

    void Start()
    {
        // Do nothing :)
    }

    private void OnEnable()
    {
        ElementState.onWaterActive += EnableWater;
        ElementState.onElementDeactivate += DisableWater;
    }

    private void OnDisable()
    {
        ElementState.onWaterActive -= EnableWater;
        ElementState.onElementDeactivate -= DisableWater;
    }

    void Update()
    {
        if (groundMaterial != null) groundMaterial.SetFloat("_WaterActive", isWaterActive ? 1.0f : 0.0f);
        if (customRenderMaterial != null) customRenderMaterial.SetFloat("_WaterActive", isWaterActive ? 1.0f : 0.0f);

        if (rightHandWater != null)
        {
            Vector3 rightHandPosition = _poseScriptableObject.GetCurrentLeftHandPosition();
            rightHandWater.gameObject.transform.position = rightHandPosition;
            //rightHandWater.gameObject.transform.rotation = _poseScriptableObject.leftHandRotation;
            if (customRenderMaterial != null) customRenderMaterial.SetVector("_RightHandPos", rightHandPosition);

            if (rightHandCaustics != null && isWaterActive) PositionCausticVolume(rightHandPosition, rightHandCaustics.transform);
        }

        if (leftHandWater != null)
        {
            Vector3 leftHandPosition = _poseScriptableObject.GetCurrentRightHandPosition();
            leftHandWater.gameObject.transform.position = leftHandPosition;
            //leftHandWater.gameObject.transform.rotation = _poseScriptableObject.rightHandRotation;
            if (customRenderMaterial != null) customRenderMaterial.SetVector("_LeftHandPos", leftHandPosition);

            if (leftHandCaustics != null && isWaterActive) PositionCausticVolume(leftHandPosition, leftHandCaustics.transform);
        }
    }

    void EnableWater()
    {
        if (rightHandWater != null)
        {
            rightHandWater.Play();
            isWaterActive = true;
        }
        if (leftHandWater != null && waterInBothHands)
        {
            leftHandWater.Play();
            isWaterActive = true;
        }

        StartCausticTransition(maxCausticStrength);
    }

    void DisableWater()
    {
        if (rightHandWater != null)
        {
            rightHandWater.Stop();
            isWaterActive = false;
        }
        if (leftHandWater != null)
        {
            leftHandWater.Stop();
            isWaterActive = false;
        }

        StartCausticTransition(0f);
    }

    private void StartCausticTransition(float targetStrength)
    {
        if (causticCoroutine != null)
        {
            StopCoroutine(causticCoroutine);
        }

        if (causticsMaterial != null)
        {
            float currentStrength = causticsMaterial.GetFloat("_CausticsStrength");
            causticCoroutine = StartCoroutine(CausticStrengthTransition(currentStrength, targetStrength, causticActivationTime));
        }
    }

    void PositionCausticVolume(Vector3 waterSphere, Transform causticVolume)
    {
        // Light direction and approximate the sphere normal
        Vector3 lightDirection = (waterSphere - mainLight.position).normalized;
        Vector3 sphereNormal = Vector3.down;

        // Calculate incidence angle and refracted direction
        float cosThetaIncident = Vector3.Dot(-lightDirection, sphereNormal);
        float sinThetaRefracted = Mathf.Sin(Mathf.Acos(cosThetaIncident)) / refractiveIndex;
        float cosThetaRefracted = Mathf.Sqrt(1 - sinThetaRefracted * sinThetaRefracted);
        Vector3 refractedDirection = lightDirection / refractiveIndex + (cosThetaRefracted - cosThetaIncident / refractiveIndex) * sphereNormal;
        refractedDirection.Normalize();

        // Project refracted ray to floor
        float t = (causticTransformHeight - waterSphere.y) / refractedDirection.y;
        Vector3 causticPosition = waterSphere + refractedDirection * t;
        causticVolume.position = new Vector3(causticPosition.x, causticTransformHeight, causticPosition.z);
    }

    private IEnumerator CausticStrengthTransition(float startStrength, float endStrength, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newStrength = Mathf.Lerp(startStrength, endStrength, t);

            if (causticsMaterial != null)
            {
                causticsMaterial.SetFloat("_CausticsStrength", newStrength);
            }

            yield return null;
        }

        if (causticsMaterial != null)
        {
            causticsMaterial.SetFloat("_CausticsStrength", endStrength);
        }

        causticCoroutine = null;
    }
}