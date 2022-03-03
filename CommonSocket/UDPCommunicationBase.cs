using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

//  XMLドキュメントの警告は無視
#pragma warning disable 1591

namespace SPCommonSocket
{
    public class UDPCommunicationBase
    {
        #region イベント
        
        //  データ受信イベント(string)
        public Action<string> evReceiveUDPStringData;
        //  データ受信イベント(バイナリ)
        public Action<byte[]> evReceiveUDPBinaryData;

        #endregion

        #region メンバー変数

        public const int RECEIVE = 0;           //  受信専用UDP
        public const int SEND = 1;              //  送信専用UDP
        public const int SEND_RECEIVE = 2;      //  送受信UDP

        public const int DATA_TYPE_STRING = 0;  //  送受信データ(文字列)
        public const int DATA_TYPE_BINARY = 1;  //  送受信データ(バイナリ)

        private int m_iCommunicationType = -1;  //  通信タイプ(受信only、送信only、送受信)
        private int m_iDataType = -1;           //  送受信データタイプ(文字列、バイナリ)

        public UdpClient m_UdpReceive;          //  受信UDPクライアント
        public UdpClient m_UdpSend;             //  送信UDPクライアント

        private IPEndPoint m_SendEndPoint;      //  送信エンドポイント
        private IPEndPoint m_ReceiveEndPoint;   //  受信エンドポイント

        private object m_lock = new object();   //  排他処理用lockオブジェクト

        Form m_formParent;                      //  アプリフォーム

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="niCommunicationType">通信タイプ</param>
        /// <param name="niDataType">送受信データタイプ</param>
        /// <param name="nformParent">親フォーム</param>
        public UDPCommunicationBase(int niCommunicationType, int niDataType, Form nformParent)
        {
            //  通信タイプを設定する
            m_iCommunicationType = niCommunicationType;
            //  送受信データタイプを設定する
            m_iDataType = niDataType;
            //  エンドポイントを初期化しておく
            m_SendEndPoint = m_ReceiveEndPoint = null;
            //  UDPオブジェクトを初期化しておく
            m_UdpReceive = m_UdpSend = null;
            //  このdllを呼んでいるフォームを覚えておく
            m_formParent = nformParent;
        }

