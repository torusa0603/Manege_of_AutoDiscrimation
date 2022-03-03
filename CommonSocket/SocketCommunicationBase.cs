using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Net;

//  XMLドキュメントの警告は無視
#pragma warning disable 1591

namespace SPCommonSocket
{
    //  イベントハンドラ用デリゲート
    public delegate void EventHandlerString(string str);
    public delegate void EventHandlerVoid();

    //  データ送信用のデリゲート
    delegate void ReceiveDataDelegate(string text);
    //  ソケットクローズ用のデリゲート
    delegate int SocketCloseDelegate();

    public class CSocketCommunicationBase
    {
        #region イベント定義
        //  データ受信イベント
        public event EventHandlerString evReceiveData;
        public event EventHandlerString evReceiveDataForSCAM;
        //  ソケットクローズイベント
        public event EventHandlerVoid evSocketClose;
        //  サーバーにクライアントから接続があったら発生するイベント
        public event EventHandlerVoid evAcceptClient;

        #endregion

        #region メンバー変数
        private TcpListener m_TcpServerListener = null;     //  TCPサーバーリスナー
        private TcpClient m_TCPServer = null;             //  サーバー
        private Thread m_ThreadServer = null;               //  サーバーのスレッド
        private int m_iServerPortNo;                        //  サーバーが使用するポート番号

        protected TcpClient m_TcpClient = null;             //  クライアント
        protected Thread m_ThreadClient = null;             //  クライアントのスレッド

        protected int m_iSokType;                           //  ソケットタイプ
        protected bool m_bActiveSocket;                     //  ソケットが有効状態か否か

        public const int CLIENT = 0;                        //  クライアント
        public const int SERVER = 1;                        //  サーバー
        public const int SCAM_CLIENT = 2;                   //  SCAM通信のためのクライアント
        private const int MAX_DATA_SIZE = 2048;             //  最大送信データバイト

        private bool m_OutputDataLog;                       //  通信ログを出力するか否か
        private int m_iInstanceIndex;                       //  オブジェクトインデックス


        public Form m_frmParent;    //  このdllを呼んでいるフォーム

        static private int m_siTotalInstanceIndex = 0;               //  今まで作成したオブジェクト数
        static object lockObject = new object();                    //  インスタンス間の排他制御に使用

        private bool m_bAddPeriodAfterIPAddress = false;    //  IPアドレスの末尾にピリオド「.」をつけるか否か

        #endregion

        #region 文字コード

        //  本ライブラリが対応する文字コード
        public enum eEncode
        {
            Shift_JIS = 1,
            UTF_8
        }
        private Dictionary<eEncode, string> m_dictEncode = new Dictionary<eEncode, string>()
        {
            {eEncode.Shift_JIS,"shift-jis" },
            {eEncode.UTF_8,"utf-8" }
        };
        //  指定する文字コード名
        private string m_strEncoding = "shift-jis";

        #endregion

        #region コンストラクタ
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// 
        /// <param name="niSokType">
        /// 
        /// <para>作成しようとしているソケットタイプ</para>
        /// <para>クライアント：CSocketCommunicationBase.CLIENT</para>
        /// <para>サーバー:CSocketCommunicationBase.SERVER</para>
        /// 
        /// </param>
        /// <param name="nformParent">親フォーム</param>
        /// 
        /// ----------------------------------------------------------------------------------------
        public CSocketCommunicationBase( int niSokType, Form nformParent )
        {
            //  ソケットタイプを設定
            m_iSokType = niSokType;
            m_bActiveSocket = false;
            m_OutputDataLog = false;
            m_frmParent = nformParent;
            m_iInstanceIndex = m_siTotalInstanceIndex;
            m_siTotalInstanceIndex++;
        }
        #endregion

