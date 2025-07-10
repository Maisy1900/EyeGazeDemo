using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyHandPoseSync : MonoBehaviour
{
    public OVRSkeleton sourceSkeleton;
    private OVRSkeleton targetSkeleton;

    private List<OVRBone> sourceBones;
    private List<OVRBone> targetBones;
    void Start()
    {
        StartCoroutine(InitWhenReady());
    }

    IEnumerator InitWhenReady()
    {
        targetSkeleton = GetComponent<OVRSkeleton>();

        // Wait for both skeletons to be ready
        while (!sourceSkeleton.IsDataValid || sourceSkeleton.Bones.Count == 0)
            yield return null;
        while (!targetSkeleton.IsDataValid || targetSkeleton.Bones.Count == 0)
            yield return null;

        sourceBones = new List<OVRBone>(sourceSkeleton.Bones);
        targetBones = new List<OVRBone>(targetSkeleton.Bones);

        yield return null;
    }

    void LateUpdate()
    {
        if (sourceBones == null || targetBones == null) return;

        for (int i = 0; i < sourceBones.Count; i++)
        {
            targetBones[i].Transform.localPosition = sourceBones[i].Transform.localPosition;
            targetBones[i].Transform.localRotation = sourceBones[i].Transform.localRotation;
        }
    }
}
