using System.Collections.Generic;
using UnityEngine;

public class SnakeMeshConstructor : MonoBehaviour
{
    [SerializeField] private SnakeBodyHandler bodyHandler;

    public MeshFilter meshFilter;
    Mesh mesh;

    private void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    // Start is called before the first frame update
    private void LateUpdate()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        int numVerticesPerSegment = 0;
        float segmentRadius = 2f;

        List<SnakeBodySegment> bodySegments = bodyHandler.bodySegments;
        for (int i = 1; i < bodySegments.Count - 1; ++i)
        {
            // We construct our cylinder about the visual portions of the body segment, not the root.
            // This is to account for self intersection and other adjustments we want to make
            // that don't influence the snake's path but are still necessary to show visually.
            Transform prev = bodySegments[i - 1].visual;
            Transform curr = bodySegments[i].visual;
            Transform next = bodySegments[i + 1].visual;

            Vector3 currToPrev = (prev.transform.position - curr.transform.position);
            Vector3 currToNext = (next.transform.position - curr.transform.position);

            // Find the cross product to determine what axis our points should be constructed along
            Vector3 R = Vector3.up; // Only works when Snake is flat
            R = curr.transform.right.normalized;
            R.Normalize();

            Vector3 S = Vector3.Cross(R, currToNext);
            S.Normalize();

            int taperBegin = transform.childCount / 4;
            int taperEnd = transform.childCount;
            float taperModifier = 1.0f;

            // TODO bdsowers : Follow a curve, don't be so linear
            if (i >= taperBegin)
            {
                int taperDistance = i - taperBegin;
                int totalTaperDistance = (taperEnd - taperBegin);
                float normalized = (float)taperDistance / (float)totalTaperDistance;
                taperModifier = Mathf.Lerp(1, 0.25f, normalized);
            }

            float adjustedSegmentRadius = segmentRadius * taperModifier;
            bodySegments[i].thickness = adjustedSegmentRadius;

            // Construct the points
            numVerticesPerSegment = 0;
            for (int j = 0; j < 360; j += 4)
            {
                float rad = Mathf.Deg2Rad * j;

                Vector3 P = curr.transform.position + adjustedSegmentRadius * R * Mathf.Cos(rad) + adjustedSegmentRadius * S * Mathf.Sin(rad);
                vertices.Add(P);

                Vector3 normal = P - curr.transform.position;
                normal.Normalize();
                normals.Add(normal);

                // TODO bdsowers - this is pretty much meaningless
                Vector2 uv = Vector2.zero;
                uv.y = j / 360f;
                uv.x = i / transform.childCount * 10f;
                uvs.Add(uv);

                ++numVerticesPerSegment;
            }
        }


        // Construct all the indices now by connecting things together...
        for (int i = 1; i < transform.childCount - 2; ++i)
        {
            int startForThisSegment = (i - 1) * numVerticesPerSegment;
            int startForNextSegment = (i) * numVerticesPerSegment;

            for (int j = 0; j < numVerticesPerSegment; j++)
            {
                int v0 = startForThisSegment + j;
                int v1 = startForNextSegment + j;
                int v2 = startForThisSegment + (j + 1) % numVerticesPerSegment;

                indices.Add(v0);
                indices.Add(v1);
                indices.Add(v2);

                v0 = startForNextSegment + j;
                v1 = startForThisSegment + (j + 1) % numVerticesPerSegment;
                v2 = startForNextSegment + (j + 1) % numVerticesPerSegment;

                indices.Add(v0);
                indices.Add(v2);
                indices.Add(v1);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }
}
