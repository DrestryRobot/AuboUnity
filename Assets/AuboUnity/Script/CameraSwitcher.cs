using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras; // 相机数组
    private int currentCameraIndex = 0; // 当前激活的相机索引

    void Start()
    {
        // 确保只有第一个相机激活（默认激活第一个相机）
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }

        Debug.Log($"默认激活相机：{cameras[currentCameraIndex].name}");
    }

    void Update()
    {
        // 监听“C”键，用于循环切换相机
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchToNextCamera();
        }
    }

    // 切换到指定相机
    public void SwitchToCamera(int cameraIndex)
    {
        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
        {
            Debug.LogError("无效的相机索引！");
            return;
        }

        // 禁用所有相机
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == cameraIndex);
        }

        // 更新当前相机索引
        currentCameraIndex = cameraIndex;

        Debug.Log($"切换到相机：{cameras[currentCameraIndex].name}");
    }

    // 切换到下一个相机（循环切换）
    private void SwitchToNextCamera()
    {
        // 禁用当前相机
        cameras[currentCameraIndex].gameObject.SetActive(false);

        // 更新当前相机索引（循环切换）
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;

        // 激活下一个相机
        cameras[currentCameraIndex].gameObject.SetActive(true);

        Debug.Log($"切换到相机：{cameras[currentCameraIndex].name}");
    }
}
