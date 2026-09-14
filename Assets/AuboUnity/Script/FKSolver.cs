using UnityEngine;

public class FKSolver : MonoBehaviour
{
    // 引用 TCP 服务器脚本，从中实时获取关节角数据
    public TcpServer tcpServer;
    // 从 TCP 服务器获取的关节角数组（单位：度）
    public float[] jointAngles = new float[6];

    // 改进 DH 参数定义
    // 连杆长度 a（对应公式中的 a_(i-1)，单位：米）
    public float[] a = { 0f, 0f, 0.647f, 0.6005f, 0f, 0f };
    // 关节偏移 d（对应公式中的 d_i，单位：米）
    public float[] d = { 0.163f, 0.2013f, 0f, 0f, 0.1025f, 0.094f };
    // 扭转角 alpha（对应公式中的 α_(i-1)，单位：度）
    public float[] alpha = { 0f, -90f, 180f, 180f, -90f, 90f };
    // 关节角偏移量（用于修正 DH 参数定义时坐标系偏差，单位：度）
    public float[] offsets = { 180f, -90f, 0f, -90f, 0f, 0f };

    void Update()
    {
        // 实时从 TCP 服务器获取关节角度数据
        if (tcpServer != null)
        {
            jointAngles = tcpServer.GetJointAngles();
        }
        // 更新正向运动学计算
        UpdateForwardKinematics();
    }

    /// <summary>
    /// 根据实时关节角计算机械臂末端位姿，并更新显示
    /// </summary>
    private void UpdateForwardKinematics()
    {
        // 将输入关节角按照各自的偏移量进行修正
        float[] correctedTheta = new float[jointAngles.Length];
        for (int i = 0; i < jointAngles.Length; i++)
        {
            correctedTheta[i] = jointAngles[i] + offsets[i];
        }

        // 从初始齐次变换矩阵开始迭代计算各关节的正向运动学
        Matrix4x4 T = Matrix4x4.identity;
        for (int i = 0; i < correctedTheta.Length; i++)
        {
            T = T * DHMatrix(correctedTheta[i], d[i], a[i], alpha[i]);
        }

        // 提取末端位置（矩阵第四列）
        Vector3 endEffectorPos = T.GetColumn(3);
        // 提取末端旋转信息（直接使用 Matrix4x4 提供的 rotation 属性，并转换成欧拉角）
        Quaternion endEffectorRot = T.rotation;

        // 利用格式化字符串保留4位小数输出末端位置
        string posStr = string.Format("({0:F4}, {1:F4}, {2:F4})",
                                       endEffectorPos.x, endEffectorPos.y, endEffectorPos.z);
        // 同样保留4位小数输出末端旋转欧拉角
        string rotStr = string.Format("({0:F4}, {1:F4}, {2:F4})",
                                       endEffectorRot.eulerAngles.x, endEffectorRot.eulerAngles.y, endEffectorRot.eulerAngles.z);

        // 输出到控制台（调试用）
        Debug.Log("末端位置：" + posStr);
        Debug.Log("末端旋转：" + rotStr);

    }

    /// <summary>
    /// 根据改进 DH 方法计算单个关节的齐次变换矩阵
    /// 参数说明：
    ///   theta  : 当前关节角（单位：度）
    ///   d      : 关节偏移（单位：米）
    ///   a      : 连杆长度（单位：米）
    ///   alpha  : 扭转角（单位：度）
    /// 按照公式：
    /// [ cosθ         -sinθ           0         a ]
    /// [ sinθ*cosα    cosθ*cosα    -sinα    -d*sinα ]
    /// [ sinθ*sinα    cosθ*sinα     cosα     d*cosα ]
    /// [   0             0           0         1 ]
    /// </summary>
    Matrix4x4 DHMatrix(float theta, float d, float a, float alpha)
    {
        float radTheta = Mathf.Deg2Rad * theta;
        float radAlpha = Mathf.Deg2Rad * alpha;

        Matrix4x4 dhMatrix = new Matrix4x4();
        // 第一行: [ cosθ, -sinθ, 0, a ]
        dhMatrix.SetRow(0, new Vector4(Mathf.Cos(radTheta),
                                       -Mathf.Sin(radTheta),
                                        0,
                                        a));
        // 第二行: [ sinθ*cosα, cosθ*cosα, -sinα, -d*sinα ]
        dhMatrix.SetRow(1, new Vector4(Mathf.Sin(radTheta) * Mathf.Cos(radAlpha),
                                       Mathf.Cos(radTheta) * Mathf.Cos(radAlpha),
                                       -Mathf.Sin(radAlpha),
                                       -d * Mathf.Sin(radAlpha)));
        // 第三行: [ sinθ*sinα, cosθ*sinα, cosα, d*cosα ]
        dhMatrix.SetRow(2, new Vector4(Mathf.Sin(radTheta) * Mathf.Sin(radAlpha),
                                       Mathf.Cos(radTheta) * Mathf.Sin(radAlpha),
                                       Mathf.Cos(radAlpha),
                                       d * Mathf.Cos(radAlpha)));
        // 第四行: [ 0, 0, 0, 1 ]
        dhMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

        return dhMatrix;
    }
}


