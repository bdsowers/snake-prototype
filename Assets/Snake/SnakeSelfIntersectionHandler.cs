using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSelfIntersectionHandler : MonoBehaviour
{
    [SerializeField] private SnakeBodyHandler bodyHandler;

    private bool SegmentIntersectsSphere(
        Vector3 p1, Vector3 p2,
        Vector3 sphereCenter, float sphereRadius,
        out float d1, out float d2)
    {
        d1 = 0;
        d2 = 0;

        Vector3 direction = (p2 - p1).normalized;
        float mag = direction.magnitude;

        float o = (p1 - sphereCenter).magnitude;
        float alpha = Vector3.Dot(direction, p1 - sphereCenter);
        float theta = alpha * alpha - (o * o - sphereRadius * sphereRadius);

        if (theta < 0)
            return false;

        d1 = -alpha + Mathf.Sqrt(theta);
        d2 = -alpha - Mathf.Sqrt(theta);

        // If d1 is out of range, there's only an intersection if d2 is in range
        if (d1 < 0 || d1 > 1)
            return d2 >= 0 && d2 <= 1;

        // If d2 is out of range, there's only an intersection if d1 is in range
        if (d2 < 0 || d2 > 1)
            return d1 >= 0 && d1 <= 1;

        // If d1 && d2 are in range, there's an intersection
        return true;
    }

    private void Update()
    {
        ResolveIntersections();
    }

    // TODO bdsowers : This could all be done trivially in a job, which is good because it's
    // roughly an O(N^2) operation.
    private void ResolveIntersections()
    {
        List<SnakeBodySegment> bodySegments = bodyHandler.bodySegments;
        for (int i = 0; i < bodySegments.Count; ++i)
        {
            bodySegments[i].pushedUpThisFrame = false;
        }

        // Handle pushing up anything that's self intersecting
        for (int i = 0; i < bodySegments.Count; ++i)
        {
            Vector3 sphereCenter = bodySegments[i].transform.localPosition;
            float sphereRadius = 4f;

            Transform visTrans = bodySegments[i].visual;
            Vector3 visPos = visTrans.localPosition;

            bool intersection = false;
            for (int j = i + 10; j < bodySegments.Count - 1; ++j)
            {
                Vector3 p1 = bodySegments[j].transform.localPosition;
                Vector3 p2 = bodySegments[j + 1].transform.localPosition;

                float d1 = 0, d2 = 0;
                if (SegmentIntersectsSphere(p1, p2, sphereCenter, sphereRadius, out d1, out d2))
                {
                    intersection = true;

                    //Debug.DrawLine(transform.GetChild(i).position, transform.GetChild(i).position + Vector3.up * 5, Color.blue);
                }
            }

            if (intersection)
            {
                // TODO bdsowers : "Double intersections" are rare but possible and need to be accounted for
                // in height limit. However, simply counting intersections doesn't work, need a smarter
                // solution.

                // TODO bdsowers : Tapering needs to be accounted for in height limit, maybe propogate that
                // down to the segments so they can notify this.
                visPos += Vector3.up * Time.deltaTime * 20f;
                visPos.y = Mathf.Min(visPos.y, 3f);
                visTrans.localPosition = visPos;
                bodySegments[i].pushedUpThisFrame = true;

                // TODO bdsowers
                // Apply a dampening influence to some of the preceding points to not make the transition
                // so rough.
                // This doesn't feel quite right yet.
                for (int backtrack = 1; backtrack < 5; backtrack++)
                {
                    if (i + backtrack < transform.childCount)
                    {
                        Transform next = bodySegments[i + backtrack].visual;
                        Vector3 pos = next.localPosition;
                        pos += Vector3.up * Time.deltaTime * (20f - backtrack * 4) * 0.5f;
                        pos.y = Mathf.Min(pos.y, 3f);
                        next.localPosition = pos;
                        bodySegments[i + backtrack].pushedUpThisFrame = true;
                    }
                }
            }
        }

        // Handle pushing down anything that's in the air and shouldn't be
        for (int i = 0; i < bodySegments.Count; ++i)
        {
            Transform visTrans = bodySegments[i].visual;
            Vector3 visPos = visTrans.localPosition;

            if (!bodySegments[i].pushedUpThisFrame)
            {
                visPos += Vector3.down * Time.deltaTime * 5f;
                visPos.y = Mathf.Max(0f, visPos.y);
                visTrans.localPosition = visPos;
            }
        }
    }
}
