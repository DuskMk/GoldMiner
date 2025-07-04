using UnityEngine;
using UnityEditor; // ��������UnityEditor�����ռ�

[CustomEditor(typeof(SpawnAreaProfile))]
public class SpawnAreaProfileEditor : Editor
{
    void OnSceneGUI()
    {
        SpawnAreaProfile profile = (SpawnAreaProfile)target;
        // �������λ��
        Handles.color = Color.green;
        Handles.DrawWireDisc(profile.cameraPosition, Vector3.up, 0.5f);
        Handles.Label(profile.cameraPosition + Vector3.up * 0.5f, "Camera Position");
        // ���������Ұ
        Handles.color = Color.blue;
        Handles.DrawWireCube(profile.cameraPosition, new Vector3(profile.cameraOrthographicSize * 2, profile.cameraOrthographicSize * 2, 0));
    }
}