using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements; // Assuming this is for UI, though not directly used in the logic provided.
using UnityEngine.EventSystems;
using static AllyPoolManager; // Assuming AllyPoolManager is a static class or has a static instance.

public enum PlacementMode
{
    None,
    DrawPath,
    PlaceSpecialUnit,
    PlaceNuke
}

public class PlayerController : MonoBehaviour
{
    public GameObject SpecialUnit;
    public GameObject Nuke;

    [SerializeField]
    private float heightToSpawnNuke = 4f;
    [SerializeField]
    private float spawnOffsetY = 0.15f;
    [SerializeField]
    private float placementSpacing = 0.5f;

    private bool isHolding = false;
    private List<Vector3> drawnPathPoints = new List<Vector3>();
    private PlacementMode currentPlacementMode = PlacementMode.None;
    private Vector3 lastSpawnPosition; // Tracks the last position an object was spawned

    void Update()
    {
        Vector3 currentInputPosition = Vector3.zero;
        bool inputDown = false;
        bool inputHeld = false;
        bool inputUp = false;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // --- Mobile Input ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                inputDown = true;
            }
            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                inputHeld = true;
            }
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                inputUp = true;
            }
            currentInputPosition = touch.position;
        }

        // --- Editor (Mouse) Input ---
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            inputDown = true;
        }
        if (Input.GetMouseButton(0))
        {
            inputHeld = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            inputUp = true;
        }
        // Only assign mouse position in Editor to avoid issues when building.
        if (Application.isEditor)
        {
            currentInputPosition = Input.mousePosition;
        }
