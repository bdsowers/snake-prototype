using UnityEngine;
using UnityEngine.AI;

// SNAKE AI STATES:
// * Patrol - move from waypoint to waypoint
// * Alerted (general) - move to waypoints close to player
// * Alerted (player in sight) - move to a reasonable height, look at player, scream
// * Attacking - move toward player, scream periodically, bite when close
// * Attack Broadcast - same as Alerted, followed by an immediate lunge & attack. Happens randomly when chasing.
// * Patrol Scare - move up above trees, scream, turn head X times, screaming between each

// SNAKE AI RANDOM NOTES:
// * Speed varies depending on alerted & attacking state
// * Should 'lowering' always behave the way it is? Can we make it more graceful?
// * 'Slither' - how do we do this safely? Do we bother?
// * Player tries to climb away - get Snake as close as possible, raise, attack if in range
// * Moving forward while raised may look wonky? May look fine, but moving back after may look bad.

// CONSIDERATIONS:
// * Head twist limit (should it even have one? 180 degree turn could be scary)
// * Body is big enough to cross - "hop" over it when necessary? Reroute (probably not feasible)?
// * What happens when we can't hop over it?
// * Collision - player shouldn't be able to move through snake, but can she climb over it?
// * Collision - moving backward, how do we handle this gracefully if intersecting w/player?
// * Collision - how to most accurately represent shape w/out being too costly

// VISUALS:
// * Control via spline (may get wonky w/self intersections, small loops - try & find RogueKart code)
// * How to make cylinder follow spline appropriately?
// * How do we make this visually "creepy" ? Fur-covered snake? Human eyes?

// ARCHITECTURE:
// * SnakeConstructor - generates initial placement, may be tricky placing in the middle of a dense environment.
// * SnakeAIController - decision maker
// * SnakeAnimator - moves body segments to maintain shape; manages 'hopping' (visual only, don't mess w/agent)
// * Spline - connects body segments w/single spline
// * SnakeVisualizer - manages & updates mesh to follow spline

public class FollowNavPoints : MonoBehaviour
{
    // Spline
    // ExampleTentacle
    // SplineMeshTiling (cylinder, WetBlack, CurveSpace)

    NavMeshAgent agent;
    public Transform navPoints;
    int currentNavTarget;
    bool activelyNavigating = true;
    Vector3 headDirection = Vector3.up;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.destination = navPoints.GetChild(currentNavTarget).position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            activelyNavigating = !activelyNavigating;
            agent.enabled = activelyNavigating;
        }

        if (!activelyNavigating)
        {
            agent.transform.position += headDirection * Time.deltaTime * 6f;

            Vector3 pos = agent.transform.position;
            pos.y = Mathf.Clamp(pos.y, 0f, 20f);
            agent.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            headDirection = Vector3.down;
            transform.parent.GetComponent<SnakeBodyHandler>().ReverseDirection();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            headDirection = Vector3.up;
            transform.parent.GetComponent<SnakeBodyHandler>().ReverseDirection();
        }

        if (!activelyNavigating)
            return;

        Vector3 targetPosition = navPoints.GetChild(currentNavTarget).position;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance < 5f)
        {
            currentNavTarget = (currentNavTarget + 1) % navPoints.childCount;
            agent.destination = navPoints.GetChild(currentNavTarget).position;
        }
    }
}
