using UnityEngine;

public class ViewControl : MonoBehaviour
{
    public float zoomSpeed = 200f;  // 滚轮放大缩小速度
    public float panSpeed = 0.1f;   // 平移速度
    public float rotateSpeed = 10f; // 旋转速度

    private Vector3 lastMousePosition; // 记录鼠标最后的位置

    void Update()
    {
        // 实现滚轮放大缩小
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 zoom = Vector3.forward * scroll * zoomSpeed * Time.deltaTime;
            transform.Translate(zoom, Space.Self);
        }

        // 按中键拖动平移
        if (Input.GetMouseButton(2)) // 鼠标中键
        {
            if (Input.GetMouseButtonDown(2))
            {
                // 初始化鼠标拖动，避免跳动
                lastMousePosition = Input.mousePosition;
            }

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition; // 鼠标移动差值
            Vector3 pan = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * panSpeed;
            transform.Translate(pan * Time.deltaTime, Space.Self);

            lastMousePosition = Input.mousePosition; // 更新鼠标位置
        }

        // 按左键拖动旋转视角
        if (Input.GetMouseButton(0)) // 鼠标左键
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 初始化鼠标拖动，避免跳动
                lastMousePosition = Input.mousePosition;
            }

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition; // 鼠标移动差值
            float rotationX = -mouseDelta.y * rotateSpeed * Time.deltaTime;
            float rotationY = mouseDelta.x * rotateSpeed * Time.deltaTime;

            // 使用平滑旋转
            transform.rotation *= Quaternion.Euler(rotationX, rotationY, 0);

            lastMousePosition = Input.mousePosition; // 更新鼠标位置
        }
    }
}

