using UnityEngine;
using System.Collections.Generic;

public class CameraProjectionPoints : MonoBehaviour
{
    public Camera cam;
    public int resolution = 10; // S? ?i?m chia l??i (10x10)
    public float rayDistance = 100f;
    public string targetTag = "Road"; // Ch? nh?n khi trúng tag "Road"

    private List<Vector3> hitPoints = new List<Vector3>();

    void Update()
    {
        UpdateProjectionPoints();
    }

    void UpdateProjectionPoints()
    {
        hitPoints.Clear();

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float vx = (float)x / (resolution - 1);
                float vy = (float)y / (resolution - 1);

                Vector3 viewportPoint = new Vector3(vx, vy, 0);
                Ray ray = cam.ViewportPointToRay(viewportPoint);

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
                {
                    if (hit.collider.CompareTag(targetTag))
                    {
                        hitPoints.Add(hit.point);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (hitPoints == null || hitPoints.Count == 0) return;

        Gizmos.color = Color.green;
        foreach (var point in hitPoints)
        {
            Gizmos.DrawSphere(point, 0.1f); // Kích th??c ?i?m v?
        }
    }
}