        /// <summary>
        /// UDPオープン(受信専用タイプ)
        /// </summary>
        /// <param name="niReceivePort">受信用ポート番号</param>
        /// <returns></returns>
        public int openUDP(int niReceivePort)
        {
            //  受信専用タイプでオブジェクトを作ってなければエラー
            if (m_iCommunicationType != RECEIVE)
            {
                return -1;
            }

            try
            {
                //  受信エンドポイント作成
                m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, niReceivePort);
                //  受信UDPオブジェクトにバインドさせる
                m_UdpReceive = new UdpClient(m_ReceiveEndPoint);
                //  受信イベントハンドラを設定する
                m_UdpReceive.BeginReceive(receiveHandler, this);
            }
            catch (System.Exception)
            {
                //  オープンエラー
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDPオープン(送信専用タイプ)
        /// </summary>
        /// <param name="nstrIPAddress">送信先のIPアドレス</param>
        /// <param name="niSendPort">送信先のポート番号</param>
        /// <returns></returns>
        public int openUDP(string nstrIPAddress,int niSendPort)
        {
            IPAddress ip_address;

            //  送信専用タイプでオブジェクトを作ってなければエラー
            if (m_iCommunicationType != SEND)
            {
                return -1;
            }

            //  IPアドレスが間違っていたらエラー
            if (IPAddress.TryParse(nstrIPAddress, out ip_address) == false)
            {
                return -2;
            }

            try
            {
                //  送信エンドポイント作成
                m_SendEndPoint = new IPEndPoint(ip_address, niSendPort);
                //  送信UDPオブジェクト作成
                m_UdpSend = new UdpClient();
                //  送信先を設定
                m_UdpSend.Connect(m_SendEndPoint);
            }
            catch (System.Exception)
            {
                //  オープンエラー
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDPオープン(送受信タイプ)
        /// </summary>
        /// <param name="nstrIPAddress">送信先のIPアドレス</param>
        /// <param name="niSendPort">送信先のポート番号</param>
        /// <param name="niReceivePort">受信用ポート番号</param>
        /// <returns></returns>
        public int openUDP(string nstrIPAddress, int niSendPort, int niReceivePort)
        {
            IPAddress ip_address;

            //  送受信タイプでオブジェクトを作ってなければエラー
            if (m_iCommunicationType != SEND_RECEIVE)
            {
                return -1;
            }
            //  IPアドレスが間違っていたらエラー
            if (IPAddress.TryParse(nstrIPAddress, out ip_address) == false)
            {
                return -2;
            }

            try
            {
                //  受信エンドポイント作成
                m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, niReceivePort);
                //  受信UDPオブジェクトにバインドさせる
                m_UdpReceive = new UdpClient(m_ReceiveEndPoint);
                //  受信イベントハンドラを設定する
                m_UdpReceive.BeginReceive(receiveHandler, this);

                //  送信エンドポイント作成
                m_SendEndPoint = new IPEndPoint(ip_address, niSendPort);
                //  送信UDPオブジェクト作成
                m_UdpSend = new UdpClient();
                //  送信先を設定
                m_UdpSend.Connect(m_SendEndPoint);
            }
            catch (System.Exception)
            {
                //  オープンエラー
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDP通信を閉じる
        /// </summary>
        /// <returns></returns>
        public int closeUDP()
        {
            //  UDPオブジェクト解放したあとにイベントハンドラに飛ぶことあるのでlockしておく
            lock(m_lock)
            {
                //UPDクローズ
                if (m_UdpSend != null)
                {
                    m_UdpSend.Close();
                    m_UdpSend = null;
                }
                if (m_UdpReceive != null)
                {

                    m_UdpReceive.Close();
                    m_UdpReceive = null;
                }
            }

            return 0;
        }

        /// <summary>
        /// 送信(文字列)
        /// </summary>
        /// <param name="nstrSend">送信データ(文字列)</param>
        /// <returns></returns>
        public int sendData(string nstrSend)
        {
            //  送信オブジェクトがなければエラー
            if (m_UdpSend == null)
            {
                return -1;
            }
            //  送受信データタイプが文字列でなければエラー
            if(m_iDataType != DATA_TYPE_STRING)
            {
                return -2;
            }

            //文字列をバイト列に変換
            byte[] bt_send_data = System.Text.Encoding.UTF8.GetBytes(nstrSend);
            //送信
            m_UdpSend.Send(bt_send_data, bt_send_data.Length);

            return 0;
        }

        /// <summary>
        /// 送信(バイナリ)
        /// </summary>
        /// <param name="nbtSend">送信データ(バイナリ)</param>
        /// <returns></returns>
        public int sendData(byte[] nbtSend)
        {
            //  送信オブジェクトがなければエラー
            if (m_UdpSend == null)
            {
                return -1;
            }
            //  送受信データタイプがバイナリでなければエラー
            if (m_iDataType != DATA_TYPE_BINARY)
            {
                return -2;
            }

            //送信
            m_UdpSend.Send(nbtSend, nbtSend.Length);

            return 0;
        }


        /// <summary>
        /// 受信(文字列)
        /// </summary>
        /// <param name="nstrReceive">受信データ(文字列)</param>
        /// <returns></returns>
        public void ReceiveStringData(string nstrReceive)
        {
            //  受信イベント発行
            evReceiveUDPStringData?.Invoke(nstrReceive);
        }

        /// <summary>
        /// 受信(バイナリ)
        /// </summary>
        /// <param name="nbtReceive">受信データ(バイナリ)</param>
        public void ReceiveBinaryData(byte[] nbtReceive)
        {
            //  受信イベント発行
            evReceiveUDPBinaryData?.Invoke(nbtReceive);
        }


        /// <summary>
        /// 受信イベントハンドラ
        /// </summary>
        /// <param name="ar"></param>
        private void receiveHandler(IAsyncResult ar)
        {
            IPEndPoint end_point = null;
            byte[] bt_receive;
            string str_receive;

            lock(m_lock)
            {
                //  自分自身のクラスオブジェクトを取得
                UDPCommunicationBase myself_object = (UDPCommunicationBase)ar.AsyncState;

                //  受信オブジェクトが空なら何もしない
                if (myself_object.m_UdpReceive == null)
                {
                    return;
                }

                try
                {
                    //  データ受信
                    bt_receive = myself_object.m_UdpReceive.EndReceive(ar, ref end_point);
                }
                //  受信失敗
                catch (System.Net.Sockets.SocketException)
                {
                    //  再度受信受け付け開始
                    myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);
                    return;
                }
                catch (System.Exception)
                {
                    //  再度受信受け付け開始
                    myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);
                    return;
                }

                //  再度受信受け付け開始
                myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);

                //  送受信データタイプが文字列
                if(m_iDataType == DATA_TYPE_STRING)
                {
                    //受信データを文字変換
                    str_receive = System.Text.Encoding.UTF8.GetString(bt_receive);
                    //  受信したことをアプリに通知する
                    m_formParent.Invoke(new Action(() => ReceiveStringData(str_receive)));
                }
                //  送受信データタイプがバイナリ
                else
                {
                    //  受信したことをアプリに通知する
                    m_formParent.Invoke(new Action(() => ReceiveBinaryData(bt_receive)));
                }
 
            }
        }

    }
}
