using UnityEngine;

/// <summary>
/// Runtime claw controller that drives a configured FABRIK rig.
/// Wire target selection externally by calling TryStartGrab().
/// </summary>
public class ClawModule : MonoBehaviour
{
    [Header("Claw Configuration")]
    private FABRIK solver;
    private Transform restPoint;
    private float maxReachDistance = 15f;
    private float extensionDuration = 2.5f;
    private float holdDuration = 1.0f;
    private float retractionDuration = 2.5f;
    private float cooldownDuration = 1.0f;
    private float grabRadius = 1.0f;

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
    private bool grabbed;

    public bool IsAvailable => state == ClawState.Idle;
    public bool IsBusy => state == ClawState.Extending || state == ClawState.Holding || state == ClawState.Retracting;
    public bool IsOnCooldown => state == ClawState.Cooldown;

    private void Awake()
    {
        if (solver == null)
        {
            solver = GetComponent<FABRIK>();
        }

        if (restPoint == null)
        {
            restPoint = transform;
        }

        if (solver != null)
        {
            solver.SetPrimaryTarget(restPoint.position);
        }
    }

    private void Update()
    {
        if (solver == null)
        {
            return;
        }

        stateTimer = Mathf.Max(0f, stateTimer - Time.deltaTime);

        switch (state)
        {
            case ClawState.Idle:
                solver.SetPrimaryTarget(restPoint.position);
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

        solver.Solve();
    }

    public bool TryStartGrab(GameObject asteroid, AsteroidData data)
    {
        if (!IsAvailable || asteroid == null)
        {
            return false;
        }

        extensionOrigin = restPoint.position;
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

        return restPoint != null ? restPoint.position : transform.position;
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
                grabbed = true;

                if (targetRigidbody != null)
                {
                    targetRigidbody.isKinematic = true;
                }

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
        solver.SetPrimaryTarget(Vector3.Lerp(extensionTarget, restPoint.position, Mathf.Clamp01(t)));

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
            // Use loot table instead of creditValue
            if (targetAsteroidData != null)
                targetAsteroidData.DropLoot(targetAsteroid.transform.position);
            else
                GameManager.Instance.AddCredits(10);
        }

        Destroy(targetAsteroid);
    }

    if (targetRigidbody != null)
        targetRigidbody.isKinematic = false;

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