        #region コンストラクタ2
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ2
        /// </summary>
        /// 
        /// <param name="niSokType">
        /// 
        /// <para>作成しようとしているソケットタイプ</para>
        /// <para>クライアント：CSocketCommunicationBase.CLIENT</para>
        /// <para>サーバー:CSocketCommunicationBase.SERVER</para>
        /// 
        /// </param>
        /// 
        /// ----------------------------------------------------------------------------------------
        public CSocketCommunicationBase(int niSokType)
        {
            //  ソケットタイプを設定
            m_iSokType = niSokType;
            m_bActiveSocket = false;
            m_OutputDataLog = false;
            m_frmParent = null;
            m_iInstanceIndex = m_siTotalInstanceIndex;
            m_siTotalInstanceIndex++;
        }
        #endregion

        #region ソケットオープン(クライアント) IPアドレス指定
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  ソケットオープン(クライアント)
        /// </summary>
        /// 
        /// <param name="nstrIPHost">IPアドレスもしくはホスト名</param>
        /// <param name="niPort">ポート番号</param>
        /// 
        /// <returns>
        /// <para>0:正常</para>
        /// <para>-1:サーバーとして起動したのにクライアントのソケットをオープンしようとした</para>
        /// <para> -2:既にソケットをオープンしている</para>
        /// <para>-3:ソケットオープンエラー</para>
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        virtual public int OpenSocket(string nstrIPHost, int niPort )
        {
            //  サーバーとして起動したのにクライアントのソケットをオープンしようとしたらエラー
            if (m_iSokType == SERVER)
            {
                return -1;
            }
            //  既にソケットをオープンしていればエラー
            else if (m_bActiveSocket == true)
            {
                return -2;
            }
            try
            {
                //  IPアドレス指定の場合
                if (IsIPAddress(nstrIPHost) == true)
                {
                    //  ピリオドを付加する場合
                    if( m_bAddPeriodAfterIPAddress == true )
                    {
                        //  文字列末尾に「.」がなければ付ける
                        if (nstrIPHost.EndsWith(".") == false)
                        {
                            nstrIPHost = nstrIPHost + ".";
                        }
                    }
                }

                //クライアントのソケットを用意
                m_TcpClient = new TcpClient(nstrIPHost, niPort);

                //サーバからのデータを受信するループをスレッドで処理
                m_ThreadClient = new Thread(new ThreadStart(this.ClientListenThread));
                //  ソケットが有効になった
                m_bActiveSocket = true;
                //  受信スレッド開始
                m_ThreadClient.Start();
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -3;
            }

            return 0;
        }
        #endregion

        #region ソケットオープン(クライアント and サーバー)
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>ソケットオープン(クライアント and サーバー)</para>
        /// <para>クライアントの場合は、ホスト名は自分自身のPCのホスト名を使用する</para>
        /// </summary>
        /// 
        /// <param name="niPort">ポート番号</param>
        /// 
        /// <returns>
        /// <para>0:正常</para>
        /// <para>-1:サーバー機能はない</para>
        /// <para> -2:既にソケットをオープンしている</para>
        /// <para>-3:ソケットオープンエラー</para>
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        virtual public int OpenSocket(int niPort)
        {
            //  既にソケットをオープンしていればエラー
            if (m_bActiveSocket == true)
            {
                return -2;
            }


            //  サーバーとしてオープン
            if (m_iSokType == SERVER)
            {
                try
                {
                    //クライアントからの接続を待機するサーバクラス設定
                    if (m_TcpServerListener == null)
                    {
                        m_TcpServerListener = new TcpListener(IPAddress.Any, niPort);
                    }
                    m_iServerPortNo = niPort;
                    //  ソケットが有効になった
                    m_bActiveSocket = true;
                    //サーバの開始　クライアントから接続されるまで待機
                    m_TcpServerListener.Start();
                    //  スレッド作成
                    m_ThreadServer = new Thread(new ThreadStart(this.ServerListenThread));
                    //  スレッド開始
                    m_ThreadServer.Start();
                }
                catch (Exception ex)
                {
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    return -3;
                }
            }
            //  クライアントとしてオープン
            else
            {
                try
                {
                    //クライアントのソケットを用意
                    m_TcpClient = new TcpClient(System.Net.Dns.GetHostName(), niPort);

                    //サーバからのデータを受信するループをスレッドで処理
                    m_ThreadClient = new Thread(new ThreadStart(this.ClientListenThread));
                    //  ソケットが有効になった
                    m_bActiveSocket = true;
                    //  受信スレッド開始
                    m_ThreadClient.Start();
                }
                catch (Exception ex)
                {
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    return -3;
                }
            }

            return 0;
        }
        #endregion

