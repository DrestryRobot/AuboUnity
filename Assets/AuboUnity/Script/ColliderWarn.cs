//using UnityEngine;
//using TMPro; // 确保引入TextMeshPro命名空间

//public class ColliderWarn : MonoBehaviour
//{
//    public Transform rayOrigin; // 射线的起点
//    public float rayDistance = 0.01f; // 射线的最大检测距离
//    public LayerMask detectionLayer; // 用于过滤检测的层级
//    public TextMeshProUGUI resultText; // 用于显示结果的UI文本
//    public int peripheralRayCount = 16; // 垂直于正方向的射线数量（默认为8）

//    void Start()
//    {
//        // 初始化UI文本的显示状态
//        if (resultText != null)
//        {
//            resultText.gameObject.SetActive(false);
//        }
//    }

//    void Update()
//    {
//        PerformRaycast(); // 检测正方向的碰撞
//        PerformPeripheralRaycast(); // 检测垂直方向的碰撞
//    }

//    void PerformRaycast()
//    {
//        // 可视化正方向射线
//        Vector3 direction = rayOrigin.forward; // 射线方向
//        Debug.DrawRay(rayOrigin.position, direction * rayDistance, Color.red);

//        RaycastHit hit;

//        // 发射正方向射线，检测碰撞体
//        if (Physics.Raycast(rayOrigin.position, direction, out hit, rayDistance, detectionLayer))
//        {
//            // 检测命中的碰撞体
//            if (hit.collider != null)
//            {
//                resultText.gameObject.SetActive(true);
//                resultText.text = $"末端即将正碰撞: {hit.collider.name}";
//                return;
//            }
//        }

//        // 如果未检测到任何对象，隐藏UI
//        resultText.gameObject.SetActive(false);
//    }

//    void PerformPeripheralRaycast()
//    {
//        // 在垂直于正方向的平面上均匀分布8根射线
//        Vector3 forward = rayOrigin.forward; // 正方向向量
//        Vector3 up = rayOrigin.up;           // 向上的方向向量（用作基准）

//        // 获取垂直平面上的另一个基准向量
//        Vector3 right = Vector3.Cross(forward, up).normalized;

//        for (int i = 0; i < peripheralRayCount; i++)
//        {
//            // 计算当前射线的角度（将360度分成8份）
//            float angle = (360f / peripheralRayCount) * i;
//            Quaternion rotation = Quaternion.AngleAxis(angle, forward); // 绕正方向旋转

//            // 计算当前射线的方向向量
//            Vector3 rayDirection = rotation * right;

//            // 可视化射线
//            Debug.DrawRay(rayOrigin.position, rayDirection * rayDistance, Color.blue);

//            // 检测碰撞体
//            RaycastHit hit;
//            if (Physics.Raycast(rayOrigin.position, rayDirection, out hit, rayDistance, detectionLayer))
//            {
//                if (hit.collider != null)
//                {
//                    resultText.gameObject.SetActive(true);
//                    resultText.text = $"末端即将侧碰撞: {hit.collider.name}";
//                    return;
//                }
//            }
//        }
//    }

//    // 方法用于动态更新 rayDistance 的值
//    public void UpdateRayDistance(float newRayDistance)
//    {
//        rayDistance = newRayDistance; // 更新射线距离
//    }
//}


using UnityEngine;
using TMPro; // 确保引入TextMeshPro命名空间

public class ColliderWarn : MonoBehaviour
{
    public Transform rayOrigin; // 射线的起点
    public float rayDistance = 0.01f; // 射线的最大检测距离
    public LayerMask detectionLayer; // 用于过滤检测的层级
    public TextMeshProUGUI resultText; // 用于显示结果的UI文本
    public int peripheralRayCount = 16; // 垂直于正方向的射线数量（默认为8）

    void Start()
    {
        // 初始化UI文本的显示状态
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = ""; // 初始化为空文本
        }
    }

    void Update()
    {
        PerformRaycast(); // 检测正方向的碰撞
        PerformPeripheralRaycast(); // 检测垂直方向的碰撞
    }

    void PerformRaycast()
    {
        // 可视化正方向射线
        Vector3 direction = rayOrigin.forward; // 射线方向
        Debug.DrawRay(rayOrigin.position, direction * rayDistance, Color.red);

        RaycastHit hit;

        // 发射正方向射线，检测碰撞体
        if (Physics.Raycast(rayOrigin.position, direction, out hit, rayDistance, detectionLayer))
        {
            // 检测命中的碰撞体
            if (hit.collider != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = $"末端即将正碰撞: {hit.collider.name}";
                return;
            }
        }

        // 如果未检测到任何对象，设置UI显示为空文本
        resultText.gameObject.SetActive(true);
        resultText.text = ""; // 显示空文本
    }

    void PerformPeripheralRaycast()
    {
        // 在垂直于正方向的平面上均匀分布射线
        Vector3 forward = rayOrigin.forward; // 正方向向量
        Vector3 up = rayOrigin.up;           // 向上的方向向量（用作基准）

        // 获取垂直平面上的另一个基准向量
        Vector3 right = Vector3.Cross(forward, up).normalized;

        bool peripheralCollisionDetected = false; // 标记是否检测到碰撞

        for (int i = 0; i < peripheralRayCount; i++)
        {
            // 计算当前射线的角度（将360度分成多份）
            float angle = (360f / peripheralRayCount) * i;
            Quaternion rotation = Quaternion.AngleAxis(angle, forward); // 绕正方向旋转

            // 计算当前射线的方向向量
            Vector3 rayDirection = rotation * right;

            // 可视化射线
            Debug.DrawRay(rayOrigin.position, rayDirection * rayDistance, Color.blue);

            // 检测碰撞体
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin.position, rayDirection, out hit, rayDistance, detectionLayer))
            {
                if (hit.collider != null)
                {
                    resultText.gameObject.SetActive(true);
                    resultText.text = $"末端即将侧碰撞: {hit.collider.name}";
                    peripheralCollisionDetected = true; // 更新标记
                    return;
                }
            }
        }

        // 如果未检测到任何对象，设置UI显示为空文本
        if (!peripheralCollisionDetected)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = ""; // 显示空文本
        }
    }

    // 方法用于动态更新 rayDistance 的值
    public void UpdateRayDistance(float newRayDistance)
    {
        rayDistance = newRayDistance; // 更新射线距离
    }
}
