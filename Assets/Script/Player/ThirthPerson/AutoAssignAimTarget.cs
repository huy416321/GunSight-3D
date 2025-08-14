using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AutoAssignAimTarget : MonoBehaviour
{
    [Header("Target to assign (không parent vào player)")]
    public Transform aimTarget;
    [Header("Multi-Aim Constraints cần gán target")]
    public MultiAimConstraint[] aimConstraints;

    void Awake()
    {
        if (aimTarget == null)
        {
            GameObject go = GameObject.Find("AimTarget");
            if (go == null)
            {
                go = new GameObject("AimTarget");
            }
            aimTarget = go.transform;
        }
        if (aimTarget == null || aimConstraints == null) return;
        foreach (var constraint in aimConstraints)
        {
            if (constraint != null)
            {
                var sources = constraint.data.sourceObjects;
                sources.Clear();
                sources.Add(new WeightedTransform(aimTarget, 1f));
                constraint.data.sourceObjects = sources;
            }
        }
    }
}