        #region サーバー用スレッド
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// サーバー用スレッド
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        protected void ServerListenThread()
        {
            //  ソケットが有効になってなれけば終了
            if (m_bActiveSocket == false)
            {
                return;
            }

            NetworkStream stream;

            try
            {
                //クライアントの要求があったら、接続を確立する
                //クライアントの要求が有るまでここで待機する 
                m_TCPServer = m_TcpServerListener.AcceptTcpClient();
                //クライアントとの間の通信に使用するストリームを取得
                stream = m_TCPServer.GetStream();
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                //  ソケットクローズ
                CloseSocket();

                return;
            }


            //  クライアントとの接続がされたイベントを発生
            AcceptClientEvent();

            Byte[] bytes = new Byte[4096];

            while (true)
            {
                try
                {
                    int intCount = stream.Read(bytes, 0, bytes.Length);
                    if (intCount != 0)
                    {
                        //受信部分だけ切り出す
                        Byte[] getByte = new byte[intCount];
                        for (int i = 0; i < intCount; i++)
                            getByte[i] = bytes[i];

                        string str;
                        //バイト配列を文字列に変換
                        str = System.Text.Encoding.GetEncoding(m_strEncoding).GetString(getByte);
                        //受信イベント発生
                        if (m_frmParent != null)
                        {
                            m_frmParent.BeginInvoke(new ReceiveDataDelegate(ReceiveData), new object[] { str });
                        }
                        else
                        {
                            ReceiveData(str);
                        }
                    }
                    //  クライアントが接続を切った
                    else
                    {
                        //ループを抜ける
                        stream.Close();
                        //  サーバーを再開する。
                        if (m_frmParent != null)
                        {
                            m_frmParent.BeginInvoke(new SocketCloseDelegate(RestartServer));
                        }
                        else
                        {
                            RestartServer();
                        }


                        return;
                    }
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                    //  スレッドを強制切断するとこのエラーが発生する
                    //  それを回避するためのエラー処理
                    //  切断されたのでループを抜ける
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    return;
                }
                catch (Exception ex)
                {
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    //  なんらかの原因で、スレッドを終了せずにアプリケーションを終了してしまった場合
                    //  親フォームは既に終了しているのでBeginInvokeを使用せずに直接スレッド/ソケットを
                    //  終了する
                    if (m_frmParent != null)
                    {
                        if (m_frmParent.Visible == true)
                        {
                            m_frmParent.BeginInvoke(new SocketCloseDelegate(CloseSocket));
                        }
                        else
                        {
                            CloseSocket();
                        }
                    }
                    else
                    {
                        CloseSocket();
                    }
                    return;
                }
            }

        }
        #endregion


