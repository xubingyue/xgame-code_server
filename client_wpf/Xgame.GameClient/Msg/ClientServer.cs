﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xgame.GamePart.Msg;

namespace Xgame.GameClient.Msg
{
    /// <summary>
    /// 客户端服务器
    /// </summary>
    public sealed partial class ClientServer
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        public static readonly ClientServer OBJ = new ClientServer();

        /** 套接字对象 */
        private Socket _clientSocket = null;
        /** GC 消息队列 */
        private Queue<BaseGCMsg> _gcMsgQ = new Queue<BaseGCMsg>();

        /// <summary>
        /// 类默认构造器
        /// </summary>
        private ClientServer()
        {
        }

        /// <summary>
        /// 获取或设置游戏服 IP 地址
        /// </summary>
        public string GameServerIpAddr
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置游戏服端口地址
        /// </summary>
        public int GameServerPort
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已经连接到游戏服务器
        /// </summary>
        public bool Connected
        {
            get
            {
                return this._clientSocket != null && this._clientSocket.Connected;
            }
        }

        /// <summary>
        /// 连接到游戏服
        /// </summary>
        public void ConnectToGameServer()
        {
            if (this._clientSocket != null)
            {
                // 如果已经连接过服务器, 
                // 则直接退出!
                return;
            }

            // 记录日志信息
            MsgLog.LOG.InfoFormat(
                "准备连接游戏服, IP = {0}, 端口 = {1}", 
                this.GameServerIpAddr, 
                this.GameServerPort
            );

            // 设定游戏服 IP 地址数组
            IPAddress[] ipAddrArr = Dns.GetHostAddresses(this.GameServerIpAddr);
            // 创建 Socket 对象并连接到服务器
            this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._clientSocket.Connect(new IPEndPoint(ipAddrArr[0], this.GameServerPort));

            // 启动线程开始接收消息
            Thread t0 = new Thread(new ThreadStart(this.StartRecevie));
            t0.Start();
        }

        /// <summary>
        /// 开始接收消息
        /// </summary>
        private void StartRecevie()
        {
            // 记录日志信息
            MsgLog.LOG.Info("开启消息接收线程");
            // 字节数组
            byte[] byteArr = new byte[2048];

            while (this._clientSocket != null && this._clientSocket.Connected)
            {
                try
                {
                    // 接收字节数组
                    int count = this._clientSocket.Receive(byteArr);

                    if (count <= 0)
                    {
                        // 如果接收数量 <= 0, 
                        // 则直接跳过!
                        continue;
                    }

                    // 创建二进制读入流
                    BinaryReader br = new BinaryReader(new BufferedStream(new MemoryStream()));
                    // 写入字节数组
                    br.BaseStream.Write(byteArr, 0, byteArr.Length);
                    br.BaseStream.Position = 0;

                    // 先读掉消息长度
                    br.ReadInt16();
                    // 再获取消息序列化 Id
                    short msgSerialUId = br.ReadInt16();

                    // 创建 GC 消息对象
                    BaseGCMsg gcMSG = MsgServ.OBJ.NewMsgObj<BaseGCMsg>(msgSerialUId);

                    if (gcMSG == null)
                    {
                        // 如果消息对象为空, 
                        // 则直接跳过!
                        MsgLog.LOG.ErrorFormat(
                            "接收到 MsgSerialUId = {0} 消息, 但消息对象为空", 
                            msgSerialUId
                        );
                        continue;
                    }

                    // 记录日志信息
                    MsgLog.LOG.InfoFormat(
                        "接收到 {0} 消息", gcMSG.GetType().Name
                    );

                    br.BaseStream.Position = 0;
                    // 令 GC 消息读取二进制数据
                    gcMSG.ReadFrom(br);

                    // 令 GC 消息入队
                    this._gcMsgQ.Enqueue(gcMSG);
                    // 处理 GC 消息
                    this.ProcessGCMsg();
                }
                catch (Exception ex)
                {
                    // 记录错误日志
                    MsgLog.LOG.Error(ex);
                }
            }
        }

        /// <summary>
        /// 执行 GC 消息
        /// </summary>
        public void ProcessGCMsg()
        {
            for (int i = 0; i < 8 && this._gcMsgQ.Count > 0; i++)
            {
                // 获取 GC 消息
                BaseGCMsg gcMSG = this._gcMsgQ.Dequeue();

                if (gcMSG == null)
                {
                    // 如果 GC 消息为空, 
                    // 则直接退出!
                    return;
                }

                // 获取消息类型
                Type msgT = gcMSG.GetType();
                // 获取内置字典
                IDictionary<Delegate, WrappedGCMsgH> innerDict = this._handlerDict.ContainsKey(msgT) ? this._handlerDict[msgT] : null;

                if (innerDict == null)
                {
                    // 如果内置字典为空, 
                    // 则直接跳过!
                    continue;
                }

                // 关键字列表
                List<Delegate> keyFuncList = null;

                foreach (Delegate keyFunc in innerDict.Keys)
                {
                    // 获取消息处理器
                    WrappedGCMsgH wrappedH = innerDict[keyFunc];

                    if (wrappedH == null
                     || wrappedH._hRef == null)
                    {
                        // 如果消息处理器为空, 
                        // 则直接跳过!
                        continue;
                    }

                    // 记录调试信息
                    MsgLog.LOG.DebugFormat(
                        "执行消息 {0}", gcMSG.GetType().Name
                    );

                    // 处理 GC 消息
                    wrappedH._hRef.DynamicInvoke(gcMSG);

                    if (!wrappedH._reusable)
                    {
                        if (keyFuncList == null)
                        {
                            keyFuncList = new List<Delegate>();
                        }

                        keyFuncList.Add(keyFunc);
                    }
                }

                foreach (Delegate keyFunc in keyFuncList)
                {
                    // 从字典中移除关键字
                    innerDict.Remove(keyFunc);
                }

                if (innerDict.Count <= 0)
                {
                    // 如果内置字典为空, 
                    // 则从根字典中移除消息类型
                    this._handlerDict.Remove(msgT);
                }
            }
        }

        /// <summary>
        /// 发送 CG 消息给服务器端
        /// </summary>
        /// <param name="cgMSG"></param>
        public void SendCGMsg(BaseCGMsg cgMSG)
        {
            if (cgMSG == null)
            {
                // 如果参数对象为空, 
                // 则直接退出!
                return;
            }

            // 记录日志信息
            MsgLog.LOG.InfoFormat(
                "准备发送 {0} 消息", cgMSG.GetType().Name
            );

            // 创建二进制输出流
            BinaryWriter bw = new BinaryWriter(new BufferedStream(new MemoryStream()));
            // 将消息写出到二进制流
            cgMSG.WriteTo(bw);
            // 将位置归零
            bw.BaseStream.Position = 0;

            // 获取消息长度
            int msgLen = (int)bw.BaseStream.Length;
            // 创建并写入字节数组
            byte[] byteArr = new byte[msgLen];
            bw.BaseStream.Read(byteArr, 0, byteArr.Length);

            // 发送字节数组
            this._clientSocket.Send(byteArr);
        }

        /// <summary>
        /// 关闭服务器连接
        /// </summary>
        public void Shutdown()
        {
            if (this._clientSocket != null 
             && this._clientSocket.Connected)
            {
                // 记录日志信息
                MsgLog.LOG.Info("断开与游戏服的连接");
                // 断开与游戏服的连接
                this._clientSocket.Close();
            }
        }
    }
}
