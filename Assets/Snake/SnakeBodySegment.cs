using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBodySegment : MonoBehaviour
{
    // The core body segment always remains following the exact path of the snake head.
    // However, to handle self intersection, the "visual" portion used to link
    // together the cylinder is a separate child object.
    public Transform visual;

    // How thick the body is at this segment.
    public float thickness;

    // Whether the segment was pushed up this frame as a result of self-intersection.
    // Used to produce cleaner self intersection visuals.
    public bool pushedUpThisFrame;
}
