using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBodyHandler : MonoBehaviour
{
    public GameObject head;
    public GameObject baseBodySegment;
    public int numBodySegments;
    public float separation;
    public Vector3 axis;

    List<Vector3> prevPositions = new List<Vector3>();
    Vector3 headPrevPosition;

    float movementAmountTracker = 0f;

    public enum MoveDirection
    {
        Forward,
        Backward,
    }

    MoveDirection currentMoveDirection = MoveDirection.Forward;

    // Start is called before the first frame update
    void Start()
    {
        prevPositions.Add(head.transform.localPosition);
        headPrevPosition = head.transform.localPosition;

        baseBodySegment.transform.localPosition = head.transform.localPosition + separation * axis;
        prevPositions.Add(baseBodySegment.transform.localPosition);

        for (int i = 0; i < numBodySegments - 1; ++i)
        {
            GameObject newSegment = GameObject.Instantiate(baseBodySegment, transform);
            newSegment.transform.localPosition = baseBodySegment.transform.localPosition + (i + 1) * separation * axis;
            prevPositions.Add(newSegment.transform.localPosition);
        }
    }

    public void ReverseDirection()
    {
        if (currentMoveDirection == MoveDirection.Forward)
            currentMoveDirection = MoveDirection.Backward;
        else
            currentMoveDirection = MoveDirection.Forward;

        // Get an updated current snapshot of these.
        UpdatePrevPositions();
    }

    private void MoveBackward(float movementAmount)
    {
        // The head is still 'driving', but instead of pulling everything toward
        // its position, it pushes everything away.
        for (int i = 1; i < numBodySegments; ++i)
        {
            Transform bodySegment = transform.GetChild(i);

            Vector3 prevPosition = Vector3.zero;
            if (i != numBodySegments - 1)
            {
                prevPosition = prevPositions[i + 1];
            }
            else
            {
                // Infer the last target position when we're moving backward
                Vector3 dir = bodySegment.transform.localPosition - transform.GetChild(i - 1).transform.localPosition;
                prevPosition = bodySegment.transform.localPosition + dir * 3f;
            }

            Vector3 direction = (prevPosition - bodySegment.localPosition).normalized;

            float dist = Vector3.Magnitude(prevPosition - bodySegment.localPosition);
            float amountToMove = Mathf.Min(dist, movementAmount);

            bodySegment.localPosition += direction * amountToMove;

            bodySegment.transform.LookAt(bodySegment.transform.position + -direction);
        }

        // We're trying to stay roughly "separationDistance" away from each other, so we want to
        // update prevPosition when we've moved enough.
        movementAmountTracker += movementAmount;
        if (movementAmountTracker >= separation)
        {
            UpdatePrevPositions();
        }
    }

    private void MoveForward(float movementAmount)
    {
        // Move every body part from its current position to the previous position of the next body
        // part in the chain.
        for (int i = 1; i < numBodySegments; ++i)
        {
            Transform bodySegment = transform.GetChild(i);
            Vector3 direction = (prevPositions[i - 1] - bodySegment.localPosition).normalized;
            float dist = Vector3.Magnitude(prevPositions[i - 1] - bodySegment.localPosition);
            float amountToMove = Mathf.Min(dist, movementAmount);

            bodySegment.localPosition += direction * amountToMove;
        }

        // We're trying to stay roughly "separationDistance" away from each other, so we want to
        // update prevPosition when we've moved enough.
        movementAmountTracker += movementAmount;
        if (movementAmountTracker >= separation)
        {
            UpdatePrevPositions();
        }
    }

    private void UpdateLookDirection()
    {
        for (int i = 1; i < numBodySegments; ++i)
        {
            Transform bodySegment = transform.GetChild(i);
            Transform prevSegment = transform.GetChild(i - 1);

            // TODO bdsowers - is this right??
            //bodySegment.transform.LookAt(bodySegment.transform.position + direction);

            Vector3 dir = (prevSegment.GetChild(0).position - bodySegment.GetChild(0).position);
            bodySegment.GetChild(0).transform.LookAt(bodySegment.transform.position - dir);

            Debug.DrawLine(bodySegment.transform.position, bodySegment.transform.position + dir, Color.red);
        }
    }

    private void UpdatePrevPositions()
    {
        movementAmountTracker = 0;

        for (int i = 0; i < numBodySegments; ++i)
        {
            prevPositions[i] = transform.GetChild(i).localPosition;
        }
    }

    private void LateUpdate()
    {
        // Find some way to 'normalize' this?
        float movementAmount = Vector3.Magnitude(head.transform.localPosition - headPrevPosition);
        headPrevPosition = head.transform.localPosition;

        if (currentMoveDirection == MoveDirection.Forward)
        {
            MoveForward(movementAmount);
        }
        else
        {
            MoveBackward(movementAmount);
        }

        UpdateLookDirection();
    }


}


