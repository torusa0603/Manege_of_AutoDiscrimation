using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manege_of_AutoDiscrimation
{
    class SocketCommunication
    {
        #region メンバー変数

        private const string m_cstrCommandPrefix = "/";         // コマンド識別記号
        private const string m_cstrCommandTermination = "\n";   // コマンド終端記号
        private const string m_cstrCommandOK = "OK";
        private const string m_cstrCommandError = "NG";
        private static int m_ciEXCEPTION_ERROR = -99;

        public Action<int> evCommandReceive;                    //  コマンド受信イベント
        public Action evSocketClose;                            //  ソケットクローズイベント

        private bool m_bReplyDone = true;                       //  受信したコマンドに対して応答文字列を返した

        public const int m_ciStartUpPGStop = 0;                 //  処理停止コマンドインデックス
        public const int m_ciStartUpPGStart = 1;                //  処理開始コマンドインデックス
        public const int m_ciStartUpPGStartB = 2;               //  処理開始コマンドインデックス

        private readonly List<string> m_lstCommand = new List<string>() //  コマンドリスト
        {
            "Stop",                 //  [StartUpPG] 処理停止コマンド文字列
            "Start",                //  [StartUpPG] 処理開始コマンド文字列
            "StartB",               //  [StartUpPG] 処理 B 開始コマンド文字列
            "DiscrimateEnd"         //  [Python] 検査終了文字列
        };

        private SPCommonSocket.CSocketCommunicationBase m_cSocket;      //  ソケット通信クラス

        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 受信イベントハンドラ
        /// </summary>
        /// <param name="nstrReceiveData"></param>
        private void ReceiveDataHandler(string nstrReceiveData)
        {
            int i_ret = 0;
            string str_temp;
            //  受信した文字列解析

            //  先頭文字がコマンド識別記号か
            if (nstrReceiveData.StartsWith(m_cstrCommandPrefix) == false)
            {
                i_ret = 1;
            }
            //  最後がターミネーター記号化
            else if (nstrReceiveData.EndsWith(m_cstrCommandTermination) == false)
            {
                i_ret = 2;
            }
            //  まだコマンドに対する応答文字列を返信してないのに再度コマンドを受信した
            else if (m_bReplyDone == false)
            {
                i_ret = 4;
            }
            //  コマンド文字列抽出
            else
            {
                str_temp = nstrReceiveData.Substring(1, nstrReceiveData.Length - 2);
                //  コマンド未定義
                if (m_lstCommand.Contains(str_temp) == false)
                {
                    i_ret = 3;
                }
                //  コマンド受信イベントを発生させる
                else
                {
                    //  コマンドを受信したので応答を返したフラグを下げる。応答したらフラグを立てる
                    m_bReplyDone = false;
                    evCommandReceive?.Invoke(m_lstCommand.IndexOf(str_temp));
                }
            }

            if (i_ret != 0)
            {
                //  コマンドエラーであればエラーを返す
                SendCommandError(i_ret);
            }
            else
            {
                SendCommand(100);
            }
            m_bReplyDone = true;
        }

        /// <summary>
        /// ソケットがクローズしたことを知るイベントハンドラ
        /// </summary>
        private void CloseSocketHandler()
        {
            evSocketClose?.Invoke();
        }

        #endregion

        #region パブリック関数

        /// <summary>
        /// 通信機能の初期化
        /// </summary>
        /// <param name="nParentForm">フォーム</param>
        /// <param name="nstrIPAddress">アドレス</param>
        /// <param name="niPortNo">ポート番号</param>
        /// <param name="niType">1:サーバー、それ以外はクライアント</param>
        /// <returns>0:正常 -1:サーバー機能はない -2:既にソケットをオープンしている -3:ソケットオープンエラー</returns>
        public int Init(Form nParentForm, string nstrIPAddress, int niPortNo, int niType)
        {
            try
            {
                if(niType == SPCommonSocket.CSocketCommunicationBase.SERVER)
                {
                    //  ソケット通信クラスオブジェクト作成(サーバーとして)
                    m_cSocket = new SPCommonSocket.CSocketCommunicationBase(SPCommonSocket.CSocketCommunicationBase.SERVER, nParentForm);
                }
                else
                {
                    //  ソケット通信クラスオブジェクト作成(クライアントとして)
                    m_cSocket = new SPCommonSocket.CSocketCommunicationBase(SPCommonSocket.CSocketCommunicationBase.CLIENT);
                }
                
                //  ソケットオープン
                int i_ret = m_cSocket.OpenSocket(niPortNo);
                //  ソケットが正常にオープンされた
                if (i_ret == 0)
                {
                    //  受信イベントハンドラを追加
                    m_cSocket.evReceiveData += ReceiveDataHandler;
                    //  相手のソケットがクローズしたことを知るイベントハンドラを追加
                    m_cSocket.evSocketClose += CloseSocketHandler;
                    //  通信ログ出力を有効にする
                    m_cSocket.SetOutputDataLogMode(true);
                }
                return i_ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(MethodBase.GetCurrentMethod().Name + "," + ex.Message);
                return m_ciEXCEPTION_ERROR;
            }

        }

        /// <summary>
        /// 通信機能の終了処理
        /// </summary>
        public void Close()
        {
            try
            {
                //  ソケットをクローズする
                m_cSocket.CloseSocket();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(MethodBase.GetCurrentMethod().Name + "," + ex.Message);
            }
        }

        /// <summary>
        /// コマンドエラーを返す
        /// </summary>
        /// <param name="niErrorNo"></param>
        /// <returns></returns>
        public int SendCommandError(int niErrorNo)
        {
            try
            {
                //  送信文字列作成 /IL X\n
                //string str_send_command = m_cstrCommandPrefix + m_cstrCommandError + " " + niErrorNo.ToString() + m_cstrCommandTermination;

                //  送信文字列作成 /NG\n
                string str_send_command = m_cstrCommandPrefix + m_cstrCommandError + m_cstrCommandTermination;

                //  送信
                return m_cSocket.Send(str_send_command);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(MethodBase.GetCurrentMethod().Name + "," + ex.Message);
                return m_ciEXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// [StartUpPG] 応答を返す
        /// </summary>
        /// <param name="niParam">100:OK, 0:</param>
        /// <returns></returns>
        public int SendCommand(int niParam)
        {
            try
            {
                //  送信文字列作成 /Start X\n
                //string str_send_command = m_cstrCommandPrefix + m_lstCommand[m_ciStartUpPGStart] + m_cstrCommandTermination;

                string strRet = m_cstrCommandError;
                if (niParam == 100)
                {
                    strRet = m_cstrCommandOK;
                }
                else
                {
                    strRet = m_lstCommand[niParam];
                }

                //  送信文字列作成 /OK\n
                string str_send_command = m_cstrCommandPrefix + strRet + m_cstrCommandTermination;
                //  応答完了フラグをtrueにする
                m_bReplyDone = true;
                //  送信
                return m_cSocket.Send(str_send_command);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(MethodBase.GetCurrentMethod().Name + "," + ex.Message);
                return m_ciEXCEPTION_ERROR;
            }

        }


        #endregion

    }
}
