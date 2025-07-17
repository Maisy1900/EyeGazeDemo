using UnityEngine;

public class TransformApplier : MonoBehaviour
{
    public Transform headTransform;       // Il "Center Eye" del Quest (tipicamente XR Rig Camera)
    public Vector3 localOffset = new Vector3(0f, -1f, 0.0f);  // Offset in avanti e in basso
    void Update()
    {
        if (headTransform == null)
        {
            Debug.LogWarning("headTransform (center eye) non assegnato.");
            return;
        }

        // Posizione: calcola posizione con offset locale rispetto alla testa
        transform.position = headTransform.TransformPoint(localOffset);

        // Rotazione: identica alla testa (puoi aggiungere rotazioni se vuoi modificare l'orientamento)
        transform.rotation = headTransform.rotation;

    }
}
