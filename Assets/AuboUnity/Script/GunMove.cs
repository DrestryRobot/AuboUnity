using TMPro;
using UnityEngine;

public class GunMove : MonoBehaviour
{
    // 关节对象
    public Transform joint;

    // 初始偏移和数据
    private float gunAngle = 0f; // 存储 gunAngle 值

    void Start()
    {
        // 初始化数组
        gunAngle = 0f;
    }

    public void UpdateGunAngle(float angle)
    {
        // 保存 gunAngle 值
        gunAngle = angle;

        // 更新关节角度并应用到关节
        ApplyJointAngles();
    }


    void ApplyJointAngles()
    {
        // 应用关节角度到对应对象
        if (joint != null) joint.localRotation = Quaternion.Euler(-gunAngle, 0, 0);
    }
}