        #region クライアントデータ受信スレッド
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// クライアントデータ受信スレッド
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        protected void ClientListenThread()
        {
            //  ソケットが有効になってなれけば終了
            if (m_bActiveSocket == false)
            {
                return;
            }

            NetworkStream stream = m_TcpClient.GetStream();

            Byte[] bytes = new Byte[4096];

            while (true)
            {
                try
                {
                    int intCount = stream.Read(bytes, 0, bytes.Length);
                    if (intCount != 0)
                    {
                        //受信部分だけ切り出す
                        Byte[] getByte = new byte[intCount];
                        for (int i = 0; i < intCount; i++)
                            getByte[i] = bytes[i];

                        string str;
                        //バイト配列を文字列に変換
                        str = System.Text.Encoding.GetEncoding(m_strEncoding).GetString(getByte);
                        //受信イベント発生
                        if (m_frmParent != null)
                        {  
                            m_frmParent.BeginInvoke(new ReceiveDataDelegate(ReceiveData), new object[] { str });
                        }
                        else
                        {
                            ReceiveData(str);
                        }
                    }
                    else
                    {
                        //ループを抜ける
                        stream.Close();
              //          CloseSocket();
                        if (m_frmParent != null)
                        {
                            m_frmParent.BeginInvoke(new SocketCloseDelegate(CloseSocket));
                        }
                        else
                        {
                            CloseSocket();
                        }
                        

                        return;
                    }
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                    //  スレッドを強制切断するとこのエラーが発生する
                    //  それを回避するためのエラー処理
                    //  切断されたのでループを抜ける
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    return;
                }
                catch (Exception ex)
                {
                    string str_err;
                    str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                    OutputErrorLog(str_err);
                    //  なんらかの原因で、スレッドを終了せずにアプリケーションを終了してしまった場合
                    //  親フォームは既に終了しているのでBeginInvokeを使用せずに直接スレッド/ソケットを
                    //  終了する
                    if (m_frmParent != null)
                    {
                        if (m_frmParent.Visible == true)
                        {
                            m_frmParent.BeginInvoke(new SocketCloseDelegate(CloseSocket));
                        }
                        else
                        {
                            CloseSocket();
                        }
                    }
                    else
                    {
                        CloseSocket();
                    }
                    return;
                }
            }

        }
        #endregion

