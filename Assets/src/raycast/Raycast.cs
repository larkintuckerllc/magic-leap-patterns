using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

public class Raycast : MonoBehaviour {

    static Vector3 POSITION = new Vector3(0.0f, 0.5f, 0.0f);
    static Vector3 DIRECTION = new Vector3(-1.0f, 0.0f, 0.0f);

    public GameObject prefab;

    void Awake()
    {
        MLResult result = MLWorldRays.Start();
        if (!result.IsOk)
        {
            Debug.LogError("Error BaseRaycast starting MLWorldRays, disabling script.");
            enabled = false;
            return;
        }
        var raycastParams = new MLWorldRays.QueryParams();
        raycastParams.Position = POSITION;
        raycastParams.Direction = DIRECTION;
        MLWorldRays.GetWorldRays(raycastParams, HandleOnReceiveRaycast);
    }
	
	private void OnDestroy()
	{
        MLWorldRays.Stop();
	}

    private void HandleOnReceiveRaycast(MLWorldRays.MLWorldRaycastResultState state, Vector3 point, Vector3 normal, float confidence) {
        if (state == MLWorldRays.MLWorldRaycastResultState.RequestFailed || state == MLWorldRays.MLWorldRaycastResultState.NoCollision)
        {
            Debug.Log("No collision");
            return;
        }
        RaycastHit result = new RaycastHit();
        result.point = point;
        result.normal = normal;
        result.distance = Vector3.Distance(POSITION, point);
        var newPlane = Instantiate(prefab);
        newPlane.transform.position = result.point;
    }
}
