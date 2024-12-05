using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemLightning : MonoBehaviour
{
    // Could just have start/end, or have midpoint and upNormal
    struct LineSegment
    {
        public Vector3 start;
        public Vector3 end;
        public Vector3 upNormal;
    }

    [SerializeField] int _iterations = 5;
    [SerializeField] float _forkChance = 0;
    [SerializeField] float _jitterChance = 0.67f;
    [SerializeField] float _jitterDist = 0.1f;
    [SerializeField] Material material;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float animationTime;
    [SerializeField] private float animCycleOffset;
    [SerializeField] private float bezierControlPointDist;
    [SerializeField] private bool debug = false;


    private List<LineSegment> _lineSegmentList = new List<LineSegment>();
    private List<LineSegment> _newlineSegmentList = new List<LineSegment>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AnimateLightning());
    }

    IEnumerator AnimateLightning()
    {
        yield return new WaitForSeconds(animCycleOffset);
        while (true)
        {
            if (startPoint != null && endPoint != null)
            {
                BuildLightningMesh(_iterations, startPoint.position, endPoint.position);
            }
            yield return new WaitForSeconds(animationTime);
        }
    }

    private void BuildLightningMesh(int _iterations, Vector3 sp, Vector3 ep)
    {
        // Start with a single line segment
        LineSegment initialLineSegment = new LineSegment();
        LineSegment initialLineSegment2 = new LineSegment();
        LineSegment initialLineSegment3 = new LineSegment();

        Vector3 midpoint = sp + startPoint.forward * bezierControlPointDist;
        Vector3 midpoint1 = (midpoint + sp)/2f;
        Vector3 midpoint2 = (midpoint + ep)/2f;

        initialLineSegment.start = sp;
        initialLineSegment.end = midpoint1;
        initialLineSegment2.start = midpoint1;
        initialLineSegment2.end = midpoint2;
        initialLineSegment3.start = midpoint2;
        initialLineSegment3.end = ep;

        initialLineSegment.upNormal = Vector3.Normalize(initialLineSegment.end - initialLineSegment.start);
        initialLineSegment2.upNormal = Vector3.Normalize(initialLineSegment2.end - initialLineSegment2.start);
        initialLineSegment3.upNormal = Vector3.Normalize(initialLineSegment3.end - initialLineSegment3.start);
        _lineSegmentList.Add(initialLineSegment);
        _lineSegmentList.Add(initialLineSegment2);
        _lineSegmentList.Add(initialLineSegment3);

        if (debug)
        {
            Debug.DrawLine(initialLineSegment.start, initialLineSegment.end, Color.red, 1f);
            Debug.DrawLine(initialLineSegment2.start, initialLineSegment2.end, Color.red, 1f);
            Debug.DrawLine(initialLineSegment3.start, initialLineSegment3.end, Color.red, 1f);
        }

        for (int i = 0; i < _iterations; i++)
        {
            foreach (LineSegment linesegment in _lineSegmentList)
            {
                bool willFork = Random.value < _forkChance;
                Jitter(linesegment, willFork);
            }
            // close previous linesegmentList
            _lineSegmentList.Clear();
            _lineSegmentList = new List<LineSegment>(_newlineSegmentList);
            _newlineSegmentList.Clear();
        }

        // Build the mesh
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();



        // [DEBUG] Draw the line segments
        for (int i = 0; i < _lineSegmentList.Count; i++)
        {
            LineSegment linesegment = _lineSegmentList[i];

            if (debug) Debug.DrawLine(linesegment.start, linesegment.end, Random.ColorHSV(), 1f);
           
            vertices.Add(linesegment.start);
            vertices.Add(linesegment.end);
            int startIndex = vertices.Count - 2;
            int endIndex = vertices.Count - 1;
            indices.Add(startIndex);
            indices.Add(endIndex);
        }
        _lineSegmentList.Clear();

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Apply the material to the MeshRenderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }


    // Could simplify to make fork a variant of jitter...
    private void Jitter(LineSegment linesegment, bool fork)
    {
        // generate two linesegments from the input line segment
        LineSegment jitteredLineSegment1 = new LineSegment();
        LineSegment jitteredLineSegment2 = new LineSegment();

        jitteredLineSegment1.start = linesegment.start;

        // Create an arbitrary vector that's not parallel to upNormal
        Vector3 arbitrary = (Mathf.Abs(linesegment.upNormal.x) < 0.9f) ? Vector3.right : Vector3.up;
        Vector3 perpVector = Vector3.Cross(linesegment.upNormal, arbitrary).normalized;
        float randomAngle1 = Random.value * 360;
        Quaternion randomRotation1 = Quaternion.AngleAxis(randomAngle1, linesegment.upNormal);

        perpVector = Vector3.Normalize(randomRotation1 * perpVector);

        // midpoint is the average of the start and end points offset by a random amount along the upNormal
        Vector3 jitterOffset = perpVector * _jitterDist * Vector3.Magnitude(linesegment.start - linesegment.end);
        Vector3 midpoint = (linesegment.start + linesegment.end) / 2 + jitterOffset;

        jitteredLineSegment1.end = midpoint;
        jitteredLineSegment2.start = midpoint;

        jitteredLineSegment2.end = linesegment.end;

        // Calculate the upNormal for the new line segments
        jitteredLineSegment1.upNormal = Vector3.Normalize(jitteredLineSegment1.end - jitteredLineSegment1.start);
        jitteredLineSegment2.upNormal = Vector3.Normalize(jitteredLineSegment2.end - jitteredLineSegment2.start);

        _newlineSegmentList.Add(jitteredLineSegment1);
        _newlineSegmentList.Add(jitteredLineSegment2);

        if (fork)
        {
            LineSegment forkedLineSegment = new LineSegment();
            forkedLineSegment.start = midpoint;
            forkedLineSegment.end = midpoint + jitteredLineSegment1.end - jitteredLineSegment1.start;
            forkedLineSegment.upNormal = Vector3.Normalize(forkedLineSegment.end - forkedLineSegment.start);

            _newlineSegmentList.Add(forkedLineSegment);
        }
    }

}
