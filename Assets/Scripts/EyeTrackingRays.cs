using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

// Creates a visual ray for eye tracking that detects objects and allows pinch selection
[RequireComponent(typeof(LineRenderer))]
public class EyeTrackingRays : MonoBehaviour
{
    [SerializeField]
    private float rayDistance = 1f;

    [SerializeField]
    private float rayWidth = 0.01f;

    [SerializeField]
    private LayerMask layersToInclude;

    [SerializeField]
    private Color rayColourDefaultState = Color.yellow;

    [SerializeField]
    private Color rayColourHoverState = Color.red;

    [SerializeField]
    private Color rayColourSelectedState = Color.green;

    [SerializeField]
    private OVRHand handUsedForPinchSelection;

    [SerializeField]
    private bool mockHandUsedForPinchSelection;

    private bool intercepting;
    private bool allowPinchSelection;
    private LineRenderer lineRenderer;
    
    // Cache interactable objects to avoid repeated GetComponent calls
    private Dictionary<int, EyeModels> models = new Dictionary<int, EyeModels>();
    
    // Stores the last object we looked at for selection
    private EyeModels lastEyeInteractable;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        allowPinchSelection = handUsedForPinchSelection != null;
        SetupRay();
    }

    // Configure the LineRenderer's visual properties
    private void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayColourDefaultState;
        lineRenderer.endColor = rayColourDefaultState;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y,transform.position.z + rayDistance));
    }

    private void Update()
    {
        // Hide the ray when pinching
        lineRenderer.enabled = !IsPinching();

        SelectionStarted();

        // Reset ray to default state if not looking at anything
        if (!intercepting)
        {
            lineRenderer.startColor = lineRenderer.endColor = rayColourDefaultState;
            lineRenderer.SetPosition(1, new Vector3 (0,0, transform.position.z + rayDistance));
            OnHoverEnded();
        }
    }

    private void FixedUpdate()
    {
        if (IsPinching()) return;

        Vector3 rayDirction = transform.TransformDirection(Vector3.forward) * rayDistance;

        // Raycast to detect what the user is looking at
        intercepting = Physics.Raycast(transform.position, rayDirction, out RaycastHit hit,Mathf.Infinity, layersToInclude);

        if (intercepting)
        {
            OnHoverEnded();
            lineRenderer.startColor = lineRenderer.endColor = rayColourHoverState;

            // Cache the EyeModels component for performance
            if (!models.TryGetValue(hit.transform.gameObject.GetHashCode(), out EyeModels eyeInteractable))
            {
                eyeInteractable = hit.transform.GetComponent<EyeModels>();
                models.Add(hit.transform.gameObject.GetHashCode(), eyeInteractable);
            }

            var toLocalSpace = transform.InverseTransformPoint(eyeInteractable.transform.position);
            lineRenderer.SetPosition(1, new Vector3(0, 0, toLocalSpace.z));

            eyeInteractable.Hover(true);
            lastEyeInteractable = eyeInteractable;
        }

    }
    
    // Handle selection via pinch gesture
    private void SelectionStarted()
    {
        if (IsPinching())
        {
            lastEyeInteractable?.Select(true, (handUsedForPinchSelection?.IsTracked ?? false) ? handUsedForPinchSelection.transform : transform);
        }
        else
        {
            lastEyeInteractable?.Select(false);
        }
    }

    private void OnHoverEnded()
    {
        foreach (var model in models) model.Value.Hover(false);
    }

    private void OnDestory() => models.Clear();

    private bool IsPinching() => (allowPinchSelection && handUsedForPinchSelection.GetFingerIsPinching(OVRHand.HandFinger.Index))|| mockHandUsedForPinchSelection;
 
}
