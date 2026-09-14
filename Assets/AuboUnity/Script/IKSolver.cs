using System;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    // 末端位姿参数（单位：毫米与度）
    [SerializeField] float x = 625.640f;
    [SerializeField] float y = 142.817f;
    [SerializeField] float z = -146.700f;
    [SerializeField] float roll_deg = 75.9f;   // 绕 X 轴旋转
    [SerializeField] float pitch_deg = 1.6f;   // 绕新 Y 轴旋转
    [SerializeField] float yaw_deg = -149.7f;  // 绕新 Z 轴旋转

    // 杆长参数（与 MATLAB 中一致）
    float L1 = 0f, L2 = 0f, L3 = 390f, L4 = 0f, L5 = 348.5f, L6 = 65f;
    // 参数赋值
    double d1, a2, a3, a4, d4, d6;

    // 关节角度限制 [min, max]（单位：度），对应：θ₁, θ₂, θ₃, θ₄, θ₅, θ₆
    float[,] joint_limits = new float[6, 2] {
        { -180f, 180f },
        {    0f, 180f },
        {  -90f,  90f },
        { -180f, 180f },
        {  -90f,  90f },
        { -180f, 180f }
    };

    void Start()
    {
        // 设置机械臂参数
        d1 = L1; a2 = L2; a3 = L3; a4 = L4; d4 = L5; d6 = L6;

        // 计算齐次变换矩阵 T
        Matrix4x4 T = ComputeT();
        Debug.Log("齐次变换矩阵 T：\n" +
            $"[{T.m00:F6}, {T.m01:F6}, {T.m02:F6}, {T.m03:F6}]\n" +
            $"[{T.m10:F6}, {T.m11:F6}, {T.m12:F6}, {T.m13:F6}]\n" +
            $"[{T.m20:F6}, {T.m21:F6}, {T.m22:F6}, {T.m23:F6}]\n" +
            $"[{T.m30:F6}, {T.m31:F6}, {T.m32:F6}, {T.m33:F6}]");

        // 提取 T 矩阵分量（按 MATLAB 中 T 排列，对应 R2 的各元素和平移列）
        double nx = T.m00, ox = T.m01, ax = T.m02, px = T.m03;
        double ny = T.m10, oy = T.m11, ay = T.m12, py = T.m13;
        double nz = T.m20, oz = T.m21, az = T.m22, pz = T.m23;

        List<double[]> solutions = CalculateIK(nx, ox, ax, px,
                                                 ny, oy, ay, py,
                                                 nz, oz, az, pz);

        Debug.Log("符合关节限制的解（度）：");
        if (solutions.Count == 0)
        {
            Debug.Log("无可行解");
        }
        else
        {
            foreach (double[] sol in solutions)
            {
                Debug.Log(string.Format("[{0:F4}, {1:F4}, {2:F4}, {3:F4}, {4:F4}, {5:F4}]",
                    sol[0], sol[1], sol[2], sol[3], sol[4], sol[5]));
            }
        }
    }

    /// <summary>
    /// 计算逆运动学解（单位：度），参数为 T 矩阵各分量（double 类型）。
    /// 按 MATLAB 代码流程计算θ₁ ~ θ₆，并返回符合关节限制的解集合。
    /// </summary>
    List<double[]> CalculateIK(double nx, double ox, double ax, double px,
                                 double ny, double oy, double ay, double py,
                                 double nz, double oz, double az, double pz)
    {
        List<double[]> sols = new List<double[]>();

        // θ₁ 的两候选解
        double[] theta1Candidates = new double[2];
        theta1Candidates[0] = Math.Atan2(-(d6 * ay - py), -(d6 * ax - px)) * (180.0 / Math.PI);
        theta1Candidates[1] = theta1Candidates[0] + 180.0;

        double[] signArray = new double[] { 1.0, -1.0 };

        foreach (double rawTheta1 in theta1Candidates)
        {
            double theta1 = NormalizeAngle(rawTheta1);
            double s1 = Math.Sin(theta1 * Math.PI / 180.0);
            double c1 = Math.Cos(theta1 * Math.PI / 180.0);

            // 中间变量
            double f1 = c1 * ax + s1 * ay;
            double f2 = c1 * px + s1 * py - a2;
            double f3 = pz - d1;

            double k1 = 2 * a3 * d4;
            double k2 = -2 * a3 * a4;
            double k3 = a4 * a4 + d4 * d4 + a3 * a3 - Math.Pow((-f1 * d6 + f2), 2)
                        - Math.Pow((az * d6 - f3), 2);

            double D = k1 * k1 + k2 * k2 - k3 * k3;
            if (D < 0)
                continue;
            double sqrt_D = Math.Sqrt(D);
            foreach (double sign in signArray)
            {
                double theta3_raw = (Math.Atan2(k3, sign * sqrt_D) - Math.Atan2(k2, k1)) * (180.0 / Math.PI);
                double theta3 = NormalizeAngle(theta3_raw);
                if (double.IsNaN(theta3))
                    continue;
                // 按 MATLAB 代码调整符号：
                theta3 = -theta3;
                double c3 = Math.Cos(theta3 * Math.PI / 180.0);
                double s3 = Math.Sin(theta3 * Math.PI / 180.0);

                // θ₂ 计算
                double g1 = f2 - d6 * f1;
                double g2 = f3 - d6 * az;
                double g3 = a4 * c3 - d4 * s3 + a3;
                double E = g1 * g1 + g2 * g2 - g3 * g3;
                if (E < 0)
                    continue;
                double sqrt_E = Math.Sqrt(E);
                foreach (double signE in signArray)
                {
                    double theta20 = (Math.Atan2(g3, signE * sqrt_E) - Math.Atan2(g2, g1)) * (180.0 / Math.PI);
                    theta20 = NormalizeAngle(theta20);
                    double theta2 = -(theta20 - 90.0); // 与 MATLAB 相同
                    if (double.IsNaN(theta20))
                        continue;

                    // θ₅ 计算（使用 acosd）
                    double c2 = Math.Cos(theta20 * Math.PI / 180.0);
                    double s2 = Math.Sin(theta20 * Math.PI / 180.0);
                    double s23 = s2 * c3 + c2 * s3;
                    double c23 = c2 * c3 - s2 * s3;
                    double acosInput = Math.Max(-1.0, Math.Min(1.0, f1 * c23 - az * s23));
                    double theta5_raw = Math.Acos(acosInput) * (180.0 / Math.PI);
                    double theta5 = NormalizeAngle(theta5_raw);
                    double[] theta5Candidates = new double[] { theta5, -theta5 };

                    foreach (double candidateTheta5 in theta5Candidates)
                    {
                        double s5 = Math.Sin(candidateTheta5 * Math.PI / 180.0);
                        if (Math.Abs(s5) < 1e-6)
                            continue;

                        // θ₄ 计算（使用 asind）
                        double asinInput = (-s1 * ax + c1 * ay) / s5;
                        asinInput = Math.Max(-1.0, Math.Min(1.0, asinInput));
                        double theta4_raw = Math.Asin(asinInput) * (180.0 / Math.PI);
                        double theta4 = NormalizeAngle(theta4_raw);
                        double[] theta4Candidates = new double[] { theta4, 180.0 - theta4 };

                        foreach (double candidateTheta4 in theta4Candidates)
                        {
                            double theta4_final = NormalizeAngle(candidateTheta4);
                            double c4 = Math.Cos(theta4_final * Math.PI / 180.0);

                            // θ₆ 计算
                            double h1 = -s1 * nx + c1 * ny;
                            double h2 = -s1 * ox + c1 * oy;
                            double h3 = -c4;
                            double F_calc = h1 * h1 + h2 * h2 - h3 * h3;
                            if (F_calc < 0)
                                continue;
                            double sqrt_F = Math.Sqrt(F_calc);
                            foreach (double signF in signArray)
                            {
                                double theta6_raw = (Math.Atan2(h3, signF * sqrt_F) - Math.Atan2(h2, h1)) * (180.0 / Math.PI);
                                double theta6 = NormalizeAngle(theta6_raw);
                                double[] sol = new double[6] { theta1, theta2, theta3, theta4_final, candidateTheta5, theta6 };
                                sols.Add(sol);
                            }
                        }
                    }
                }
            }
        }
        // 去除重复解（用容差 tol = 1e-4）
        List<double[]> uniqueSols = RemoveDuplicateSolutions(sols, 1e-4);
        // 根据关节限制筛选有效解
        List<double[]> validSols = new List<double[]>();
        foreach (double[] sol in uniqueSols)
        {
            bool valid = true;
            for (int j = 0; j < 6; j++)
            {
                if (sol[j] < joint_limits[j, 0] || sol[j] > joint_limits[j, 1])
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
                validSols.Add(sol);
        }
        return validSols;
    }

    /// <summary>
    /// 计算齐次变换矩阵 T（4×4）。
    /// 采用外旋顺序：R = Rz * Ry * Rx，然后按 MATLAB 代码重排得到 R2，
    /// 最后 T = [R2, [x;y;z]; 0 0 0 1].
    /// </summary>
    Matrix4x4 ComputeT()
    {
        // 将 roll, pitch, yaw 转换为弧度（采用 double 精度）
        double roll = roll_deg * Math.PI / 180.0;
        double pitch = pitch_deg * Math.PI / 180.0;
        double yaw = yaw_deg * Math.PI / 180.0;

        // 分别构造 3×3 旋转矩阵
        double[,] Rx = new double[3, 3] {
            { 1, 0, 0 },
            { 0, Math.Cos(roll), -Math.Sin(roll) },
            { 0, Math.Sin(roll), Math.Cos(roll) }
        };
        double[,] Ry = new double[3, 3] {
            { Math.Cos(pitch), 0, Math.Sin(pitch) },
            { 0, 1, 0 },
            { -Math.Sin(pitch), 0, Math.Cos(pitch) }
        };
        double[,] Rz = new double[3, 3] {
            { Math.Cos(yaw), -Math.Sin(yaw), 0 },
            { Math.Sin(yaw), Math.Cos(yaw), 0 },
            { 0, 0, 1 }
        };

        // 采用外旋顺序：R = Rz * Ry * Rx
        double[,] R_temp = MultiplyMatrix3x3(Ry, Rx);
        double[,] R_mat = MultiplyMatrix3x3(Rz, R_temp);

        // 根 据 MATLAB 代码重排列： R2 = [R(3,3) R(2,3) R(1,3);
        //                                      R(3,2) R(2,2) R(1,2);
        //                                      R(3,1) R(2,1) R(1,1)]
        double[,] R2 = new double[3, 3];
        R2[0, 0] = R_mat[2, 2]; R2[0, 1] = R_mat[1, 2]; R2[0, 2] = R_mat[0, 2];
        R2[1, 0] = R_mat[2, 1]; R2[1, 1] = R_mat[1, 1]; R2[1, 2] = R_mat[0, 1];
        R2[2, 0] = R_mat[2, 0]; R2[2, 1] = R_mat[1, 0]; R2[2, 2] = R_mat[0, 0];

        // 构造齐次变换矩阵 T = [R2, [x;y;z]; 0 0 0 1]
        Matrix4x4 T = new Matrix4x4();
        T.m00 = (float)R2[0, 0]; T.m01 = (float)R2[0, 1]; T.m02 = (float)R2[0, 2]; T.m03 = x;
        T.m10 = (float)R2[1, 0]; T.m11 = (float)R2[1, 1]; T.m12 = (float)R2[1, 2]; T.m13 = y;
        T.m20 = (float)R2[2, 0]; T.m21 = (float)R2[2, 1]; T.m22 = (float)R2[2, 2]; T.m23 = z;
        T.m30 = 0; T.m31 = 0; T.m32 = 0; T.m33 = 1;
        return T;
    }

    /// <summary>
    /// 3×3 矩阵乘法：计算 A*B，A、B 均为 3×3 double 数组
    /// </summary>
    double[,] MultiplyMatrix3x3(double[,] A, double[,] B)
    {
        double[,] C = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                C[i, j] = 0;
                for (int k = 0; k < 3; k++)
                {
                    C[i, j] += A[i, k] * B[k, j];
                }
            }
        }
        return C;
    }

    /// <summary>
    /// 将角度归一化到 [-180,180) 范围（double 版本），模仿 MATLAB 的 mod(theta+180,360)-180
    /// </summary>
    double NormalizeAngle(double angle)
    {
        angle = (angle + 180.0) % 360.0;
        if (angle < 0)
            angle += 360.0;
        return angle - 180.0;
    }

    /// <summary>
    /// 去除重复解：判断两解各分量误差是否均小于 tol
    /// </summary>
    List<double[]> RemoveDuplicateSolutions(List<double[]> sols, double tol)
    {
        List<double[]> uniqueSols = new List<double[]>();
        foreach (double[] sol in sols)
        {
            bool duplicate = false;
            foreach (double[] usol in uniqueSols)
            {
                if (AreSolutionsEqual(sol, usol, tol))
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
                uniqueSols.Add(sol);
        }
        return uniqueSols;
    }

    bool AreSolutionsEqual(double[] sol1, double[] sol2, double tol)
    {
        for (int i = 0; i < sol1.Length; i++)
        {
            if (Math.Abs(sol1[i] - sol2[i]) > tol)
                return false;
        }
        return true;
    }
}
