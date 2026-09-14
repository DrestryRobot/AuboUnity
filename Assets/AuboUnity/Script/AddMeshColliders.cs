using UnityEngine;

public class AddMeshColliders : MonoBehaviour
{
    void Start()
    {
        // 遍历当前对象及其所有子对象
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // 检查是否已经有MeshCollider
            if (meshFilter.GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh; // 设置Mesh为MeshFilter的网格
                meshCollider.convex = false; // 设置为非凸面（静态对象）
            }
        }
    }
}
