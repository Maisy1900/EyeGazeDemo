using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework; // Optional, for clarity

public class FingertipVisualizer : MonoBehaviour
{
    public OVRSkeleton ovrSkeleton;
    public GameObject fingertipPrefab;  // Your transparent sphere prefab

    private Dictionary<OVRSkeleton.BoneId, GameObject> fingertipMarkers;

    void Start()
    {
        fingertipMarkers = new Dictionary<OVRSkeleton.BoneId, GameObject>();

        if (ovrSkeleton == null || fingertipPrefab == null)
        {
            Debug.LogError("FingertipVisualizer is missing references.");
            enabled = false;
            return;
        }

        if (!ovrSkeleton.IsInitialized)
        {
            StartCoroutine(WaitForSkeletonReady());
        }
        else
        {
            CreateMarkers();
        }
    }

    System.Collections.IEnumerator WaitForSkeletonReady()
    {
        while (!ovrSkeleton.IsInitialized)
            yield return null;

        CreateMarkers();
    }

    void CreateMarkers()
    {
        foreach (var bone in ovrSkeleton.Bones)
        {
            if (IsFingertip(bone.Id))
            {
                GameObject marker = Instantiate(fingertipPrefab, bone.Transform.position, Quaternion.identity);
                marker.transform.SetParent(this.transform); // Keep organized
                fingertipMarkers[bone.Id] = marker;
            }
        }
    }

    void Update()
    {
        if (ovrSkeleton == null || fingertipMarkers == null)
            return;

        foreach (var bone in ovrSkeleton.Bones)
        {
            if (fingertipMarkers.ContainsKey(bone.Id))
            {
                fingertipMarkers[bone.Id].transform.position = bone.Transform.position;
                fingertipMarkers[bone.Id].transform.rotation = bone.Transform.rotation;
            }
        }
    }

    bool IsFingertip(OVRSkeleton.BoneId id)
    {
        return id == OVRSkeleton.BoneId.Hand_ThumbTip ||
               id == OVRSkeleton.BoneId.Hand_IndexTip ||
               id == OVRSkeleton.BoneId.Hand_MiddleTip ||
               id == OVRSkeleton.BoneId.Hand_RingTip ||
               id == OVRSkeleton.BoneId.Hand_PinkyTip;
    }
}
