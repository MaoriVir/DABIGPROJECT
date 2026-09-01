using UnityEngine;

public class lightningStrike : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform player;
    public Transform enemy;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (player != null && enemy != null)
        {
            // Update the start and end points every frame to follow movement
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, enemy.position);
        }
        else
        {
            // Destroy the lightning if either character disappears
            Destroy(gameObject);
        }
    }
}
