using UnityEngine;

public class PixelPerfectFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10); // Keeps Z at -10
    public float pixelsPerUnit = 32; // Match your project's PPU!

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Get target destination
        Vector3 desiredPosition = target.position + offset;

        // 2. Snap coordinates directly to the pixel grid
        float unitsPerPixel = 1f / pixelsPerUnit;
        desiredPosition.x = Mathf.Round(desiredPosition.x / unitsPerPixel) * unitsPerPixel;
        desiredPosition.y = Mathf.Round(desiredPosition.y / unitsPerPixel) * unitsPerPixel;

        // 3. Apply position directly without smoothing to prevent jitter
        transform.position = desiredPosition;
    }
}