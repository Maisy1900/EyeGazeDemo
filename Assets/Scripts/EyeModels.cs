using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class EyeModels : MonoBehaviour
{
    [field: SerializeField]
    public bool IsHovered { get; private set; }

    [field: SerializeField]
    public bool IsSelected { get; private set; }

    [SerializeField]
    private UnityEvent<GameObject> OnObjectHover;

    [SerializeField] 
    private UnityEvent<GameObject> OnObjectSelected;

    [SerializeField]
    private Material OnHoverActiveMaterial;

    [SerializeField]
    private Material OnSelectActiveMaterial;

    [SerializeField]
    private Material OnIdleMaterial;

    private MeshRenderer meshRenderer;

    private Transform originalAnchor;

    private TextMeshPro statusText;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        statusText = GetComponentInChildren<TextMeshPro>();
        originalAnchor = transform.parent;
    } 
    
    public void Hover(bool state)
    {
        IsHovered = state;
    }

    public void Select(bool state, Transform anchor = null)
    {
        IsSelected = state;
        if (anchor) transform.SetParent(anchor);
        if (!IsSelected) transform.SetParent(originalAnchor);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsHovered)
        {
            OnObjectHover?.Invoke(gameObject);
            meshRenderer.material = OnHoverActiveMaterial;
            statusText.text = $"<color=\"yellow\">Hovered</color>";
        }
        if (IsSelected)
        {
            OnObjectHover?.Invoke(gameObject);
            meshRenderer.material = OnSelectActiveMaterial;
            statusText.text = $"<color=\"green\">Selected</color>";
        }
        if (!IsHovered && !IsSelected)
        {
            meshRenderer.material = OnIdleMaterial;
            statusText.text = $"<color=\"white\">Idle</color>";
        }
    }
    
}
