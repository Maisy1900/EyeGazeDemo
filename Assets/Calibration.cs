using UnityEngine;

public class Calibration : MonoBehaviour
{
    [Header("Dependencies")]
    public TextEntryVR textEntry;
    public Transform keyboard;
    public Transform hands;
    public RectTransform textDisplay;
    public OVRHand ovrHand;
    public OVRSkeleton ovrSkeleton;

    [Header("Keyboard Calibration Points (Virtual Model)")]
    public Transform Qcalibration;
    public Transform Pcalibration;
    public Transform Zcalibration;

    private Vector3 qMeasured, pMeasured, zMeasured;
    private bool gotQ = false, gotP = false, gotZ = false;
    private bool calibrated = false;
    void Start()
    {
        if (textEntry != null)
            textEntry.OnCharReceived += HandleKey;
    }

    void HandleKey(string character)
    {
        character = character.ToLower();

        if (character == "q" && !gotQ)
        {
            qMeasured = GetIndexTip();
            gotQ = true;
            Debug.Log("Q measured at " + qMeasured);
            Debug.Log("Q calibrated at " + Qcalibration.position);
        }
        else if (character == "p" && !gotP)
        {
            pMeasured = GetIndexTip();
            gotP = true;
            Debug.Log("P measured at " + pMeasured);
            Debug.Log("P calibrated at " + Pcalibration.position);
        }
        else if (character == "z" && !gotZ)
        {
            zMeasured = GetIndexTip();
            gotZ = true;
            Debug.Log("Z measured at " + zMeasured);
            Debug.Log("Z calibrated at " + Zcalibration.position);
        }

        if (gotQ && gotP && gotZ)
        {
            if(calibrated == false)
            {
                ApplyFullCalibration();
                calibrated = true;
            }
            //gotQ = false; gotP = false; gotZ = false;
            enabled = false;
        }
    }

    Vector3 GetIndexTip()
    {
        if (ovrSkeleton == null || !ovrSkeleton.IsDataValid)
        {
            Debug.LogWarning("OVRSkeleton not valid yet.");
            return Vector3.zero;
        }

        foreach (var bone in ovrSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                Vector3 worldPos = bone.Transform.position;
                return worldPos; // Convert to pivot local
            }
        }

        Debug.LogWarning("IndexTip bone not found.");
        return Vector3.zero;
    }

    void ApplyFullCalibration()
    {
        // 1. Get virtual keyboard calibration points (world space)
        Vector3 m0 = Qcalibration.position;
        Vector3 m1 = Pcalibration.position;
        Vector3 m2 = Zcalibration.position;

        // 2. Get real-world measured points (also in world space)
        Vector3 r0 = qMeasured;
        Vector3 r1 = pMeasured;
        Vector3 r2 = zMeasured;

        // 3. Compute centroids
        Vector3 modelCenter = (m0 + m1 + m2) / 3f;
        Vector3 realCenter = (r0 + r1 + r2) / 3f;

        // 4. Compute translation delta in world space
        Vector3 delta = realCenter - modelCenter;
        Debug.Log("delta " + delta);
        // 5. Apply delta in world space
        if (keyboard != null)
            keyboard.position += delta;
        //if (hands != null)
        //hands.position += delta;
        //if (textDisplay != null)
        //textDisplay.position += delta;

        Debug.Log($"✅ Calibrated (world-space translation only). Δ = {delta:F4}");
    }





    void ApplyTranslationOnly(Transform target, Vector3 offset)
    {
        if (target != null)
            target.localPosition += offset;
    }


}
