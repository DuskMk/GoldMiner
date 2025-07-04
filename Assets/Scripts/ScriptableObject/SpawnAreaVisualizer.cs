using UnityEngine;

public class SpawnAreaVisualizer : MonoBehaviour
{
    public SpawnAreaProfile profile;

    private void OnDrawGizmos()
    {
        if (profile == null) return;

        Gizmos.color = new Color(1, 0.5f, 0f, 0.4f); // ³ÈÉ«°ëÍ¸Ã÷
        Rect rect = profile.spawnBounds;

        Vector3 center = new Vector3(rect.x + rect.width / 2f, rect.y + rect.height / 2f, 0);
        Vector3 size = new Vector3(rect.width, rect.height, 0.1f);

        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
