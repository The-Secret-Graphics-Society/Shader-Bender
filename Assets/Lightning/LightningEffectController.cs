using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffectController : MonoBehaviour
{
    [SerializeField] private Transform LightningSource;
    [SerializeField] private Transform LightningTarget;

    [SerializeField] private Shader LightningShader;
    [SerializeField] private LightningEffect _lightningScriptableObject;
    public Material material; // The material using the shader

    // lightning mesh
    private Mesh mesh;

    private const int VertexCount = 128;

    void Start()
    {
        if (material == null)
        {
            Debug.LogError("Material not assigned!");
            return;
        }

        // Create the mesh with 128 vertices and 127 edges
        mesh = new Mesh();

        // Generate vertices along the line between source and target
        Vector3[] vertices = new Vector3[VertexCount];
        for (int i = 0; i < VertexCount; i++)
        {
            float t = (float)i / (VertexCount - 1); // Normalize between 0 and 1
            vertices[i] = Vector3.Lerp(LightningSource.position, LightningTarget.position, t);
        }

        // Generate edges (indices)
        int[] indices = new int[(VertexCount - 1) * 2];
        for (int i = 0; i < VertexCount - 1; i++)
        {
            indices[i * 2] = i;
            indices[i * 2 + 1] = i + 1;
        }

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
        //// Dynamically update vertices if source or target moves
        //Vector3[] vertices = mesh.vertices;
        //for (int i = 0; i < VertexCount; i++)
        //{
        //    float t = (float)i / (VertexCount - 1); // Normalize between 0 and 1
        //    vertices[i] = Vector3.Lerp(LightningSource.position, LightningTarget.position, t);
        //}

        //mesh.vertices = vertices;
        //mesh.RecalculateBounds();

        // Pass the source and target to the shader
        material.SetVector("_SourcePoint", LightningSource.position);
        material.SetVector("_TargetPoint", LightningTarget.position);
    }
}
