using UnityEngine;
using TMPro; // 引入 TextMeshPro 的命名空间

public class PoseInput : MonoBehaviour
{
    public GameObject targetObject; // 当前目标物体
    public TMP_InputField inputX;   // 输入框：X 坐标
    public TMP_InputField inputY;   // 输入框：Y 坐标
    public TMP_InputField inputZ;   // 输入框：Z 坐标
    public TMP_InputField inputYaw; // 输入框：Y 轴旋转角度

    void Start()
    {
        // 初始化：确保目标物体正确设置并加载默认值
        UpdateTarget(targetObject); // 初始设置

        // 为输入框添加事件监听
        inputX.onValueChanged.AddListener((value) => OnInputChanged());
        inputY.onValueChanged.AddListener((value) => OnInputChanged());
        inputZ.onValueChanged.AddListener((value) => OnInputChanged());
        inputYaw.onValueChanged.AddListener((value) => OnInputChanged());
    }

    public void UpdateTarget(GameObject newTarget)
    {
        targetObject = newTarget;

        if (targetObject != null)
        {
            // 加载保存的数据
            float x = PlayerPrefs.GetFloat($"PosX_{targetObject.name}", targetObject.transform.position.x);
            float y = PlayerPrefs.GetFloat($"PosY_{targetObject.name}", targetObject.transform.position.y);
            float z = PlayerPrefs.GetFloat($"PosZ_{targetObject.name}", targetObject.transform.position.z);
            float yaw = PlayerPrefs.GetFloat($"Yaw_{targetObject.name}", targetObject.transform.eulerAngles.y);

            // 设置物体位置和旋转
            targetObject.transform.position = new Vector3(x, y, z);
            Vector3 currentRotation = targetObject.transform.eulerAngles;
            targetObject.transform.rotation = Quaternion.Euler(currentRotation.x, yaw, currentRotation.z);

            // 更新输入框
            inputX.text = x.ToString("F4");
            inputY.text = y.ToString("F4");
            inputZ.text = z.ToString("F4");
            inputYaw.text = yaw.ToString("F2");

            Debug.Log($"目标物体更新成功：{targetObject.name}");
        }
        else
        {
            Debug.LogError("目标物体为空，无法更新！");
        }
    }

    void OnInputChanged()
    {
        if (targetObject == null)
        {
            Debug.LogError("目标物体丢失，无法更新位置和旋转！");
            return;
        }

        // 尝试解析输入框内容
        if (float.TryParse(inputX.text, out float x) &&
            float.TryParse(inputY.text, out float y) &&
            float.TryParse(inputZ.text, out float z) &&
            float.TryParse(inputYaw.text, out float yaw))
        {
            // 获取当前的 Pitch 和 Roll
            Vector3 currentRotation = targetObject.transform.eulerAngles;
            float pitch = currentRotation.x;
            float roll = currentRotation.z;

            // 更新物体的位置
            targetObject.transform.position = new Vector3(x, y, z);

            // 更新物体的旋转
            targetObject.transform.rotation = Quaternion.Euler(pitch, yaw, roll);

            // 保存数据
            PlayerPrefs.SetFloat($"PosX_{targetObject.name}", x);
            PlayerPrefs.SetFloat($"PosY_{targetObject.name}", y);
            PlayerPrefs.SetFloat($"PosZ_{targetObject.name}", z);
            PlayerPrefs.SetFloat($"Yaw_{targetObject.name}", yaw);
            PlayerPrefs.Save();

            Debug.Log($"物体更新成功：位置({x}, {y}, {z})，Yaw({yaw})");
        }
        else
        {
            Debug.LogError("输入值无效，更新失败！");
        }
    }

    public void ClearSavedData()
    {
        // 清理所有保存的数据
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("已清理所有保存的数据！");
    }
}


