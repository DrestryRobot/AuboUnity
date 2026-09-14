using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpServer : MonoBehaviour
{
    private TcpListener mainListener; // 主服务器监听器
    private TcpListener instructionListener; // 指令服务器监听器
    private Thread mainServerThread; // 主服务器线程
    private Thread instructionServerThread; // 指令服务器线程
    private bool isRunning = true; // 服务器运行状态标志

    public int mainPort = 12347; // 主数据端口
    public int instructionPort = 12349; // 指令端口
    public JointMove jointMoveScript; // JointMove脚本引用
    public GunMove gunMoveScript; // GunMove脚本引用

    private float[] jointAngles = new float[6]; // 存储关节角度
    private float[] endEffectorPose = new float[6]; // 存储末端位姿
    private float gunAngle; // 存储枪角度
    private string commandValue1; // 存储指令值
    private string commandValue2; // 存储指令值
    private string commandValue3; // 存储指令值

    private object lockObject = new object(); // 用于线程同步的锁对象
    private object lockObject1 = new object(); // 用于线程同步的锁对象

    void Start()
    {
        // 检查是否分配了JointMove脚本
        if (jointMoveScript == null)
        {
            return;
        }

        // 启动主服务器线程
        mainServerThread = new Thread(StartMainServer);
        mainServerThread.IsBackground = true;
        mainServerThread.Start();

        // 启动指令服务器线程
        instructionServerThread = new Thread(StartInstructionServer);
        instructionServerThread.IsBackground = true;
        instructionServerThread.Start();
    }

    void StartMainServer()
    {
        try
        {
            // 初始化主服务器监听器
            mainListener = new TcpListener(IPAddress.Any, mainPort);
            mainListener.Start();

            while (isRunning)
            {
                // 检查是否有挂起的客户端连接
                if (mainListener.Pending())
                {
                    TcpClient client = mainListener.AcceptTcpClient();

                    // 为每个客户端启动单独的线程处理
                    Thread clientThread = new Thread(() => HandleMainClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }

                Thread.Sleep(50); // 降低CPU占用率
            }
        }
        catch (Exception)
        {
            // 捕获并忽略异常
        }
        finally
        {
            mainListener?.Stop(); // 停止监听器
        }
    }

    void StartInstructionServer()
    {
        try
        {
            // 初始化指令服务器监听器
            instructionListener = new TcpListener(IPAddress.Any, instructionPort);
            instructionListener.Start();

            while (isRunning)
            {
                // 检查是否有挂起的客户端连接
                if (instructionListener.Pending())
                {
                    TcpClient client = instructionListener.AcceptTcpClient();

                    // 为每个客户端启动单独的线程处理
                    Thread clientThread = new Thread(() => HandleInstructionClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }

                Thread.Sleep(100); // 降低CPU占用率
            }
        }
        catch (Exception)
        {
            // 捕获并忽略异常
        }
        finally
        {
            instructionListener?.Stop(); // 停止监听器
        }
    }

    void HandleMainClient(TcpClient client)
    {
        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                // 持续读取客户端数据
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] dataSegments = receivedData.Split(',');

                    // 数据格式校验
                    if (dataSegments.Length >= 13)
                    {
                        lock (lockObject)
                        {
                            // 解析关节角度
                            for (int i = 0; i < 6; i++)
                            {
                                if (!float.TryParse(dataSegments[i], out jointAngles[i]))
                                {
                                    return;
                                }
                            }

                            // 解析末端位姿
                            for (int i = 6; i < 12; i++)
                            {
                                if (!float.TryParse(dataSegments[i], out endEffectorPose[i - 6]))
                                {
                                    return;
                                }
                            }

                            // 解析枪角度
                            if (!float.TryParse(dataSegments[12], out gunAngle))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // 捕获并忽略异常
        }
        finally
        {
            client.Close(); // 关闭客户端连接
        }
    }

    //void HandleInstructionClient(TcpClient client)
    //{
    //    try
    //    {
    //        using (NetworkStream stream = client.GetStream())
    //        {
    //            byte[] buffer = new byte[1024];
    //            int bytesRead;

    //            // 持续读取客户端指令
    //            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
    //            {
    //                string receivedInstruction = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    //                lock (lockObject)
    //                {
    //                    commandValue = receivedInstruction.Trim(); // 更新指令值
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        // 捕获并忽略异常
    //    }
    //    finally
    //    {
    //        client.Close(); // 关闭客户端连接
    //    }
    //}

    void HandleInstructionClient(TcpClient client)
    {
        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                // 持续读取客户端指令
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string receivedInstruction = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    lock (lockObject)
                    {
                        // 分隔指令并存储到数组中
                        string[] commandValues = receivedInstruction.Split(';');

                        // 如果需要具体处理，可以针对数组内容分别操作
                        if (commandValues.Length >= 3)
                        {
                            commandValue1 = commandValues[0].Trim();
                            commandValue2 = commandValues[1].Trim();
                            commandValue3 = commandValues[2].Trim();
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // 捕获并忽略异常
        }
        finally
        {
            client.Close(); // 关闭客户端连接
        }
    }


    void Update()
    {


        lock (lockObject)
        {
            // 更新数据到JointMove和GunMove脚本
            jointMoveScript.UpdateJointAngles(jointAngles);
            jointMoveScript.UpdateEndEffectorPose(endEffectorPose);
            jointMoveScript.UpdateGunAngle(gunAngle);
            gunMoveScript.UpdateGunAngle(gunAngle);
            jointMoveScript.UpdateCommand(commandValue1);
            jointMoveScript.UpdatePici(commandValue2);
            jointMoveScript.UpdateBianhao(commandValue3);
        }
    }

    public float[] GetJointAngles()
    {
        lock (lockObject)
        {
            // 返回关节角度的副本以防止线程冲突
            return (float[])jointAngles.Clone();
        }
    }

    public float[] GetEndEffectorPose()
    {
        lock (lockObject1)
        {
            // 返回关节角度的副本以防止线程冲突
            return (float[])endEffectorPose.Clone();
        }
    }

    void OnApplicationQuit()
    {
        // 停止服务器线程和监听器
        isRunning = false;
        mainListener?.Stop();
        instructionListener?.Stop();
        mainServerThread?.Join();
        instructionServerThread?.Join();
    }
}
