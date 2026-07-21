using UnityEngine;
using UnityEngine.Events;

public class HookLauncher : MonoBehaviour
{
    public enum HookLauncherState
    {
        Inactive = 0,
        Idle = 1,
        Firing = 2,
        Retracting = 3
    }

    [Header("Hook Settings")]
    public Transform firingPoint;
    public float maxDepthTime = 5f; // Maximum time the hook can descend
    public float hookSpeed = 5f; // Speed of the hook
    public float retractionSpeed = 7f; // Speed of retraction

    [Header("Camera Settings")]
    public CameraFollow cameraFollow;

    private GameObject currentHook;
    private HookMovement currentHookMovement;
    private HookAttacking currentHookAttacking;
    private HookTrailManager currentHookTrail;
    public float currentDepth;

    public UnityEvent enterIdleStateEvent;
    public UnityEvent enterFiringStateEvent;
    public UnityEvent enterRetractStateEvent;

    [field: SerializeField]
    public HookLauncherState launcherState { get; private set; }

    private void Start()
    {
        setLauncherState(HookLauncherState.Inactive);
        
        // Find camera follow if not assigned
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                Debug.LogWarning("CameraFollow component not found on main camera!");
            }
        }
    }

    private void Update()
    {
        if (launcherState == HookLauncherState.Idle)
        {
            IdleState();
        }
        if (launcherState == HookLauncherState.Firing)
        {
            FiringState();
        }
        if (launcherState == HookLauncherState.Retracting)
        {
            RetractingState();
        }
    }

    public void setLauncherState(HookLauncherState newState)
    {
        HookLauncherState num = launcherState;
        launcherState = newState;
        if (num == HookLauncherState.Idle)
        {
            exitIdleState();
        }
        if (num == HookLauncherState.Firing)
        {
            exitFiringState();
        }
        if (num == HookLauncherState.Retracting)
        {
            exitRetractingState();
        }
        if (launcherState == HookLauncherState.Idle)
        {
            enterIdleState();
        }
        if (launcherState == HookLauncherState.Firing)
        {
            enterFiringState();
        }
        if (launcherState == HookLauncherState.Retracting)
        {
            enterRetractingState();
        }
    }

    public void enterIdleState()
    {
        enterIdleStateEvent.Invoke();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform); // Follow player when idle
        }
    }

    public void IdleState()
    {
    }

    public void exitIdleState()
    {
    }

    public void enterFiringState()
    {
        // Spawn hook at firing point
        currentHook = Instantiate(Resources.Load<GameObject>("Hook"), firingPoint.position, firingPoint.rotation);
        setCurrentHookProperties(currentHook);
        currentDepth = 0f;
        enterFiringStateEvent.Invoke();
        Debug.Log("Hook spawned");
        currentHookAttacking.isRetracting = false;

        // Set camera to follow hook
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(currentHook.transform);
        }
    }

    public void FiringState()
    {
        currentDepth += Time.deltaTime;
        if (currentDepth >= maxDepthTime)
        {
            setLauncherState(HookLauncherState.Retracting);
        }
    }

    public void exitFiringState()
    {
    }

    public void enterRetractingState()
    {
        currentHookMovement.RetractHook(firingPoint);
        currentHookTrail.enableRetractionLine(firingPoint);
        enterRetractStateEvent.Invoke();
        currentHookAttacking.isRetracting = true;
    }

    public void RetractingState()
    {
        if (Vector2.Distance(currentHook.transform.position, firingPoint.position) <= 0.3)
        {
            setLauncherState(HookLauncherState.Idle);
        }
    }

    public void exitRetractingState()
    {
        resetCurrentHookProperties();
    }

    private void setCurrentHookProperties(GameObject hook)
    {
        currentHookMovement = hook.GetComponent<HookMovement>();
        currentHookAttacking = hook.GetComponent<HookAttacking>();
        currentHookTrail = hook.GetComponent<HookTrailManager>();
        if (currentHookMovement == null)
        {
            Debug.LogError(name + " could not find a HookMovement component in the current hook");
            return;
        }
        if (currentHookAttacking == null)
        {
            Debug.LogError(name + " could not find a HookAttacking component in the current hook");
            return;
        }
        if (currentHookTrail == null)
        {
            Debug.LogError(name + " could not find a HookTrail component in the current hook");
            return;
        }
        currentHookMovement.moveSpeed = hookSpeed;
        currentHookMovement.retractionSpeed = retractionSpeed;
        currentHookAttacking.damage = 1; // Default damage
    }

    private void resetCurrentHookProperties()
    {
        Destroy(currentHook);
        currentHookMovement = null;
    }
} 