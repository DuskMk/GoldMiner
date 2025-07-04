using UnityEngine;
using UnityEditor; // 必须引入UnityEditor命名空间

[CustomEditor(typeof(SpawnAreaProfile))]
public class SpawnAreaProfileEditor : Editor
{
    void OnSceneGUI()
    {
        SpawnAreaProfile profile = (SpawnAreaProfile)target;
        // 绘制相机位置
        Handles.color = Color.green;
        Handles.DrawWireDisc(profile.cameraPosition, Vector3.up, 0.5f);
        Handles.Label(profile.cameraPosition + Vector3.up * 0.5f, "Camera Position");
        // 绘制相机视野
        Handles.color = Color.blue;
        Handles.DrawWireCube(profile.cameraPosition, new Vector3(profile.cameraOrthographicSize * 2, profile.cameraOrthographicSize * 2, 0));
    }
}