//using UnityEngine;

//public class FKSolver : MonoBehaviour
//{
//    // 引用 JointMove
//    public JointMove jointMove;

//    // TCP 服务器脚本的引用（保持不变）
//    public TcpServer tcpServer;

//    public float[] jointAngles = new float[6];

//    public float[] a = { 0f, 0f, 0.647f, 0.6005f, 0f, 0f };
//    public float[] d = { 0.163f, 0.2013f, 0f, 0f, 0.1025f, 0.094f };
//    public float[] alpha = { 0f, -90f, 180f, 180f, -90f, 90f };
//    public float[] offsets = { 180f, -90f, 0f, -90f, 0f, 0f };

//    void Update()
//    {
//        if (tcpServer != null)
//        {
//            jointAngles = tcpServer.GetJointAngles();
//        }
//        UpdateForwardKinematics();
//    }

//    private void UpdateForwardKinematics()
//    {
//        float[] correctedTheta = new float[jointAngles.Length];
//        for (int i = 0; i < jointAngles.Length; i++)
//        {
//            correctedTheta[i] = jointAngles[i] + offsets[i];
//        }

//        Matrix4x4 T = Matrix4x4.identity;
//        for (int i = 0; i < correctedTheta.Length; i++)
//        {
//            T = T * DHMatrix(correctedTheta[i], d[i], a[i], alpha[i]);
//        }

//        Vector3 endEffectorPos = T.GetColumn(3);
//        Quaternion endEffectorRot = T.rotation;

//        // 构造末端位姿数据数组
//        float[] pose = new float[6];
//        pose[0] = endEffectorPos.x;
//        pose[1] = endEffectorPos.y;
//        pose[2] = endEffectorPos.z;
//        pose[3] = endEffectorRot.eulerAngles.x;
//        pose[4] = endEffectorRot.eulerAngles.y;
//        pose[5] = endEffectorRot.eulerAngles.z;

//        // 使用 JointMove 更新位姿
//        if (jointMove != null)
//        {
//            jointMove.UpdateEndEffectorPose(pose);
//        }
//    }

//    private Matrix4x4 DHMatrix(float theta, float d, float a, float alpha)
//    {
//        float radTheta = Mathf.Deg2Rad * theta;
//        float radAlpha = Mathf.Deg2Rad * alpha;

//        Matrix4x4 dhMatrix = new Matrix4x4();
//        dhMatrix.SetRow(0, new Vector4(Mathf.Cos(radTheta), -Mathf.Sin(radTheta), 0, a));
//        dhMatrix.SetRow(1, new Vector4(Mathf.Sin(radTheta) * Mathf.Cos(radAlpha), Mathf.Cos(radTheta) * Mathf.Cos(radAlpha), -Mathf.Sin(radAlpha), -d * Mathf.Sin(radAlpha)));
//        dhMatrix.SetRow(2, new Vector4(Mathf.Sin(radTheta) * Mathf.Sin(radAlpha), Mathf.Cos(radTheta) * Mathf.Sin(radAlpha), Mathf.Cos(radAlpha), d * Mathf.Cos(radAlpha)));
//        dhMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

//        return dhMatrix;
//    }
//}
