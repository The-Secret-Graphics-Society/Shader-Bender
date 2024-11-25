using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffectController : MonoBehaviour
{
    [SerializeField] private Transform LightningSource;
    [SerializeField] private Transform LightningTarget;

    [SerializeField] private Shader LightningShader;
    public Material material; // The material using the shader

    private Mesh mesh;

    void Start()
    {
        if (material == null)
        {
            Debug.LogError("Material not assigned!");
            return;
        }

        // Create a simple mesh with two vertices and one edge
        mesh = new Mesh();

        Vector3[] vertices = new Vector3[2]
        {
            LightningSource.position,
            LightningTarget.position
        };

        int[] indices = new int[2]
        {
            0, 1 // Edge connecting the two vertices
        };

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        // Assign the mesh to the MeshFilter
        GetComponent<MeshFilter>().mesh = mesh;

        // Set the material
        GetComponent<MeshRenderer>().material = material;
    }

    void Update()
    {
        UpdateLightningVertices();
    }

    private void UpdateLightningVertices()
    {
        // Update the mesh if the source or target points change
        mesh.vertices = new Vector3[2] { LightningSource.position, LightningTarget.position };
        mesh.RecalculateBounds();

        // Pass the source and target to the shader
        material.SetVector("_SourcePoint", LightningSource.position);
        material.SetVector("_TargetPoint", LightningTarget.position);
    }

}
