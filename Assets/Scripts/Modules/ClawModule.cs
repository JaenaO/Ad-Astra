using UnityEngine;

/// <summary>
/// Runtime claw controller that drives a configured FABRIK rig.
/// Wire target selection externally by calling TryStartGrab().
/// </summary>
public class ClawModule : MonoBehaviour
{
    private const int DefaultCreditReward = 10;

    [Header("Claw Configuration")]
    [SerializeField] private FABRIK solver;
    [SerializeField] private float maxReachDistance = 15f;
    [SerializeField] private float extensionDuration = 2.5f;
    [SerializeField] private float holdDuration = 1.0f;
    [SerializeField] private float retractionDuration = 2.5f;
    [SerializeField] private float cooldownDuration = 1.0f;
    [SerializeField] private float grabRadius = 1.0f;

    private enum ClawState
    {
        Idle,
        Extending,
        Holding,
        Retracting,
        Cooldown,
    }

    private ClawState state = ClawState.Idle;
    private float stateTimer;

    private GameObject targetAsteroid;
    private AsteroidData targetAsteroidData;
    private Rigidbody targetRigidbody;
    private Vector3 extensionTarget;
    private Vector3 extensionOrigin;
    private Vector3 initialGripRestTarget;
    private bool grabbed;

    public bool IsAvailable => state == ClawState.Idle;
    public bool IsBusy => state == ClawState.Extending || state == ClawState.Holding || state == ClawState.Retracting;
    public bool IsOnCooldown => state == ClawState.Cooldown;

    private void Awake()
    {
        EnsureReferences();
        InitializeSolverState();
    }

    private void OnValidate()
    {
        maxReachDistance = Mathf.Max(0f, maxReachDistance);
        extensionDuration = Mathf.Max(0f, extensionDuration);
        holdDuration = Mathf.Max(0f, holdDuration);
        retractionDuration = Mathf.Max(0f, retractionDuration);
        cooldownDuration = Mathf.Max(0f, cooldownDuration);
        grabRadius = Mathf.Max(0f, grabRadius);
    }

    private void Update()
    {
        if (solver == null)
        {
            return;
        }

        stateTimer = Mathf.Max(0f, stateTimer - Time.deltaTime);
        TickState();
        solver.Solve();
    }

    private void TickState()
    {
        switch (state)
        {
            case ClawState.Idle:
                solver.SetPrimaryTarget(initialGripRestTarget);
                break;
            case ClawState.Extending:
                TickExtending();
                break;
            case ClawState.Holding:
                TickHolding();
                break;
            case ClawState.Retracting:
                TickRetracting();
                break;
            case ClawState.Cooldown:
                TickCooldown();
                break;
        }
    }

    public bool TryStartGrab(GameObject asteroid, AsteroidData data)
    {
        if (!IsAvailable || asteroid == null)
        {
            return false;
        }

        extensionOrigin = initialGripRestTarget;
        extensionTarget = ClampToMaxReach(extensionOrigin, asteroid.transform.position);

        targetAsteroid = asteroid;
        targetAsteroidData = data;
        targetRigidbody = asteroid.GetComponent<Rigidbody>();
        grabbed = false;

        EnterState(ClawState.Extending, extensionDuration);
        return true;
    }

    public Vector3 GetGripPoint()
    {
        Vector3 position;
        if (solver != null && solver.TryGetPrimaryEndEffectorPosition(out position))
        {
            return position;
        }

        return initialGripRestTarget;
    }

    private void TickExtending()
    {
        if (targetAsteroid != null)
        {
            extensionTarget = ClampToMaxReach(extensionOrigin, targetAsteroid.transform.position);
        }

        float t = extensionDuration <= 0f ? 1f : 1f - (stateTimer / extensionDuration);
        solver.SetPrimaryTarget(Vector3.Lerp(extensionOrigin, extensionTarget, Mathf.Clamp01(t)));

        if (targetAsteroid != null)
        {
            Vector3 gripPoint = GetGripPoint();
            float distToAsteroid = Vector3.Distance(gripPoint, targetAsteroid.transform.position);
            if (distToAsteroid <= grabRadius)
            {
                MarkTargetAsGrabbed();
                EnterState(ClawState.Holding, holdDuration);
                return;
            }
        }

        if (stateTimer > 0f)
        {
            return;
        }

        EnterState(ClawState.Retracting, retractionDuration);
    }

    private void TickHolding()
    {
        solver.SetPrimaryTarget(extensionTarget);

        if (grabbed && targetAsteroid != null)
        {
            targetAsteroid.transform.position = GetGripPoint();
        }

        if (stateTimer <= 0f)
        {
            EnterState(ClawState.Retracting, retractionDuration);
        }
    }

    private void TickRetracting()
    {
        float t = retractionDuration <= 0f ? 1f : 1f - (stateTimer / retractionDuration);
        solver.SetPrimaryTarget(Vector3.Lerp(extensionTarget, initialGripRestTarget, Mathf.Clamp01(t)));

        if (grabbed && targetAsteroid != null)
        {
            targetAsteroid.transform.position = GetGripPoint();
        }

        if (stateTimer <= 0f)
        {
            FinishAttempt();
            EnterState(ClawState.Cooldown, cooldownDuration);
        }
    }

    private void TickCooldown()
    {
        if (stateTimer <= 0f)
        {
            EnterState(ClawState.Idle, 0f);
        }
    }

    private void FinishAttempt()
    {
        if (grabbed && targetAsteroid != null)
        {
            if (GameManager.Instance != null)
            {
                int credits = targetAsteroidData != null ? targetAsteroidData.creditValue : DefaultCreditReward;
                GameManager.Instance.AddCredits(credits);
            }

            Destroy(targetAsteroid);
        }

        if (targetRigidbody != null)
        {
            targetRigidbody.isKinematic = false;
        }

        targetAsteroid = null;
        targetAsteroidData = null;
        targetRigidbody = null;
        grabbed = false;
    }

    private void EnterState(ClawState nextState, float duration)
    {
        state = nextState;
        stateTimer = Mathf.Max(0f, duration);
    }

    private void EnsureReferences()
    {
        if (solver == null)
        {
            solver = GetComponent<FABRIK>();
        }
    }

    private void InitializeSolverState()
    {
        if (solver == null)
        {
            return;
        }

        // ClawModule drives solve timing explicitly in Update().
        solver.SetAutoSolve(false);

        if (solver.TryGetPrimaryEndEffectorPosition(out Vector3 initialGrip))
        {
            initialGripRestTarget = initialGrip;
        }
        else
        {
            initialGripRestTarget = transform.position;
        }

        solver.SetPrimaryTarget(initialGripRestTarget);
    }

    private void MarkTargetAsGrabbed()
    {
        grabbed = true;

        if (targetRigidbody != null)
        {
            targetRigidbody.isKinematic = true;
        }
    }

    private Vector3 ClampToMaxReach(Vector3 origin, Vector3 target)
    {
        Vector3 toTarget = target - origin;
        float distance = toTarget.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return origin;
        }

        float clampedDistance = Mathf.Min(distance, maxReachDistance);
        return origin + toTarget.normalized * clampedDistance;
    }
}
