using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

public class Planes : MonoBehaviour {

    private static uint MAX_RESULTS = 100;
    public Transform BBoxTransform;
    public Vector3 BBoxExtents;
    public GameObject prefab;

    void Awake()
    {
        MLResult result = MLWorldPlanes.Start();
        if (!result.IsOk)
        {
            Debug.LogError("Error Planes starting MLWorldPlanes, disabling script.");
            enabled = false;
            return;
        }
        var queryParams = new MLWorldPlanesQueryParams();
        var queryFlags = MLWorldPlanesQueryFlags.Horizontal;
        queryParams.Flags = queryFlags;
        queryParams.MaxResults = MAX_RESULTS;
        queryParams.BoundsCenter = BBoxTransform.position;
        queryParams.BoundsRotation = BBoxTransform.rotation;
        queryParams.BoundsExtents = BBoxExtents;
        MLWorldPlanes.GetPlanes(queryParams, HandleOnReceivedPlanes);
    }

    private void HandleOnReceivedPlanes(MLResult result, MLWorldPlane[] planes)
    {
        if (!result.IsOk)
        {
            Debug.LogError("Error GetPlanes");
            return;
        }
        for (int i = 0; i < planes.Length; ++i)
        {
            var plane = planes[i];
            var newPlane = Instantiate(prefab);
            newPlane.transform.position = plane.Center;
            newPlane.transform.rotation = plane.Rotation;
            newPlane.transform.localScale = new Vector3(plane.Width, plane.Height, 1.0f);
        }
    }

    private void OnDestroy()
    {
        MLWorldPlanes.Stop();
    }
}