        #region ソケット終了
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ソケット終了
        /// </summary>
        /// 
        ///<returns>
        ///
        ///<para>0:正常</para>
        ///<para>-1:ソケットが有効でない</para>
        ///<para>-2:ソケットクローズエラー</para>
        ///
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int CloseSocket()
        {
            //  ソケットが有効になってなれけば終了
            if (m_bActiveSocket == false)
            {
                return -1;
            }

            try
            {
                //  サーバーである
                if(m_iSokType == SERVER)
                {
                    m_bActiveSocket = false;

                    //サーバーのインスタンスが有って、接続されていたら
                    if (m_TCPServer != null && m_TCPServer.Connected)
                    {
                        m_TCPServer.Close();
                        m_TCPServer = null;
                    }

                    CloseSocketEvent();

                    //スレッドは必ず終了させること
                    if (m_ThreadServer != null)
                    {
                        m_TcpServerListener.Stop();
                        m_TcpServerListener = null;
                        m_ThreadServer.Abort();
                        m_ThreadServer = null;
                    }

                }
                //  クライアントである
                else
                {
                    m_bActiveSocket = false;

                    //クライアントのインスタンスが有って、接続されていたら
                    if (m_TcpClient != null && m_TcpClient.Connected)
                    {
                        m_TcpClient.Close();
                        m_TcpClient = null;
                    }

                    CloseSocketEvent();

                    //スレッドは必ず終了させること
                    if (m_ThreadClient != null)
                    {
                        m_ThreadClient.Abort();
                        m_ThreadClient = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -2;
            }
            return 0;
        }
        #endregion

        #region ソケットが有効かどうか調べる
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ソケットが有効かどうか調べる
        /// </summary>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        public bool getSocketActive()
        {
            return m_bActiveSocket;
        }
        #endregion

        #region データ送信
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// データ送信
        /// </summary>
        /// 
        /// <param name="nstrSendData">送信文字列</param>
        /// 
        /// <returns>
        /// 
        /// <para>0:正常</para>
        /// <para>-1:データ送信エラー</para>
        /// <para>-2:データ数オーバー</para>
        /// <para>-3:ソケットがオープンしていない(有効でない)</para>
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Send( string nstrSendData )
        {

            //sift-jisに変換して送る
            Byte[] data = Encoding.GetEncoding(m_strEncoding).GetBytes(nstrSendData);

            //  ソケットが有効でなければエラー
            if (m_bActiveSocket == false)
            {
                return -3;
            }

            //  バイト数がオーバーしていればエラー
            if (data.Count() > MAX_DATA_SIZE)
            {
                return -2;
            }

            //送信streamを作成
            NetworkStream stream = null;

            //  サーバーである
            if(m_iSokType == SERVER)
            {
                stream = m_TCPServer.GetStream();
            }
            //  クライアントである
            else
            {
                stream = m_TcpClient.GetStream();
            }

            try
            {
                if (m_OutputDataLog == true)
                {
                    OutputDataLog(nstrSendData, 1);
                }
                //Streamを使って送信
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -1;
            }

            return 0;
        }
        #endregion

        #region データ受信イベント発生
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// データ受信イベントを発生させる
        /// </summary>
        /// 
        /// <param name="strReceiveData">受信データ文字列</param>
        /// ----------------------------------------------------------------------------------------
        public void ReceiveData(string strReceiveData)
        {
            try
            {
                if (m_OutputDataLog == true)
                {
                    OutputDataLog(strReceiveData, 0);
                }

                //  SCAM以外の通信
                if (m_iSokType != SCAM_CLIENT)
                {
                    if (evReceiveData != null)    //  登録してない可能性あるのでこれ必須
                    {
                        //  イベント送信
                        evReceiveData(strReceiveData);
                    }
                }
                //  SCAMとのリモート通信
                else
                {
                    if (evReceiveDataForSCAM != null)    //  登録してない可能性あるのでこれ必須
                    {
                        //  イベント送信
                        evReceiveDataForSCAM(strReceiveData);
                    }
                }
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return;
            }
        }
        #endregion

        #region ソケットクローズイベント発生
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ソケットクローズイベント発生
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void CloseSocketEvent()
        {
            try
            {
                if (evSocketClose != null)    //  登録してない可能性あるのでこれ必須
                {
                    //  イベント送信
                    evSocketClose();
                }
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return;
            }
        }
        #endregion

        #region サーバーにクライアントから接続があったイベント発生
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// サーバーにクライアントから接続があったイベント発生evAcceptClient
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void AcceptClientEvent()
        {
            try
            {
                if (evAcceptClient != null)    //  登録してない可能性あるのでこれ必須
                {
                    //  イベント送信
                    evAcceptClient();
                }
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return;
            }
        }

        #endregion

        #region 入力されたのがIPアドレスなのかホスト名なのかを判断する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 入力されたのがIPアドレスなのかホスト名なのかを判断する
        /// </summary>
        /// <param name="nstrIPHost">入力された文字列</param>
        /// <returns>true:IPアドレス  false:ホスト名</returns>
        /// ----------------------------------------------------------------------------------------
        private bool IsIPAddress(string nstrIPHost)
        {
            int i_loop;
            int i_wrk;
            //  IPアドレスかどうかの判断は、「.」で文字列分解し、4つの数字に分解でき、
            //  その数字が0～255であればIPアドレスと判断する

            //  データ分割(半角スペース)
            string[] stArrayData = nstrIPHost.Split('.');
            //  最後の分割文字列が""なら文字列最後の文字は「.」であったということ
            if (stArrayData[stArrayData.Count() - 1] == "")
            {
                //  この場合、それまでの分割文字列を使用
                if (stArrayData.Count() == 5)
                {
                    for (i_loop = 0; i_loop < 4; i_loop++)
                    {
                        //  0～255の値でなければホスト名
                        if (int.TryParse(stArrayData[i_loop], out i_wrk) == true)
                        {
                            if (i_wrk < 0 || i_wrk > 255)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                //  分割数が5でなければホスト名
                else
                {
                    return false;
                }
            }
            else
            {
                //  この場合、それまでの分割文字列を使用
                if (stArrayData.Count() == 4)
                {
                    for (i_loop = 0; i_loop < 4; i_loop++)
                    {
                        //  0～255の値でなければホスト名
                        if (int.TryParse(stArrayData[i_loop], out i_wrk) == true)
                        {
                            if (i_wrk < 0 || i_wrk > 255)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                //  分割数が5でなければホスト名
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region エラーログを出力する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// エラーログを出力する
        /// </summary>
        /// <param name="nstrLogMessage">出力するログ文字列</param>
        /// ----------------------------------------------------------------------------------------
        protected void OutputErrorLog(string nstrLogMessage)
        {
            const string str_directory_path = "log";
            string str_file_path;

            try
            {
                //  インスタンス間の排他制御
                lock (lockObject)
                {
                    //  ファイルパス決定
                    str_file_path = str_directory_path + "\\" + "SPCommonSocket_ERROR_" + DateTime.Today.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".log";

                    //  log出力ディレクトリの存在チェック。なければ作成
                    if (Directory.Exists(str_directory_path) == false)
                    {
                        Directory.CreateDirectory(str_directory_path);
                    }

                    //  ファイルオープン
                    Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                    StreamWriter writer = new StreamWriter(str_file_path, true, sjisEnc);

                    //  ログに日付を付加
                    nstrLogMessage = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "," + m_iInstanceIndex.ToString() + "," + nstrLogMessage;
                    //  書き込み
                    writer.WriteLine(nstrLogMessage);
                    //  クローズ
                    writer.Close();
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        #region 通信ログを出力する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 通信ログを出力する
        /// </summary>
        /// 
        /// <param name="nstrLogMessage">受信/送信文字列</param>
        /// <param name="niSendRecv">0:受信　1:送信</param>
        /// ----------------------------------------------------------------------------------------        
        protected void OutputDataLog(string nstrLogMessage, int niSendRecv )
        {
            const string str_directory_path = "log";
            string str_file_path;
            string str_send_recv;

            try
            {
                //  インスタンス間の排他制御
                lock (lockObject)
                {
                    //  ファイルパス決定
                    str_file_path = str_directory_path + "\\" + "SPCommonSocket_DATA_" + DateTime.Today.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".log";

                    //  log出力ディレクトリの存在チェック。なければ作成
                    if (Directory.Exists(str_directory_path) == false)
                    {
                        Directory.CreateDirectory(str_directory_path);
                    }

                    //  ファイルオープン
                    Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                    StreamWriter writer = new StreamWriter(str_file_path, true, sjisEnc);

                    //  受信
                    if (niSendRecv == 0)
                    {
                        str_send_recv = "Recv>";
                    }
                    //  送信
                    else
                    {
                        str_send_recv = "Send>";
                    }

                    //  ログに日付を付加
                    nstrLogMessage = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "," + m_iInstanceIndex.ToString() + "," + str_send_recv + "," + nstrLogMessage;

                    //  書き込み
                    writer.WriteLine(nstrLogMessage);
                    //  クローズ
                    writer.Close();
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        #endregion

        #region 通信ログを出力するか否かを設定する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 通信ログを出力するか否かを設定する
        /// </summary>
        /// 
        /// <param name="nbEnableOutputLog">true: 通信ログを出力する　　false: 通信ログを出力しない</param>
        /// ---------------------------------------------------------------------------------------- 
        public void SetOutputDataLogMode(bool nbEnableOutputLog)
        {
            m_OutputDataLog = nbEnableOutputLog;
        }
        #endregion

        #region IPアドレスの末尾にピリオド「.」をつける
        /// <summary>
        /// IPアドレスの末尾にピリオド「.」をつける(つけないとソケットオープン出来ない事象があったための対応)
        /// </summary>
        public void AddPeriodAfterIPAddress(bool nbAddPeriodAfterIPAddress)
        {
            m_bAddPeriodAfterIPAddress = nbAddPeriodAfterIPAddress;
        }
        #endregion

        #region サーバーの再開をする

        /// <summary>
        /// サーバーの再開をする
        /// </summary>
        /// <returns></returns>
        public int RestartServer()
        {
            //  一旦ソケットをクローズする
            CloseSocket();
            //  再オープンする
            OpenSocket(m_iServerPortNo);

            return 0;
        }

        /// <summary>
        /// 文字コードを設定する
        /// </summary>
        /// <param name="nEncode"></param>
        public void SetEncoding(eEncode nEncode)
        {
            m_strEncoding = m_dictEncode[nEncode];
        }

        #endregion


    }
}
