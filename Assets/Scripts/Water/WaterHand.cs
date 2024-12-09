using UnityEngine;

public class WaterHand : MonoBehaviour
{
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [SerializeField] private ParticleSystem rightHandWater;
    [SerializeField] private ParticleSystem leftHandWater;
    [SerializeField, Tooltip("If we should play the water in both hands")]
    private bool waterInBothHands = true;
    [SerializeField, Tooltip("If we want dripping on the floor.")]
    private Material groundMaterial;
    private Material customRenderMaterial;
    [SerializeField] private float floorYRotation = 25.641f;
    private CustomRenderTexture wetnessTexture;
    private bool isWaterActive = false;

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
            rightHandWater.gameObject.transform.position = _poseScriptableObject.GetCurrentLeftHandPosition();
            //rightHandWater.gameObject.transform.rotation = _poseScriptableObject.leftHandRotation;
            if (customRenderMaterial != null) customRenderMaterial.SetVector("_RightHandPos", rightHandWater.gameObject.transform.position);
        }
        if (leftHandWater != null)
        {
            leftHandWater.gameObject.transform.position = _poseScriptableObject.GetCurrentRightHandPosition();
            //leftHandWater.gameObject.transform.rotation = _poseScriptableObject.rightHandRotation;
            if (customRenderMaterial != null) customRenderMaterial.SetVector("_LeftHandPos", leftHandWater.gameObject.transform.position);
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
    }
}