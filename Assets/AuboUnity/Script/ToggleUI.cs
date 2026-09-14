using UnityEngine;
using TMPro; // 确保引入 TextMeshPro 命名空间

public class ToggleUI : MonoBehaviour
{
    public GameObject[] uiElements; // UI元素的数组，可以通过Inspector分配多个UI对象
    public TextMeshProUGUI buttonText; // TextMeshPro按钮上的文本

    private bool isUIVisible = true; // 当前UI的显示状态，初始为显示

    // 方法用于切换UI的显示状态
    public void Toggle()
    {
        isUIVisible = !isUIVisible; // 切换状态

        // 更新UI对象的激活状态
        foreach (GameObject uiElement in uiElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(isUIVisible);
            }
        }

        // 更新按钮文本
        if (buttonText != null)
        {
            buttonText.text = isUIVisible ? "关闭 UI" : "打开 UI";
        }
    }

    // 使用快捷键切换UI的显示状态
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // 按下 "U" 键切换状态
        {
            Toggle();
        }
    }
}
