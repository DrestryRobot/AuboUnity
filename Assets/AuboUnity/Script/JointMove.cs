using System.Text;
using System;
using TMPro;
using UnityEngine;


public class JointMove : MonoBehaviour
{
    // 关节对象
    public Transform joint1;
    public Transform joint2;
    public Transform joint3;
    public Transform joint4;
    public Transform joint5;
    public Transform joint6;

    // UI 文本对象
    public TMP_Text angleDisplayText;    // 单个文本显示所有关节角度
    public TMP_Text poseDisplayText;     // 单个文本显示末端位姿数据
    public TMP_Text gunAngleDisplayText; // 单个文本显示 gunAngle 数据
    public TMP_Text commandDisplayText;  // 单个文本显示指令
    public TMP_Text piciDisplayText;  // 单个文本显示指令
    public TMP_Text bianhaoDisplayText;  // 单个文本显示指令
    public TMP_Text workDisplayText;  // 单个文本显示指令

    // 初始偏移和数据
    private float[] originalAngles = new float[6]; // 原始输入角度
    private float[] endEffectorPose = new float[6]; // 存储末端位姿数据
    private float gunAngle = 0f; // 存储 gunAngle 值
    private string commandValue1; // 存储当前的指令值，-1 表示未接收到任何指令
    private string commandValue2; // 存储当前的指令值，-1 表示未接收到任何指令
    private string commandValue3; // 存储当前的指令值，-1 表示未接收到任何指令


    void Start()
    {
        // 初始化数组
        originalAngles = new float[6];
        endEffectorPose = new float[6];
        UpdateUIText(); // 初始化 UI 显示
    }

    public void UpdateJointAngles(float[] angles)
    {
        if (angles.Length != 6) return;

        // 保存原始输入的角度
        for (int i = 0; i < 6; i++)
        {
            originalAngles[i] = angles[i];
        }

        // 更新关节角度并应用到关节
        ApplyJointAngles();
        UpdateUIText(); // 更新显示
    }

    public void UpdateEndEffectorPose(float[] pose)
    {
        if (pose.Length != 6) return;

        // 保存末端位姿数据
        for (int i = 0; i < 6; i++)
        {
            endEffectorPose[i] = pose[i];
        }

        // 更新 UI 显示
        UpdateUIText();
    }

    public void UpdateGunAngle(float angle)
    {
        // 保存 gunAngle 值
        gunAngle = angle;

        // 更新 UI 显示
        UpdateUIText();
    }

    public void UpdateCommand(string command)
    {
        // 保存指令值
        commandValue1 = command;

        // 更新 UI 显示
        UpdateUIText();
    }

    public void UpdatePici(string command)
    {
        // 保存指令值
        commandValue2 = command;

        // 更新 UI 显示
        UpdateUIText();
    }

    public void UpdateBianhao(string command)
    {
        // 保存指令值
        commandValue3 = command;

        // 更新 UI 显示
        UpdateUIText();
    }

    void ApplyJointAngles()
    {
        // 应用关节角度到对应对象
        if (joint1 != null) joint1.localRotation = Quaternion.Euler(0, -(originalAngles[0] - 270f), 0);
        if (joint2 != null) joint2.localRotation = Quaternion.Euler(-originalAngles[1], 0, 0);
        if (joint3 != null) joint3.localRotation = Quaternion.Euler(originalAngles[2], 0, 0);
        if (joint4 != null) joint4.localRotation = Quaternion.Euler(-originalAngles[3], 0, 0);
        if (joint5 != null) joint5.localRotation = Quaternion.Euler(0, -originalAngles[4], 0);
        if (joint6 != null) joint6.localRotation = Quaternion.Euler(-(originalAngles[5] - 90f), 0, 0);
    }

    void UpdateUIText()
    {
        // 更新关节角度的显示
        if (angleDisplayText != null)
        {
            angleDisplayText.text =
                $"关节角:\n" +
                $"关节1: {originalAngles[0]:F2}度\n" +
                $"关节2: {originalAngles[1]:F2}度\n" +
                $"关节3: {originalAngles[2]:F2}度\n" +
                $"关节4: {originalAngles[3]:F2}度\n" +
                $"关节5: {originalAngles[4]:F2}度\n" +
                $"关节6: {originalAngles[5]:F2}度";
        }

        // 计算装配状态
        string GetAssemblyStatus(float value, float max)
        {
            if (value == 0) return "<color=#FF0000>待完成</color>";    // 红色
            if (value > 0 && value < max) return "<color=#FFFF00>进行中</color>"; // 黄色
            if (value == max) return "<color=#00FF00>已完成</color>";  // 绿色
            return "<color=#FFFFFF>未知状态</color>"; // 白色
        }

        // 更新装配进度的显示
        if (workDisplayText != null)
        {
            workDisplayText.text =
                "装配进度:\n" +
                $"引信天线: {GetAssemblyStatus(endEffectorPose[1], 72)}\n" +
                $"卫星天线: {GetAssemblyStatus(endEffectorPose[2], 24)}\n" +
                $"指令天线: {GetAssemblyStatus(endEffectorPose[3], 10)}";
        }

        // 更新末端位姿的显示
        if (poseDisplayText != null)
        {

            string processStatus = endEffectorPose[0] switch
            {
                0 => "空闲状态",
                1 => "引信天线",
                2 => "卫星天线",
                3 => "指令天线",
                _ => "未知状态"
            };

            poseDisplayText.text =
                $"状态信息:\n" +
                $"装配工艺: {processStatus}\n" +
                $"引信天线: {endEffectorPose[1] * 100 / 72:F0}%\n" +
                $"卫星天线: {endEffectorPose[2] * 100 / 24:F0}%\n" +
                $"指令天线: {endEffectorPose[3] * 100 / 10:F0}%\n" +
                $"实时力矩: {endEffectorPose[4] * 0.01:F2}N.m\n" +
                $"接触检测: {(endEffectorPose[5] == 0 ? "无接触" : "<color=#FF0000>已接触</color>")}";
        }


        // 更新 gunAngle 的显示
        if (gunAngleDisplayText != null)
        {
            gunAngleDisplayText.text = $"滚环角度: {gunAngle:F1}度";
        }

        // 更新指令的显示
        if (commandDisplayText != null)
        {
            commandDisplayText.text = $"{commandValue1}";
        }

        // 更新批次的显示
        if (piciDisplayText != null)
        {
            piciDisplayText.text = $"批次 {commandValue2}";
        }

        // 更新编号的显示
        if (bianhaoDisplayText != null)
        {
            bianhaoDisplayText.text = $"编号 {commandValue3}";
        }
    }
}
