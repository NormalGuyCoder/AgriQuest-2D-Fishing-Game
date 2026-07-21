using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Agriquest.Minigames
{
    /// <summary>
    /// Keeps track of the player's last transform for every scene and reposition them
    /// to that transform when the scene is reloaded.
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static PlayerSpawnManager Instance { get; private set; }

        [Header("Player Tracking")]
        [Tooltip("Tag that identifies the player instance that should be repositioned.")]
        [SerializeField]
        private string playerTag = "Player";

        [Tooltip("Automatically move the player back to the stored transform when a scene loads.")]
        [SerializeField]
        private bool moveOnLoad = true;

        [Tooltip("Distance the player must move away from a teleport location before another teleport is allowed in that scene.")]
        [SerializeField]
        private float teleportDepartureDistance = 1f;

        [Tooltip("Time in seconds the player must stay at teleport location before teleport is considered complete and disabled.")]
        [SerializeField]
        private float teleportCooldownTime = 0.5f;

        [Tooltip("Minimum distance difference required to trigger a teleport. Prevents teleporting if player is already close to target.")]
        [SerializeField]
        private float minimumTeleportDistance = 0.1f;

        [Tooltip("Delay in seconds after scene load before attempting teleport. Prevents conflicts with other teleport systems.")]
        [SerializeField]
        private float teleportDelayAfterLoad = 0.1f;

        private readonly Dictionary<string, TransformData> sceneSpawns = new();
        private Transform playerTransform;
        private string pendingSceneToApply;
        private bool hasAppliedPendingSpawn;
        private string lastTeleportedScene;
        private Vector3 lastTeleportedPosition;
        private bool hasLeftTeleportLocation = true;
        private float teleportTime;
        private bool teleportCooldownActive;
        private float sceneLoadTime;
        private bool hasAttemptedTeleportThisLoad;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                CachePlayerTransform();
            }

            if (playerTransform == null)
            {
                return;
            }

            var sceneName = SceneManager.GetActiveScene().name;
            UpdateTeleportCooldown(sceneName);
            UpdateTeleportDeparture(sceneName);
            
            // Only try to teleport once per scene load, and only after delay
            if (!hasAttemptedTeleportThisLoad && Time.time >= sceneLoadTime + teleportDelayAfterLoad)
            {
                TryApplyPendingSpawn(sceneName);
            }
            
            sceneSpawns[sceneName] = new TransformData(playerTransform.position, playerTransform.rotation);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If entering a different scene, mark that we've left the previous teleport location
            if (scene.name != lastTeleportedScene)
            {
                hasLeftTeleportLocation = true;
            }

            pendingSceneToApply = moveOnLoad ? scene.name : null;
            hasAppliedPendingSpawn = false;
            teleportCooldownActive = false;
            teleportTime = 0f;
            sceneLoadTime = Time.time;
            hasAttemptedTeleportThisLoad = false;

            CachePlayerTransform();
        }

        private void CachePlayerTransform()
        {
            if (playerTransform != null)
            {
                return;
            }

            var playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        private void TryApplyPendingSpawn(string sceneName)
        {
            // Mark that we've attempted teleport for this load
            hasAttemptedTeleportThisLoad = true;

            if (playerTransform == null ||
                !moveOnLoad ||
                string.IsNullOrEmpty(pendingSceneToApply) ||
                pendingSceneToApply != sceneName ||
                teleportCooldownActive)
            {
                return;
            }

            // Only allow teleport if:
            // 1. It's a different scene, OR
            // 2. It's the same scene but player has left the teleport location
            if (sceneName == lastTeleportedScene && !hasLeftTeleportLocation)
            {
                return;
            }

            if (sceneSpawns.TryGetValue(sceneName, out var transformData))
            {
                // Check if player is already at the target position (within tolerance)
                float distanceToTarget = Vector3.Distance(playerTransform.position, transformData.Position);
                
                // Only teleport if player is far enough from target position
                if (distanceToTarget < minimumTeleportDistance)
                {
                    // Player is already at target, just mark as applied without teleporting
                    hasAppliedPendingSpawn = true;
                    pendingSceneToApply = null;
                    lastTeleportedScene = sceneName;
                    lastTeleportedPosition = transformData.Position;
                    hasLeftTeleportLocation = false;
                    return;
                }

                // Teleport player to saved position
                playerTransform.SetPositionAndRotation(transformData.Position, transformData.Rotation);
                hasAppliedPendingSpawn = true;
                pendingSceneToApply = null;
                lastTeleportedScene = sceneName;
                lastTeleportedPosition = transformData.Position;
                hasLeftTeleportLocation = false;
                teleportTime = Time.time;
            }
        }

        private void UpdateTeleportCooldown(string sceneName)
        {
            if (!hasAppliedPendingSpawn ||
                string.IsNullOrEmpty(lastTeleportedScene) ||
                lastTeleportedScene != sceneName ||
                playerTransform == null ||
                teleportCooldownActive ||
                teleportTime <= 0f)
            {
                return;
            }

            float distanceFromTeleport = Vector3.Distance(playerTransform.position, lastTeleportedPosition);
            float timeSinceTeleport = Time.time - teleportTime;

            // If player is still at teleport location after cooldown time, activate cooldown
            if (distanceFromTeleport <= teleportDepartureDistance && timeSinceTeleport >= teleportCooldownTime)
            {
                teleportCooldownActive = true;
            }
        }

        private void UpdateTeleportDeparture(string sceneName)
        {
            if (!hasAppliedPendingSpawn ||
                string.IsNullOrEmpty(lastTeleportedScene) ||
                lastTeleportedScene != sceneName ||
                playerTransform == null ||
                hasLeftTeleportLocation)
            {
                return;
            }

            if (Vector3.Distance(playerTransform.position, lastTeleportedPosition) > teleportDepartureDistance)
            {
                hasLeftTeleportLocation = true;
                // Clear cooldown when player leaves, so they can teleport again when returning
                teleportCooldownActive = false;
            }
        }

        private readonly struct TransformData
        {
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }

            public TransformData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }
    }
}