#endif

        // --- Common Logic ---
        if (inputDown)
        {
            if (currentPlacementMode == PlacementMode.PlaceSpecialUnit)
            {
                TryPlaceSpecialUnitOnClick(currentInputPosition);
                return;
            }
            else if (currentPlacementMode == PlacementMode.PlaceNuke)
            {
                TryPlaceNukeOnClick(currentInputPosition);
                return;
            }

            // Ch? khi ?ang ? ch? ?? None thì m?i cho phép v? ???ng
            if (currentPlacementMode == PlacementMode.None)
            {
                ResetDrawPathState();
                isHolding = true;
                currentPlacementMode = PlacementMode.DrawPath;
                ProcessInputForDrawing(currentInputPosition, true);
            }
        }

        if (inputHeld && isHolding)
        {
            if (currentPlacementMode == PlacementMode.DrawPath)
            {
                ProcessInputForDrawing(currentInputPosition, false); // Continue drawing and spawning
            }
        }

        if (inputUp)
        {
            if (currentPlacementMode == PlacementMode.DrawPath)
            {
                Debug.Log("Finished drawing path.");
            }
            ResetDrawPathState(); // Reset state after input is released
            // Only reset mode to None if it was a path drawing action.
            // SpecialUnit/Nuke mode is reset inside their respective placement functions.
            if (currentPlacementMode != PlacementMode.PlaceSpecialUnit && currentPlacementMode != PlacementMode.PlaceNuke)
            {
                currentPlacementMode = PlacementMode.None;
            }
        }
    }

    // --- UI Button Clicked Handlers ---
    public void ButtonClickedPlaceSpecialUnit()
    {
        currentPlacementMode = PlacementMode.PlaceSpecialUnit;
        Debug.Log("Special Unit placement mode activated. Tap the screen to place.");
        ResetDrawPathState(); // Clear any pending draw state
    }

    public void ButtonClickedSpawnNuke()
    {
        currentPlacementMode = PlacementMode.PlaceNuke;
        Debug.Log("Nuke placement mode activated. Tap the screen to place.");
        ResetDrawPathState(); // Clear any pending draw state
    }

    // --- Specific Placement Logic for Special Unit (on NavMesh) ---
    void TryPlaceSpecialUnitOnClick(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Road"))
            {
                Vector3 proposedPoint = hit.point + Vector3.up * spawnOffsetY;

                NavMeshHit navHit;
                float maxDistance = 1.0f; // Max distance to search for NavMesh

                if (NavMesh.SamplePosition(proposedPoint, out navHit, maxDistance, NavMesh.AllAreas))
                {
                    SpawnSpecialUnit(navHit.position);
                    currentPlacementMode = PlacementMode.None; // Reset to None after placement
                }
                else
                {
                    Debug.LogWarning("Special Unit Placement: Not on a valid NavMesh.");
                }
            }
            else
            {
                Debug.Log("Special Unit: Not touching 'Road' for placement.");
            }
        }
    }

    // --- Specific Placement Logic for Nuke (above ground) ---
    void TryPlaceNukeOnClick(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Road")) // Can place Nuke on "Road" tag as well
            {
                Vector3 spawnPoint = hit.point + Vector3.up * heightToSpawnNuke;
                SpawnNuke(spawnPoint);
                currentPlacementMode = PlacementMode.None; // Reset to None after placement
            }
            else
            {
                Debug.Log("Nuke: Not touching terrain for placement.");
            }
        }
    }

    // --- Object Spawning Methods ---
    void SpawnSpecialUnit(Vector3 position)
    {
        Instantiate(SpecialUnit, position, Quaternion.identity);
    }

    void SpawnNuke(Vector3 position)
    {
        GameObject nukeInstance = Instantiate(Nuke, position, Quaternion.identity);
        if (nukeInstance != null)
        {
            NukeLogic nukeLogic = nukeInstance.GetComponent<NukeLogic>();
            AudioManager.Instance.Play("Nuke");
            if (nukeLogic != null)
            {
                nukeLogic.pc = this;
            }
        }
    }

    // --- Path Drawing and Continuous Spawning Logic ---
    void ProcessInputForDrawing(Vector3 screenPosition, bool isFirstPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 proposedPoint = hit.point + Vector3.up * spawnOffsetY;
            NavMeshHit navHit;
            float maxDistance = 1.0f;
            if (hit.collider.gameObject.CompareTag("DefenseLine")|| hit.collider.gameObject.CompareTag("Ally"))
            {
                
                return;
            }
            // Luôn th? l?y ?i?m h?p l? g?n nh?t trên NavMesh
            if (!NavMesh.SamplePosition(proposedPoint, out navHit, maxDistance, NavMesh.AllAreas))
            {
                // Không có NavMesh g?n ?ó ? b? qua l?n này nh?ng không reset state
                return;
            }

            // N?u lastSpawnPosition v?n là zero (ch?a spawn ???c l?n nào) thì spawn luôn t?i ?ây
            if (lastSpawnPosition == Vector3.zero)
            {
                drawnPathPoints.Add(navHit.position);
                SpawnObjectWithRotation(navHit.position, Quaternion.identity);
                lastSpawnPosition = navHit.position;
                Debug.Log("Started drawing path and spawned first unit (after skipping invalid start).");
                return;
            }

            // ?ã có lastSpawnPosition ? x? lý ti?p các ?i?m theo kho?ng cách
            Vector3 currentHitPointFlat = new Vector3(navHit.position.x, lastSpawnPosition.y, navHit.position.z);
            float distanceToCurrentHit = Vector3.Distance(lastSpawnPosition, currentHitPointFlat);

            if (distanceToCurrentHit >= placementSpacing)
            {
                int numNewSpawns = Mathf.FloorToInt(distanceToCurrentHit / placementSpacing);
                Vector3 direction = (currentHitPointFlat - lastSpawnPosition).normalized;

                for (int i = 0; i < numNewSpawns; i++)
                {
                    Vector3 spawnPoint = lastSpawnPosition + direction * placementSpacing;
                    NavMeshHit spawnNavHit;

                    if (NavMesh.SamplePosition(spawnPoint, out spawnNavHit, maxDistance, NavMesh.AllAreas))
                    {
                        SpawnObjectWithRotation(spawnNavHit.position, Quaternion.LookRotation(direction));
                        drawnPathPoints.Add(spawnNavHit.position);
                        lastSpawnPosition = spawnNavHit.position;
                    }
                    else
                    {
                        Debug.LogWarning("Spawn point not on NavMesh. Skipping spawn.");
                        break;
                    }
                }
            }
        }
    }



    // --- Object Spawning and Pool Management ---
    void SpawnObjectWithRotation(Vector3 position, Quaternion rotation)
    {
        if (AllyPoolManager.Instance == null)
        {
            Debug.LogWarning("AllyPoolManager is not initialized. Cannot spawn allies from pool.");
            return;
        }

        GameObject ally = AllyPoolManager.Instance.GetFromPool(position, rotation);
        if (ally == null) return;

        // Activate NavMeshAgent or NavMeshObstacle based on ally type
        if (AllyPoolManager.Instance.GetSelectedAllyType() != AllyType.Barrier)
        {
            NavMeshAgent agent = ally.GetComponentInChildren<NavMeshAgent>();
            if (agent != null) agent.enabled = true;
        }
        else if (AllyPoolManager.Instance.GetSelectedAllyType() == AllyType.Barrier)
        {
            NavMeshObstacle obstacle = ally.GetComponentInChildren<NavMeshObstacle>();
            if (obstacle != null) obstacle.enabled = true;
        }
    }

    // --- Reset State Helper ---
    void ResetDrawPathState()
    {
        isHolding = false;
        drawnPathPoints.Clear();
        lastSpawnPosition = Vector3.zero; // Reset last spawned position
    }
}