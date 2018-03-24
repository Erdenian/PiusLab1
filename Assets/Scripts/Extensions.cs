using UnityEngine;

public static class Extensions
{
    public static Bounds TransformBounds(this Transform trans, Bounds localBounds)
    {
        var center = trans.TransformPoint(localBounds.center);

        // transform the local extents' axes
        var extents = localBounds.extents;
        var axisX = trans.TransformVector(extents.x, 0, 0);
        var axisY = trans.TransformVector(0, extents.y, 0);
        var axisZ = trans.TransformVector(0, 0, extents.z);

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    public static Bounds InverseTransformBounds(this Transform trans, Bounds bounds)
    {
        var center = trans.InverseTransformPoint(bounds.center);

        // transform the local extents' axes
        var extents = bounds.extents;
        var axisX = trans.InverseTransformVector(extents.x, 0, 0);
        var axisY = trans.InverseTransformVector(0, extents.y, 0);
        var axisZ = trans.InverseTransformVector(0, 0, extents.z);

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }
}
