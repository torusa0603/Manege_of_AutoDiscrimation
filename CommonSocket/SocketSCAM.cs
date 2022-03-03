using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

//  XMLドキュメントの警告は無視
#pragma warning disable 1591

namespace SPCommonSocket
{
    #region SCAMからの受信データ情報構造体
    //  SCAMからの受信データ情報構造体
    public struct ReceiveFromSCAMInfo
    {
        public string strRawReceiveData;
        public int iDataType;
        public int iCommandReply;
        public int iWparam;
        public int iLparam;
        public int iMessageNo;
        public string[] strReceiveDataArray;
		public string strLastSentCommand;
    }
    #endregion

    //  イベントハンドラ用デリゲート
    public delegate void EventHandlerReceiveSCAM(ReceiveFromSCAMInfo ReceiveInfo);

    public class CSocketSCAM : CSocketCommunicationBase
    {
        #region イベント定義
        //  SCAMデータ受信イベント
        public event EventHandlerReceiveSCAM evReceiveSCAMDataInfo;
        #endregion

        #region メンバー変数

        /// <summary>
        /// リモートコマンドの返答 /OK
        /// </summary>
        public const int eCommandReplyOK    = 0;
        /// <summary>
        /// リモートコマンドの返答 /NG
        /// </summary>
        public const int eCommandReplyNG    = 1;
        /// <summary>
        /// リモートコマンドの返答 /IL
        /// </summary>
        public const int eCommandReplyIL    = 2;
        /// <summary>
        /// リモートコマンドの返答 /ManualEnd
        /// </summary>
        public const int eCommandManualEnd = 6;
        /// <summary>
        /// :WM_USERメッセージ
        /// </summary>
        public const int eWMUSER            = 3;
        /// <summary>
        /// :SCAM_EVENTメッセージ
        /// </summary>
        public const int eSCAM_EVENT        = 4;
        /// <summary>
        /// :AF_getZAxisStatusメッセージ
        /// </summary>
        public const int eZAxisStatus       = 5;
        /// <summary>
        /// その他
        /// </summary>
        public const int eOther             = 100;
        /// <summary>
        /// 不定値
        /// </summary>
        public const int eInvalid           = -9999;

        const string strCommandOK           = "/OK";
        const string strCommandNG           = "/NG";
        const string strCommandIL           = "/IL";
        const string strCommandManualEnd    = "/ManualEnd";
        const string strWM_USER             = ":WM_USER";
        const string strSCAM_EVENT          = ":SCAM_EVENT";
        const string strZAxisStatus         = ":AF_getZAxisStatus";

        public bool m_bSentRemoteCommand;   //  リモートコマンドを送信するとtrueとなり
                                            //  その返答を受け取るとfalseに戻る
        const string strDoublePointFormat = "F6";   //  浮動小数点を文字列に変換するときの小数点以下の桁数

        public bool m_SentExeptionCommand;  //  2度送り防止の例外コマンドを送信するとtrue
                                            //  その返答を受け取るとfalse
		public string m_strLastSentNormalCommand;	//	最後にSCAMに送信したコマンド名(二度送り防止の普通のコマンド)
		public string m_strLastSentExceptionCommand;	//	最後にSCAMに送信したコマンド名(二度送り防止の例外コマンド)
        #endregion

        #region コンストラクタ
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// 
        /// <param name="niType">
        /// 
        /// <para>作成しようとしているソケットタイプ</para>
        /// <para>必ずCSocketCommunicationBase.SCAM_CLIENTを設定してください</para>
        /// 
        /// </param>
        /// 
        /// <param name="nformParent">親フォーム</param>
        /// ----------------------------------------------------------------------------------------
        public CSocketSCAM(int niType, Form nformParent ): base(CSocketCommunicationBase.SCAM_CLIENT, nformParent)
        {
            m_bSentRemoteCommand = false;
            m_SentExeptionCommand = false;
			m_strLastSentNormalCommand = "";
			m_strLastSentExceptionCommand = "";
            this.evReceiveDataForSCAM += ReceiveDataFromBase;
        }
        #endregion

        #region コンストラクタ2
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ2
        /// </summary>
        /// 
        /// <param name="niType">
        /// 
        /// <para>作成しようとしているソケットタイプ</para>
        /// <para>必ずCSocketCommunicationBase.SCAM_CLIENTを設定してください</para>
        /// 
        /// </param>
        /// 
        /// ----------------------------------------------------------------------------------------
        public CSocketSCAM(int niType)
            : base(CSocketCommunicationBase.SCAM_CLIENT)
        {
            m_bSentRemoteCommand = false;
            m_SentExeptionCommand = false;
			m_strLastSentNormalCommand = "";
			m_strLastSentExceptionCommand = "";
            this.evReceiveDataForSCAM += ReceiveDataFromBase;
        }
        #endregion

        #region ソケットオープン(IPもしくはホスト名指定)
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  ソケットオープン(IPもしくはホスト名指定)
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
        override public int OpenSocket(string nstrIPHost, int niPort)
        {
            //  2度送りフラグをfalseにする
            m_bSentRemoteCommand = false;
            return base.OpenSocket(nstrIPHost, niPort);
        }
        #endregion

        #region ソケットオープン
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>ソケットオープン(ホスト名として自分自身のPCのホスト名を使用する)</para>
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
        override public int OpenSocket(int niPort)
        {
            //  2度送りフラグをfalseにする
            m_bSentRemoteCommand = false;
            return base.OpenSocket(niPort);
        }
        #endregion

        #region リモートコマンド送信中フラグをクリアし、送信していない状態に戻す
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  リモートコマンド送信中フラグをクリアし、送信していない状態に戻す
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void CancelCommandSent()
        {
            m_bSentRemoteCommand = false;
            m_SentExeptionCommand = false;
        }
        #endregion

        #region データ受信イベントハンドラ
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// データ受信イベントハンドラ
        /// </summary>
        /// 
        /// <param name="strReceiveData">受信データ</param>
        /// ----------------------------------------------------------------------------------------
        public void ReceiveDataFromBase(string strReceiveData)
        {
            int i_index;
            ReceiveFromSCAMInfo info = new ReceiveFromSCAMInfo();

            try
            {
                //  不定値で初期化
                info.iCommandReply = info.iDataType = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;

                //  受信文字列解読

                //  データ分割(半角スペース)
                string[] stArrayData = strReceiveData.Split(' ');

                if (stArrayData.Count() > 0)
                {
                    //  分割した最後のデータの末尾に終端文字や改行文字が入っていれば除去
                    stArrayData[stArrayData.Count() - 1] = stArrayData[stArrayData.Count() - 1].Trim('\0', '\n');

                    //  データタイプを決定
                    switch (stArrayData[0])
                    {
                        case strCommandOK:
                            info.iDataType = eCommandReplyOK;

                            //  2度送り防止の例外コマンドの返答であった場合
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	コマンド名を空にする
								m_strLastSentExceptionCommand = "";
                            }
                            //  通常のリモートコマンドの返答であった場合
                            else
                            {
                                //  リモートコマンドに対する返答を受信したので、コマンド送信フラグを下げる
                                m_bSentRemoteCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	コマンド名を空にする
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandNG:
                            info.iDataType = eCommandReplyNG;

                            //  2度送り防止の例外コマンドの返答であった場合
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	コマンド名を空にする
								m_strLastSentExceptionCommand = "";
                            }
                            //  通常のリモートコマンドの返答であった場合
                            else
                            {
                                //  リモートコマンドに対する返答を受信したので、コマンド送信フラグを下げる
                                m_bSentRemoteCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	コマンド名を空にする
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandIL:
                            info.iDataType = eCommandReplyIL;

                            //  2度送り防止の例外コマンドの返答であった場合
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	コマンド名を空にする
								m_strLastSentExceptionCommand = "";
                            }
                            //  通常のリモートコマンドの返答であった場合
                            else
                            {
                                //  リモートコマンドに対する返答を受信したので、コマンド送信フラグを下げる
                                m_bSentRemoteCommand = false;
								//	コマンド名を返す。
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	コマンド名を空にする
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandManualEnd:
                            info.iDataType = eCommandManualEnd;
                            //  リモートコマンドに対する返答を受信したので、コマンド送信フラグを下げる
                            m_bSentRemoteCommand = false;
							//	コマンド名を返す。
							info.strLastSentCommand = m_strLastSentNormalCommand;
							//	コマンド名を空にする
							m_strLastSentNormalCommand = "";
                            break;
                        case strWM_USER:
                            info.iDataType = eWMUSER;
                            break;
                        case strSCAM_EVENT:
                            info.iDataType = eSCAM_EVENT;
                            break;
                        default:
                            info.iDataType = eOther;
                            break;
                    }
                    //  その他に分類されても:AF_getZAxisStatusメッセージだけは特殊なのでここでチェック
                    //  また、/OK,xというカンマ区切りで返ってくるコマンドもあるのでそれもここでチェック
                    if (info.iDataType == eOther)
                    {
                        //  文字列内に:AF_getZAxisStatusという文字が含まれるか確認
                        if (stArrayData[0].IndexOf(strZAxisStatus) >= 0)
                        {
                            //  含まれている場合
                            info.iDataType = eZAxisStatus;
                            //  =以降の数値を取り出す
                            i_index = stArrayData[0].IndexOf("=");
                            if (i_index > 0)
                            {
                                info.iCommandReply = int.Parse(stArrayData[0].Substring(i_index + 1));
                            }
                        }
                        //  /OKが含まれるか確認
                        if (stArrayData[0].IndexOf(strCommandOK) >= 0)
                        {
                            //  含まれている場合
                            info.iDataType = eCommandReplyOK;
                            //  リモートコマンドに対する返答を受信したので、コマンド送信フラグを下げる
                            m_bSentRemoteCommand = false;   
                        }
                    }

                    //  リモートコマンド応答でかつ、NG、ILであった場合
                    if (info.iDataType == eCommandReplyNG || info.iDataType == eCommandReplyIL)
                    {
                        //  エラー番号を設定する
                        if (stArrayData.Count() > 1)
                        {
                            info.iCommandReply = int.Parse(stArrayData[1]);
                        }
                    }
                    //  WM_USERメッセージであった場合
                    else if (info.iDataType == eWMUSER)
                    {
                        //  必ず　:WM_USER xxx yyy zzzの4つの文字列に分割されている必要あり
                        if (stArrayData.Count() == 4)
                        {
                            info.iMessageNo = int.Parse(stArrayData[1]);
                            info.iWparam = int.Parse(stArrayData[2]);
                            info.iLparam = int.Parse(stArrayData[3]);
                        }
                    }
                }

                //  生受信データ文字列を設定
                info.strRawReceiveData = strReceiveData;
                //  スペース区切りで分割された文字列配列を設定
                info.strReceiveDataArray = new string[stArrayData.Count()];
                for (int i_loop = 0; i_loop < stArrayData.Count(); i_loop++)
                {
                    info.strReceiveDataArray[i_loop] = stArrayData[i_loop];
                }
            }
            catch (FormatException)
            {
                //  例外発生した場合は、DataTypeと生データだけ渡すことにする
                info.iCommandReply = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;
                info.strRawReceiveData = strReceiveData;
            }
            catch (Exception)
            {
                //  例外発生した場合は、DataTypeと生データだけ渡すことにする
                info.iCommandReply = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;
                info.strRawReceiveData = strReceiveData;
            }
            finally
            {
                //  最後にイベント送信
                if (evReceiveSCAMDataInfo != null)    //  登録してない可能性あるのでこれ必須
                {
                    //  イベント送信
                    evReceiveSCAMDataInfo(info);
                }
            }
        }
        #endregion

        #region SCAMリモートコマンド送信一覧     (新たにコマンドを定義する場合はここに追加する)

        #region Connect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// このコマンドを受け付けるとリモート機能を開始します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Connect()
        {
            const string cstr_command = "/Connect";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);     
        }
        #endregion

        #region DisConnect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// リモート機能を終了します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DisConnect()
        {
            const string cstr_command = "/DisConnect";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);  
        }
        #endregion

        #region Auto1
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// オート１を実行します
        /// </summary>
        /// 
        /// <param name="niMacroNum">実行するマクロファイル数</param>
        /// <param name="nstrMacroFile">マクロファイル名配列</param>
        /// <param name="nstrOutputFile">結果ファイル名</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Auto1(int niMacroNum, string[] nstrMacroFile, string nstrOutputFile)
        {
            const string cstr_command = "/Auto1";
            List<string> list_send_data_list = new List<string>();

            //  マクロ数と、実際のマクロファイル名配列の要素数が異なっていればエラー
            if (niMacroNum != nstrMacroFile.Count())
            {
                return -1;
            }
            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMacroNum.ToString());
            foreach (string macro in nstrMacroFile)
            {
                list_send_data_list.Add(macro);
            }
            list_send_data_list.Add(nstrOutputFile);
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false); 
        }
        #endregion

        #region AFMoveTo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	Zの移動命令
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:絶対値移動　false:相対値移動</param>
        /// <param name="ndZPosition">Z座標</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int AFMoveTo(bool nbABSMove, double ndZPosition)
        {
            const string cstr_command = "/AFMoveTo";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  絶対値移動
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  相対値移動
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndZPosition.ToString(strDoublePointFormat)); 

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Status
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 問い合わせを行います
        /// </summary>
        /// 
        /// <param name="nRequestNo">
        /// 
        /// <para>問い合わせ番号(1～)</para>
        /// 
        /// <para>1:実行モード</para>
        /// <para>2:実行中のマクロ</para>
        /// <para>3:結果レコード件数</para>
        /// <para>4:表示状態</para>
        /// <para>5:原点復帰などの初期化処理が終了したか否か</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int Status( int nRequestNo )
        {
            const string cstr_command = "/Status";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nRequestNo.ToString());

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region TableMoveTo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    テーブルの移動命令
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:絶対値移動　false:相対値移動</param>
        /// <param name="ndX">X座標</param>
        /// <param name="ndY">Y座標</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int TableMoveTo(bool nbABSMove, double ndX, double ndY)
        {
            const string cstr_command = "/TableMoveTo";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  絶対値移動
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  相対値移動
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Through
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  画像のスルーを行います
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Through()
        {
            const string cstr_command = "/Through";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Freeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  画像のフリーズを行います
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Freeze()
        {
            const string cstr_command = "/Freeze";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AFocusA
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// オートフォーカスＡを実行します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int AFocusA()
        {
            const string cstr_command = "/AFocusA";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AFocusB
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// オートフォーカスBを実行します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int AFocusB()
        {
            const string cstr_command = "/AFocusB";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RunMacro
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     マクロ実行命令
        /// </summary>
        /// 
        /// <param name="nstrMacroFileName">実行するマクロファイル名</param>
        /// <param name="niFlg">0以外ならアライメント初期化なし</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RunMacro(string nstrMacroFileName, int niFlg)
        {
            const string cstr_command = "/RunMacro";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrMacroFileName);
            list_send_data_list.Add(niFlg.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightOn
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    照明制御を行います
        /// </summary>
        /// 
        /// <param name="niLightType">0: 透過  1: 落射  2: 反射</param>
        /// <param name="niValue">照度 （0-100）</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightOn(int niLightType, int niValue)
        {
            const string cstr_command = "/LightOn";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            list_send_data_list.Add(niValue.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    照明制御を行います
        /// </summary>
        /// 
        /// <param name="niLightType">0: 透過  1: 落射  2: 反射</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightOff(int niLightType)
        {
            const string cstr_command = "/LightOff";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Light2On
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    照明制御を行います
        /// </summary>
        /// 
        /// <param name="niDirection">0:後  1:右  2:前  3:左</param>
        /// <param name="niValue">照度 (0-100)</param>
        /// <param name="niAngle">角度  0:３０度  1:４５度  2:６０度</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Light2On(int niDirection, int niValue, int niAngle)
        {
            const string cstr_command = "/Light2On";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDirection.ToString());
            list_send_data_list.Add(niValue.ToString());
            list_send_data_list.Add(niAngle.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Light2Off
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    照明制御を行います
        /// </summary>
        /// 
        /// <param name="niDirection">0:後  1:右  2:前  3:左</param>
        /// <param name="niAngle">角度  0:３０度  1:４５度  2:６０度</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Light2Off(int niDirection, int niAngle)
        {
            const string cstr_command = "/Light2Off";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDirection.ToString());
            list_send_data_list.Add(niAngle.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     アライメントをセットします
        /// </summary>
        /// 
        /// <param name="ndX">設定座標 X</param>
        /// <param name="ndY">設定座標 Y</param>
        /// <param name="ndAngle">設定角度</param>
        /// <param name="ndAxis">設定軸</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetAlign(double ndX, double ndY, double ndAngle, double ndAxis)
        {
            const string cstr_command = "/SetAlign";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAngle.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAxis.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ResetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  アライメントをリセットします
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetAlign()
        {
            const string cstr_command = "/ResetAlign";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetFaceAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    面アライメントをセットします
        /// </summary>
        /// 
        /// <param name="ndXOrigin">X軸原点（ワールド座標系）</param>
        /// <param name="ndYOrigin">Y軸原点（ワールド座標系）</param>
        /// <param name="ndZOrigin">Z軸原点</param>
        /// <param name="ndXAxisAngle">面上のX軸の傾き</param>
        /// <param name="ndYAxisAngle">面のX軸の傾き(Z軸)</param>
        /// <param name="ndZAxisAngle">面のY軸の傾き(Z軸)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetFaceAlign(double ndXOrigin, double ndYOrigin, double ndZOrigin,
                                double ndXAxisAngle, double ndYAxisAngle, double ndZAxisAngle)
        {
            const string cstr_command = "/SetFaceAlign";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(ndXOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndYOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZAxisAngle.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndYAxisAngle.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZAxisAngle.ToString(strDoublePointFormat));

                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + " " +
                                   list_send_data_list[2] + " " + list_send_data_list[3] + " " +
                                    list_send_data_list[4] + " " + list_send_data_list[5] + "," +
                                     list_send_data_list[6] + "\n";

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region ResetFaceAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  面アライメントをリセットします
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetFaceAlign()
        {
            const string cstr_command = "/ResetFaceAlign";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetManual
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///   リモート接続中にマニュアル操作を許可します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetManual()
        {
            const string cstr_command = "/SetManual";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ShowWindow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM表示状態を変更します
        /// </summary>
        /// 
        /// <param name="niMode">0:通常表示 1:最小化 2:最大化</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int ShowWindow(int niMode)
        {
            const string cstr_command = "/ShowWindow";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ErrorAction
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// リモート接続時エラーのダイアログの表示、非表示の設定、再測定の問合わせダイアログの表示、非表示の設定を行います
        /// </summary>
        /// 
        /// <param name="niMode">0:通常表示 1:非表示</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int ErrorAction(int niMode)
        {
            const string cstr_command = "/ErrorAction";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region PixelToTable
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 渡された値を、視野内かんめん座標からワーク座標に変換して返します
        /// </summary>
        /// 
        /// <param name="ndPixelX">視野内かんめん座標 X</param>
        /// <param name="ndPixelY">視野内かんめん座標 Y</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int PixelToTable(double ndPixelX, double ndPixelY)
        {
            const string cstr_command = "/PixelToTable";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndPixelX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndPixelY.ToString(strDoublePointFormat));

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetHardError
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM側でハードエラーが発生しているかどうかの状態を返します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetHardError()
        {
            const string cstr_command = "/GetHardError";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region Filter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// フィルタリングの実行命令
        /// </summary>
        /// 
        /// <param name="niFilterGroup">フィルターグループ</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int Filter(int niFilterGroup)
        {
            const string cstr_command = "/Filter";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niFilterGroup.ToString());

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LAFocusTrace
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  LAFトレースモードの制御実行命令
        /// </summary>
        /// 
        /// <param name="niTraceMode">0:LAFトレースモードの終了を指示    1:開始を指示</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int LAFocusTrace(int niTraceMode)
        {
            const string cstr_command = "/LAFocusTrace";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niTraceMode.ToString());

            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetWorld
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	基準座標を設定します
        /// </summary>
        /// 
        /// <param name="ndX">X座標(mm)</param>
        /// <param name="ndY">Y座標(mm)</param>
        /// <param name="ndAngle">角度(°)</param>
        /// <param name="ndReverseXFlg">x軸反転フラグ(0:off, 1:on)</param>
        /// <param name="ndReverseYFlg">x軸反転フラグ(0:off, 1:on)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetWorld(double ndX, double ndY, double ndAngle, double ndReverseXFlg, double ndReverseYFlg)
        {
            const string cstr_command = "/SetWorld";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAngle.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndReverseXFlg.ToString());
            list_send_data_list.Add(ndReverseYFlg.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWorld
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 基準座標を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWorld()
        {
            const string cstr_command = "/GetWorld";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ローカルアライメント原点位置を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAlign()
        {
            const string cstr_command = "/GetAlign";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetContBri
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     <para>画像処理ボードのコントラスト・ブライトネスを設定します。</para>
        ///     <para>カメラ番号にハードウェアに存在しない番号を指定するとすべてのカメラに対して設定を行います</para>
        /// </summary>
        /// 
        /// <param name="niCameraNo">カメラ番号(0:Main-Lo, 1:Main-Hi, 2:Sub-Lo, 3:Sub-Hi)</param>
        /// <param name="ndContrast">コントラスト(0.0 -> 1.0)</param>
        /// <param name="ndBrightness">ブライトネス(0.0 -> 1.0)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetContBri(int niCameraNo, double ndContrast, double ndBrightness)
        {
            const string cstr_command = "/SetContBri";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            list_send_data_list.Add(ndContrast.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndBrightness.ToString(strDoublePointFormat));
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetContBri
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	画像処理ボードのコントラスト・ブライトネスを取得します
        /// </summary>
        /// 
        /// <param name="niCameraNo">カメラ番号(0:Main-Lo, 1:Main-Hi, 2:Sub-Lo, 3:Sub-Hi)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetContBri(int niCameraNo)
        {
            const string cstr_command = "/GetContBri";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMMACPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 現在SCAMに設定されているMAC形式ティーチングマクロファイルの保存先をフルパスで取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMMACPath()
        {
            const string cstr_command = "/GetSCAMMACPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMCSVPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	現在SCAMに設定されているCSV形式ファイルの保存先をフルパスで取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMCSVPath()
        {
            const string cstr_command = "/GetSCAMCSVPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMMACPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  現在SCAMに設定されているMAC形式ティーチングマクロファイルの保存先をパラメーターで指定したパス名で更新します
        /// </summary>
        /// 
        /// <param name="nstrMACPath">MAC形式ティーチングマクロファイルの保存先</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMMACPath(string nstrMACPath)
        {
            const string cstr_command = "/SetSCAMMACPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrMACPath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMCSVPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	現在SCAMに設定されているCSV形式ファイルの保存先をパラメーターで指定したパス名で更新します
        /// </summary>
        /// 
        /// <param name="nstrCSVPath">CSVファイルの保存先</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMCSVPath(string nstrCSVPath)
        {
            const string cstr_command = "/SetSCAMCSVPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrCSVPath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SaveImageJPGNow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 最後に取り込まれた画像を、JPEG形式ファイルとして保存します
        /// </summary>
        /// 
        /// <param name="nstrSaveFileFullPath">保存するファイル名（フルパスで拡張子付きで指定)</param>
        /// <param name="niColor">
        /// 
        /// <para>0：モノクロ</para>
        /// <para>1：R+G+B </para>
        /// <para>2：R</para>
        /// <para>3：G</para>
        /// <para>4：B</para>
        /// 
        /// </param>
        /// <param name="niGraphic">画像にグラフィックスをオーバレイするときは1、しない場合は0を指定</param>
        /// <param name="niQuality">JPEGの品質を1（低品質、サイズ小）～100（高品質、サイズ大）で指定</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SaveImageJPGNow(string nstrSaveFileFullPath, int niColor, int niGraphic, int niQuality )
        {
            const string cstr_command = "/SaveImageJPGNow";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveFileFullPath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            list_send_data_list.Add(niQuality.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StartSaveImageJPGOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>マクロコマンドFreezeが実行されるごとに、JPEG形式ファイルを</para>
        /// <para>指定したベース名 ＋ アンダーバー ＋ シーケンシャル番号 ＋ ．JPG</para>
        /// <para>という名前で保存します。シーケンシャル番号はこのコマンドを受信してから最初のFreezeで1に初期化され、以後、保存する度に1インクリメントされます</para>
        /// </summary>
        /// 
        /// <param name="nstrSaveBaseFilePath">保存するベースファイル名（パス付きで指定、拡張子は不要)</param>
        /// <param name="niColor">
        /// 
        /// <para>0：モノクロ</para>
        /// <para>1：R+G+B </para>
        /// <para>2：R</para>
        /// <para>3：G</para>
        /// <para>4：B</para>
        /// 
        /// </param>
        /// <param name="niGraphic">画像にグラフィックスをオーバレイするときは1、しない場合は0を指定</param>
        /// <param name="niQuality">JPEGの品質を1（低品質、サイズ小）～100（高品質、サイズ大）で指定</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StartSaveImageJPGOnFreeze(string nstrSaveBaseFilePath, int niColor, int niGraphic, int niQuality)
        {
            const string cstr_command = "/StartSaveImageJPGOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveBaseFilePath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            list_send_data_list.Add(niQuality.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StopSaveImageJPGOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  コマンドStartSaveImageJPGOnFreezeによる画像取り込みごとの自動画像保存モードを終了します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StopSaveImageJPGOnFreeze()
        {
            const string cstr_command = "/StopSaveImageJPGOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SaveImageBMPNow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	最後に取り込まれた画像を、BMP形式ファイルとして保存します
        /// </summary>
        /// 
        /// <param name="nstrSaveFileFullPath">保存するファイル名（フルパスで拡張子付きで指定)</param>
        /// <param name="niColor">0：モノクロ、1：R+G+B 2：R、3：G、4：B</param>
        /// <param name="niGraphic">画像にグラフィックスをオーバレイするときは1、しない場合は0を指定</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SaveImageBMPNow(string nstrSaveFileFullPath, int niColor, int niGraphic)
        {
            const string cstr_command = "/SaveImageBMPNow";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveFileFullPath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StartSaveImageBMPOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>マクロコマンドFreezeが実行されるごとに、BMP形式ファイルを</para>
        /// <para>指定したベース名 ＋ アンダーバー ＋ シーケンシャル番号 ＋ ．BMP</para>
        /// <para>という名前で保存します。シーケンシャル番号はこのコマンドを受信してから最初のFreezeで1に初期化され、以後、保存する度に1インクリメントされます</para>
        /// </summary>
        /// 
        /// <param name="nstrSaveBaseFilePath">保存するベースファイル名（パス付きで指定、拡張子は不要)</param>
        /// <param name="niColor">
        /// 
        /// <para>0：モノクロ</para>
        /// <para>1：R+G+B </para>
        /// <para>2：R</para>
        /// <para>3：G</para>
        /// <para>4：B</para>
        /// 
        /// </param>
        /// <param name="niGraphic">画像にグラフィックスをオーバレイするときは1、しない場合は0を指定</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StartSaveImageBMPOnFreeze(string nstrSaveBaseFilePath, int niColor, int niGraphic )
        {
            const string cstr_command = "/StartSaveImageBMPOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveBaseFilePath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StopSaveImageBMPOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	コマンドStartSaveImageBMPOnFreezeによる画像取り込みごとの自動画像保存モードを終了します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StopSaveImageBMPOnFreeze()
        {
            const string cstr_command = "/StopSaveImageBMPOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMATCPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在SCAMに設定されているATC形式ティーチングマクロファイルの保存先をフルパスで取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMATCPath()
        {
            const string cstr_command = "/GetSCAMATCPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMATCPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在SCAMに設定されているATC形式ティーチングマクロファイルの保存先をパラメーターで指定したパス名で更新します
        /// </summary>
        /// 
        /// <param name="nstrATCPath">ATC形式ティーチングマクロファイルの保存先</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMATCPath(string nstrATCPath)
        {
            const string cstr_command = "/SetSCAMATCPath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrATCPath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetNowWorkPos
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	現在のテーブル位置をワーク座標系で取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetNowWorkPos()
        {
            const string cstr_command = "/GetNowWorkPos";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ShutteOffOnDisconnect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///リモートディスコネクト（コマンドのdisconnectではない）時にシャッタをCloseする（通常はCloseする）か否かを設定します
        /// </summary>
        /// 
        /// <param name="niMode">0以外はCloseする。0の場合はCloseしない</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ShutteOffOnDisconnect(int niMode)
        {
            const string cstr_command = "/ShutteOffOnDisconnect";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region DilateFilter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///Dilate filterを実行します
        /// </summary>
        /// 
        /// <param name="niCount">0Dilate filterの繰り返し回数</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DilateFilter(int niCount)
        {
            const string cstr_command = "/DilateFilter";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCount.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ErodeFilter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    Erode filterを実行します
        /// </summary>
        /// 
        /// <param name="niCount">ErodeFilterの繰り返し回数</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ErodeFilter(int niCount)
        {
            const string cstr_command = "/ErodeFilter";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCount.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ResetImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// フィルター処理を行う前の画像に戻します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetImage()
        {
            const string cstr_command = "/ResetImage";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAcqPixelArea
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 現在の画像取り込み画面の画素数を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAcqPixelArea()
        {
            const string cstr_command = "/GetAcqPixelArea";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRemoteNoRemeasureOther
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// オート１、リモートによる自動実行に対する再測定（アラインメント以外）の有無フラグを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteNoRemeasureOther()
        {
            const string cstr_command = "/GetRemoteNoRemeasureOther";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRemoteNoRemeasureAlignment
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	オート１、リモートによる自動実行に対する再測定（アラインメント）の有無フラグを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteNoRemeasureAlignment()
        {
            const string cstr_command = "/GetRemoteNoRemeasureAlignment";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetRemoteNoRemeasureOther
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	オート１、リモートによる自動実行に対する再測定（アラインメント以外）の有無フラグを設定します
        /// </summary>
        /// 
        /// <param name="niRemeasure">	0：再測定を行う、1：再測定を行わない</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemoteNoRemeasureOther(int niRemeasure )
        {
            const string cstr_command = "/SetRemoteNoRemeasureOther";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRemeasure.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetRemoteNoRemeasureAlignment
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  	オート１、リモートによる自動実行に対する再測定（アラインメント）の有無フラグを設定します
        /// </summary>
        /// 
        /// <param name="niRemeasure">	0：再測定を行う、1：再測定を行わない</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemoteNoRemeasureAlignment(int niRemeasure)
        {
            const string cstr_command = "/SetRemoteNoRemeasureAlignment";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRemeasure.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightInformation
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 照明のハードウェア定義情報を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightInformation()
        {
            const string cstr_command = "/LightInformation";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	指定した顕微鏡の照明状態を取得します
        /// </summary>
        /// 
        /// <param name="niCameraNo">
        /// <para>0:メイン顕微鏡</para>
        /// <para>1:サブ顕微鏡</para>
        /// <para>2:現在選択されている顕微鏡</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightStatus(int niCameraNo)
        {
            const string cstr_command = "/LightStatus";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region CameraStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// カメラのハードウェア定義情報および、現在使用しているカメラ番号を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int CameraStatus()
        {
            const string cstr_command = "/CameraStatus";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region MaterialHandStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAMがマテハンモード中かどうかを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MaterialHandStatus()
        {
            const string cstr_command = "/MaterialHandStatus";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RunTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	TFTMeasureのレシピを実行します
        /// </summary>
        /// 
        /// <param name="nstrRecipeFilePath">レシピファイル名(フルパス)</param>
        /// <param name="nstrLayerName">レイヤー名</param>
        /// <param name="nstrComment">測定結果ファイルに記述されるコメント     
        ///                           必ずTITLE=xxxxxの形式で指定します（TITLEは大文字です）</param>
        /// <param name="nstrResultFilePath">測定結果ファイル名(フルパス)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int RunTFTMeasure(string nstrRecipeFilePath, string nstrLayerName, string nstrComment,
                                 string nstrResultFilePath)
        {
            const string cstr_command = "/RunTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrRecipeFilePath);
                list_send_data_list.Add(nstrLayerName);
                list_send_data_list.Add(nstrComment);
                list_send_data_list.Add(nstrResultFilePath);

                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + "\t" +
                                   list_send_data_list[2] + "\t" + list_send_data_list[3] + "\t" +
                                    list_send_data_list[4] + "\n";

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region GetTFTMeasureLayer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// TFTMeasureのレイヤー名称を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTFTMeasureLayer()
        {
            const string cstr_command = "/GetTFTMeasureLayer";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTimelyDataSaveInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 自動実行において測定が確定するごとに測定結果を逐次保存するためのパラメーターを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTimelyDataSaveInfo()
        {
            const string cstr_command = "/GetTimelyDataSaveInfo";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTimelyDataSaveInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	自動実行において測定が確定するごとに測定結果を逐次保存するためのパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="niExecute">測定結果の逐次保存を行う（1）か否（0）か</param>
        /// <param name="niFileSaveType">ファイル書き込み方法（1：指定測定数ごと、２：指定経過時間ごと)</param>
        /// <param name="niMeasuerNumber">ファイル書き込み方法が「指定測定数ごと」のときの指定測定数</param>
        /// <param name="niTime">ファイル書き込み方法が「指定経過時間ごと」のときの指定経過時間（秒単位)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetTimelyDataSaveInfo(int niExecute, int niFileSaveType, int niMeasuerNumber, int niTime )
        {
            const string cstr_command = "/SetTimelyDataSaveInfo";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecute.ToString());
            list_send_data_list.Add(niFileSaveType.ToString());
            list_send_data_list.Add(niMeasuerNumber.ToString());
            list_send_data_list.Add(niTime.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAutoSaveDataByStopMeasurement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	自動実行中止時に自動的にデータ保存を行うためのパラメーターを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAutoSaveDataByStopMeasurement()
        {
            const string cstr_command = "/GetAutoSaveDataByStopMeasurement";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAutoSaveDataByStopMeasurement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	自動実行中止時に自動的にデータ保存を行うためのパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="niRun">
        /// 
        /// <para>0：自動実行中止時に自動的にデータ保存を行わない</para>
        /// <para>1：自動実行中止時に自動的にデータ保存を行う</para>
        ///                     
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetAutoSaveDataByStopMeasurement(int niRun)
        {
            const string cstr_command = "/SetAutoSaveDataByStopMeasurement";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRun.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	自動画像保存機能に関するパラメーターを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetImageAutoSaveProp()
        {
            const string cstr_command = "/GetImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRawDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	簡易照明積算タイマ機能において、各照明の積算値（秒単位でのカウント数）を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRawDataLightTimer()
        {
            const string cstr_command = "/GetRawDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetWarningDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 簡易照明積算タイマ機能において、各照明の積算値の警告状態を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWarningDataLightTimer()
        {
            const string cstr_command = "/GetWarningDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region LAFocusSearch
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	LAサーチモードを実行します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LAFocusSearch()
        {
            const string cstr_command = "/LAFocusSearch";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetFormattedRawDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 簡易照明積算タイマ機能において、各照明の積算値を書式化された形式（時：分：秒など）で取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetFormattedRawDataLightTimer()
        {
            const string cstr_command = "/GetFormattedRawDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetWarningSettingsLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	簡易照明積算タイマ機能において、各照明の警告とする積算値を書式化された形式（時：分：秒など）で取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWarningSettingsLightTimer()
        {
            const string cstr_command = "/GetWarningSettingsLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetRemeasureProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	オート１、リモートでの自動実行に対する再測定に関するパラメーターを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemeasureProp()
        {
            const string cstr_command = "/GetRemeasureProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleItemNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// サブタイトルデータの項目数を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleItemNum()
        {
            const string cstr_command = "/GetSubTitleItemNum";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleTitleList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの項目ごとのタイトルリストを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleTitleList()
        {
            const string cstr_command = "/GetSubTitleTitleList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleInputMethodList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの項目ごとの入力方法リストを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleInputMethodList()
        {
            const string cstr_command = "/GetSubTitleInputMethodList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMaxLengthList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの項目ごとの最大長リストを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMaxLengthList()
        {
            const string cstr_command = "/GetSubTitleMaxLengthList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMinLengthList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの項目ごとの最小長リストを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMinLengthList()
        {
            const string cstr_command = "/GetSubTitleMinLengthList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMaxInputHistoryNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの指定した項目番号の入力履歴の最大数を取得します
        /// </summary>
        /// 
        /// <param name="niNth">１から始まるサブタイトルデータの指定した項目番号</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMaxInputHistoryNum(int niNth)
        {
            const string cstr_command = "/GetSubTitleMaxInputHistoryNum";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleSelectList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	サブタイトルデータの指定した項目番号の選択肢リストを取得します
        /// </summary>
        /// 
        /// <param name="niNth">１から始まるサブタイトルデータの指定した項目番号</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleSelectList(int niNth)
        {
            const string cstr_command = "/GetSubTitleSelectList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleInputHistory
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	サブタイトルデータの指定した項目番号の入力履歴リストを取得します
        /// </summary>
        /// 
        /// <param name="niNth">１から始まるサブタイトルデータの指定した項目番号</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleInputHistory(int niNth)
        {
            const string cstr_command = "/GetSubTitleInputHistory";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWeatherSensorData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	気象センサーデータ（温度、気圧、湿度、CO2濃度）を取得します
        /// </summary>
        /// 
        /// <param name="niAxis">軸の番号（1：X,　2：Y（Y1）,　3：Y2,　4：Z)</param>
        /// <param name="niDigitForTempData">温度データを受け取るときの小数点以下の桁数</param>
        /// <param name="niDigitForPressureData">気圧データを受け取るときの小数点以下の桁数</param>
        /// <param name="niDigitForHumidityData">湿度データを受け取るときの小数点以下の桁数</param>
        /// <param name="niDigitForCO2densityData">CO２濃度データを受け取るときの小数点以下の桁数</param>
        /// <param name="niPressureUnit">気圧データを受け取るときの単位（1：hPa,　2：mmHg)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetWeatherSensorData(int niAxis, int niDigitForTempData, int niDigitForPressureData,
                                         int niDigitForHumidityData, int niDigitForCO2densityData,
                                          int niPressureUnit )
        {
            const string cstr_command = "/GetWeatherSensorData";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxis.ToString());
            list_send_data_list.Add(niDigitForTempData.ToString());
            list_send_data_list.Add(niDigitForPressureData.ToString());
            list_send_data_list.Add(niDigitForHumidityData.ToString());
            list_send_data_list.Add(niDigitForCO2densityData.ToString());
            list_send_data_list.Add(niPressureUnit.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetChamberTemperatureData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	チャンバーの温度データを取得します
        /// </summary>
        /// 
        /// <param name="niSensorNo">センサー番号</param>
        /// <param name="niDigitForTempData">温度データを受け取るときの小数点以下の桁数</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetChamberTemperatureData(int niSensorNo, int niDigitForTempData)
        {
            const string cstr_command = "/GetChamberTemperatureData";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niSensorNo.ToString());
            list_send_data_list.Add(niDigitForTempData.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetMasterRecipeForTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Recipe MakerにおけるAuto Recipe Update（通称：Master Recipe）機能に関するパラメーターを取得しま
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetMasterRecipeForTFTMeasure()
        {
            const string cstr_command = "/GetMasterRecipeForTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetMasterRecipeForTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Recipe MakerにおけるAuto Recipe Update（通称：Master Recipe）機能に関するパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="niNum">設定するパラメーターの個数（今回のバージョンでは最大5）</param>
        /// <param name="niAutoRecipeUpdateEnable">Auto Recipe Update 機能が有効である(1)か否(0)か</param>
        /// <param name="niSaveFileBeforeUpdate">更新前のファイルを別名で保存する(1)か否(0)か</param>
        /// <param name="niUpdateAfterAutoMeas">自動実行終了後（再測定前）に更新を行う(1)か否(0)か</param>
        /// <param name="niUpdateAfterReMeas">再測定終了後に更新を行う(1)か否(0)か</param>
        /// <param name="niAlignFileUpdate">更新を行う際、（測定箇所だけではなく）アライメントファイルの更新を行う(1)か否(0)か</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetMasterRecipeForTFTMeasure(int niNum, int niAutoRecipeUpdateEnable, int niSaveFileBeforeUpdate,
                                         int niUpdateAfterAutoMeas, int niUpdateAfterReMeas,
                                          int niAlignFileUpdate )
        {
            const string cstr_command = "/SetMasterRecipeForTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNum.ToString());
            list_send_data_list.Add(niAutoRecipeUpdateEnable.ToString());
            list_send_data_list.Add(niSaveFileBeforeUpdate.ToString());
            list_send_data_list.Add(niUpdateAfterAutoMeas.ToString());
            list_send_data_list.Add(niUpdateAfterReMeas.ToString());
            list_send_data_list.Add(niAlignFileUpdate.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTFTMeasureZInputSupportPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>レシピ実行時にZ座標の入力支援機能を使用するフラグを設定し、かつ、</para>
        ///<para>レシピ実行時の1点目のZ座標を最終のアライメント測定の画像フリーズZ座標からのオフセットZ座標を設定します</para>
        /// </summary>
        /// 
        /// <param name="niParamNum">設定パラメーター数</param>
        /// <param name="niMode">
        /// 
        /// <para>0 : レシピ実行時のZ座標を現状のままにします。</para>
        /// <para>1 : レシピ実行時のZ座標を最終のアライメント測定の画像フリーズ座標からのオフセットにします</para>
        ///                      
        /// </param>
        /// <param name="ndZOffset">レシピ実行時の1点目のZ座標を最終のアライメント測定の画像フリーズ
        ///                          Z座標からのオフセットZ座標(mm単位)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetTFTMeasureZInputSupportPrm(int niParamNum, int niMode, double ndZOffset)
        {
            const string cstr_command = "/SetTFTMeasureZInputSupportPrm";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niParamNum.ToString());
            list_send_data_list.Add(niMode.ToString());
            list_send_data_list.Add(ndZOffset.ToString(strDoublePointFormat));
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTFTMeasureZInputSupportPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>レシピ実行時にZ座標の入力支援機能を使用するフラグを設定し、かつ、</para>
        ///<para>レシピ実行時の1点目のZ座標を最終のアライメント測定の画像フリーズZ座標からのオフセットZ座標を取得します</para>
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTFTMeasureZInputSupportPrm()
        {
            const string cstr_command = "/GetTFTMeasureZInputSupportPrm";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndFileName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///BNDファイル名を取得しま
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndFileName()
        {
            const string cstr_command = "/GetBndFileName";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndFileName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BNDファイル名を設定します
        /// </summary>
        /// 
        /// <param name="nstrBNDFilePath">BNDファイルパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndFileName(string nstrBNDFilePath)
        {
            const string cstr_command = "/SetBndFileName";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrBNDFilePath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BNDファイルが存在するフォルダパスを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndFilePath()
        {
            const string cstr_command = "/GetBndFilePath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BNDファイルが存在するフォルダパスを設定します
        /// </summary>
        /// 
        /// <param name="nstrBNDFolderPath">BNDファイルが存在するフォルダパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndFilePath(string nstrBNDFolderPath)
        {
            const string cstr_command = "/SetBndFilePath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrBNDFolderPath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndUseFlg
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///撓み補正機能の実行状態を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndUseFlg()
        {
            const string cstr_command = "/GetBndUseFlg";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndUseFlg
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 撓み補正機能の実行状態を設定します
        /// </summary>
        /// 
        /// <param name="niExecCalib">
        /// 
        /// <para>1：撓み補正機能を実行する </para>
        /// <para>0:実行しない</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndUseFlg(int niExecCalib )
        {
            const string cstr_command = "/SetBndUseFlg";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecCalib.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetUserGridCorrectFlag
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///ユーザーグリッド補正機能の実行状態を設定します
        /// </summary>
        /// 
        /// <param name="niExecGridCorrect">1：ユーザーグリッド補正機能を実行する 0:実行しない</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetUserGridCorrectFlag(int niExecGridCorrect)
        {
            const string cstr_command = "/SetUserGridCorrectFlag";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecGridCorrect.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetUserGridCorrectFlag
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///ユーザーグリッド補正機能の実行状態を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetUserGridCorrectFlag()
        {
            const string cstr_command = "/GetUserGridCorrectFlag";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetUserGridCorrectFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ユーザーグリッド補正ファイルのフルパスを設定します
        /// </summary>
        /// 
        /// <param name="nstrUserGridCorrectFilePath">ユーザーグリッド補正ファイルのフルパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetUserGridCorrectFilePath(string nstrUserGridCorrectFilePath)
        {
            const string cstr_command = "/SetUserGridCorrectFilePath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrUserGridCorrectFilePath);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetUserGridCorrectFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ユーザーグリッド補正ファイルのフルパスを取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetUserGridCorrectFilePath()
        {
            const string cstr_command = "/GetUserGridCorrectFilePath";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LoadImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	画像ファイルを読み込んでImage windowに表示します
        /// </summary>
        /// 
        /// <param name="nstrImageFilePath">画像ファイルのフルパス名</param>
        /// <param name="nstrMemo1">任意のメモ用文字列1（１バイト以上６３バイト以下)</param>
        /// <param name="nstrMemo2">任意のメモ用文字列2（１バイト以上６３バイト以下)</param>
        /// <param name="nstrMemo3">任意のメモ用文字列3（１バイト以上６３バイト以下)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int LoadImage(string nstrImageFilePath, string nstrMemo1, string nstrMemo2,string nstrMemo3)
        {
            const string cstr_command = "/LoadImage";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrImageFilePath);
                list_send_data_list.Add(nstrMemo1);
                list_send_data_list.Add(nstrMemo2);
                list_send_data_list.Add(nstrMemo3);

                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + "\t" + list_send_data_list[1] + "\t" +
                                   list_send_data_list[2] + "\t" + list_send_data_list[3] + "\t" +
                                    list_send_data_list[4] + "\n";

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region GetRemoteAutoRunStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	最後に実行した「/RunMacro、/Auto1、/RunTFTMeasure」の処理結果を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteAutoRunStatus()
        {
            const string cstr_command = "/GetRemoteAutoRunStatus";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region DriveFreeCheckerGetStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在の（主にハードディスク）空き容量チェック情報を取得します
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DriveFreeCheckerGetStatus()
        {
            const string cstr_command = "/DriveFreeCheckerGetStatus";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region MoveHandlingPosition
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	カメラをマテハン退避位置に移動します
        /// </summary>
        /// 
        /// <param name="niHandlingPosXY">X、Y軸の退避動作を行う（1）か否（0）か</param>
        /// <param name="niHandlingPosZ">Z軸の退避動作を行う（1）か否（0）か</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int MoveHandlingPosition(int niHandlingPosXY, int niHandlingPosZ)
        {
            const string cstr_command = "/MoveHandlingPosition";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niHandlingPosXY.ToString());
            list_send_data_list.Add(niHandlingPosZ.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetCylinderMode
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	（SMIC系（MGV5）の）シリンダーの使用の許可／不許可を設定します
        /// </summary>
        /// 
        /// <param name="niUseCylinderAccept">シリンダーの使用を許可する（1）か不許可にする（0）</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetCylinderMode(int niUseCylinderAccept)
        {
            const string cstr_command = "/SetCylinderMode";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niUseCylinderAccept.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMPCTime
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	（KEISOKU.EXEが動作している）PCの現在の日時を取得します
        /// </summary>
        /// 
        /// <param name="niTimeMode">
        /// 
        /// <para>システム時刻（世界標準時刻）で取得する場合は0</para>
        /// <para>ローカル時刻（そのPCの設定地域の時刻）で取得する場合は1</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetSCAMPCTime(int niTimeMode)
        {
            const string cstr_command = "/GetSCAMPCTime";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niTimeMode.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region SetSCAMPCTime
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	（KEISOKU.EXEが動作している）PCの現在の日時を更新します
        /// </summary>
        /// 
        /// <param name="niTimeMode">
        /// 
        /// <para>システム時刻（世界標準時刻）で取得する場合は0</para>
        /// <para>ローカル時刻（そのPCの設定地域の時刻）で取得する場合は1</para>
        /// 
        /// </param>
        /// <param name="niYear">年（西暦４桁)</param>
        /// <param name="niMonth">月（１月は1...１２月は12）</param>
        /// <param name="niDay">曜日（日曜日は0、月曜日は1、土曜日は6)</param>
        /// <param name="niDate">日</param>
        /// <param name="niHour">時</param>
        /// <param name="niMinute">分</param>
        /// <param name="niSecond">秒</param>
        /// <param name="nimmSecond">ミリ秒</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMPCTime(int niTimeMode, int niYear, int niMonth, int niDay, int niDate,
                                 int niHour, int niMinute, int niSecond, int nimmSecond)
        {
            const string cstr_command = "/SetSCAMPCTime";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niTimeMode.ToString());
            list_send_data_list.Add(niYear.ToString());
            list_send_data_list.Add(niMonth.ToString());
            list_send_data_list.Add(niDay.ToString());
            list_send_data_list.Add(niDate.ToString());
            list_send_data_list.Add(niHour.ToString());
            list_send_data_list.Add(niMinute.ToString());
            list_send_data_list.Add(niSecond.ToString());
            list_send_data_list.Add(nimmSecond.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region SetMeasurementResultCorrection
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	測定結果補正機能の制御パラメーターを設定します
        /// </summary>
        /// <param name="niMeasurementResultCorrectionEnable">	測定結果補正機能を有効にする(1)か否(0)か</param>
        /// <param name="nstrMeasurementResultCorrectionDataFilePath">
        /// 
        /// <para>(niMeasurementResultCorrectionEnableが1の場合)測定結果補正のデータセットファイルパス名</para>
        /// <para>niMeasurementResultCorrectionEnableが0の場合は（存在するファイルパス名である必要はないが）何らかの文字列をセットする必要がある</para>
        ///  
        /// </param>
        /// <param name="niOutputFileNameOnHeader">CSVファイルのヘッダー部分に補正ファイル名を出力する（1）か否（0）か</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------  
        public int SetMeasurementResultCorrection(int niMeasurementResultCorrectionEnable,
                                                  string nstrMeasurementResultCorrectionDataFilePath, 
                                                  int niOutputFileNameOnHeader )
        {
            const string cstr_command = "/SetMeasurementResultCorrection";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 アスキーコード
            try
            {
                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                if (niMeasurementResultCorrectionEnable == 0)
                {
                    nstrMeasurementResultCorrectionDataFilePath = "dummy";
                }

                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niMeasurementResultCorrectionEnable.ToString());
                list_send_data_list.Add(nstrMeasurementResultCorrectionDataFilePath);
                list_send_data_list.Add(niOutputFileNameOnHeader.ToString());

                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + ((char)i_separetor).ToString() +
                                   list_send_data_list[2] + ((char)i_separetor).ToString() + list_send_data_list[3] + "\n";

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region GetMeasurementResultCorrection
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	測定結果補正機能の制御パラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetMeasurementResultCorrection()
        {
            const string cstr_command = "/GetMeasurementResultCorrection";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetExImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	自動画像保存機能に関するパラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetExImageAutoSaveProp()
        {
            const string cstr_command = "/GetExImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAroundSearchPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	周回パターンマッチング機能に関するパラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAroundSearchPrm()
        {
            const string cstr_command = "/GetAroundSearchPrm";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetChamberStatusData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在のチャンバーのステータス情報を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetChamberStatusData()
        {
            const string cstr_command = "/GetChamberStatusData";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetAgingProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>「自動実行条件の付加パラメーター」のうち、エージング機能に関するパラメーターを取得します。</para>
        /// <para> このパラメーターは、「測定条件－自動実行条件－一般項目」の「エージング時間」および</para>
        /// <para>「“「エージング中止」ボタンを表示する“チェックボックス」の値です。</para>
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAgingProp()
        {
            const string cstr_command = "/GetAgingProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAgingProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>「自動実行条件の付加パラメーター」のうち、エージング機能に関するパラメーターを更新します。</para>
        /// <para> このパラメーターは、「測定条件－自動実行条件－一般項目」の「エージング時間」および</para>
        /// <para>「“「エージング中止」ボタンを表示する“チェックボックス」の値です。</para>
        /// </summary>
        /// 
        /// <param name="niAgingTime">エージング時間（秒単位）</param>
        /// <param name="niShowAgingStopButton">
        /// 
        /// <para>1：「エージング中止」ボタンを表示する</para>
        /// <para>0：「エージング中止」ボタンを表示しない</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetAgingProp(int niAgingTime, int niShowAgingStopButton )
        {
            const string cstr_command = "/SetAgingProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAgingTime.ToString());
            list_send_data_list.Add(niShowAgingStopButton.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWatchParamFileNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	パラメーターファイル監視リストに登録されているファイル数を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWatchParamFileNum()
        {
            const string cstr_command = "/GetWatchParamFileNum";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWatchParamFileInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	指定された監視対象ファイルの情報を取得します
        /// </summary>
        /// 
        /// <param name="niRegisterFileNoOnList">パラメーターファイル監視リストのX番目に登録されたファイル</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWatchParamFileInfo(int niRegisterFileNoOnList)
        {
            const string cstr_command = "/GetWatchParamFileInfo";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRegisterFileNoOnList.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetCurrentData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	指定された軸のモーター負荷率を取得します
        /// </summary>
        /// 
        /// <param name="niAxisNo">モーター負荷率を取得する軸番号(0～)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetCurrentData(int niAxisNo)
        {
            const string cstr_command = "/GetCurrentData";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxisNo.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region UserManager_Login
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	<para>指定したユーザー名、パスワードでSCAMのユーザーマネージャーにログインします。</para>
        ///	<para>既に別のユーザーがログイン済みの場合は、ユーザーの入れ替えが発生します</para>
        /// </summary>
        /// 
        /// <param name="nstrUserName">ユーザー名</param>
        /// <param name="nstrPassword">パスワード</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int UserManager_Login(string nstrUserName, string nstrPassword)
        {
            const string cstr_command = "/UserManager_Login";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 アスキーコード
            try
            {
                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrUserName);
                list_send_data_list.Add(nstrPassword);

                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + ((char)i_separetor).ToString() +
                                   list_send_data_list[2] + "\n";

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;                
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region UserManager_GetCurrentLoginUserName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在SCAMのユーザーマネージャーにログインされているユーザー名を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetCurrentLoginUserName()
        {
            const string cstr_command = "/UserManager_GetCurrentLoginUserName";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region UserManager_GetUserNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAMのユーザーマネージャーに現在登録されている社内エンジニア用以外のユーザー数を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetUserNum()
        {
            const string cstr_command = "/UserManager_GetUserNum";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region UserManager_GetUserList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	SCAMのユーザーマネージャーに現在登録されている社内エンジニア用以外のユーザーのユーザー名、パスワード、権限属性のリストを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetUserList()
        {
            const string cstr_command = "/UserManager_GetUserList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ReadCodeData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	2次元コードを読み取る
        /// </summary>
        /// 
        /// <param name="niCode">
        /// 
        /// <para>0：DataMatrix</para>
        /// <para>1：QR code</para>
        /// <para>2：Veri Code</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int ReadCodeData(int niCode)
        {
            const string cstr_command = "/ReadCodeData";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCode.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerGeneralParameterGet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	レシピメーカーによる自動測定に関するパラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RecipeMakerGeneralParameterGet()
        {
            const string cstr_command = "/RecipeMakerGeneralParameterGet";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerGeneralParameterSet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	レシピメーカーによる自動測定に関するパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="niParamNum">この引数を除く、のパラメーターの個数（現時点では最大4）</param>
        /// <param name="niP">
        /// 
        /// <para>P[0]:レシピメーカーによる「アライメント＋測定」の自動測定で「アライメントマクロ」の実行が終了したときに、</para>
        /// <para>     その時点で作成されたワーク座標系を基準座標系とする（1）か否（0）か</para>
        /// <para>---</para>
        /// <para>P[1]:レシピメーカーによる「アライメント＋測定」の自動測定で「アライメントマクロ」の実行が終了したときに、</para>
        /// <para>     その時点で作成されたワーク座標系を基準座標系とする際の軸反転方法について</para>
        /// <para>0：反転しない</para>
        /// <para>1：X軸を反転する</para>
        /// <para>2：Y軸を反転する</para>
        /// <para>---</para>
        /// <para>P[2]:レシピメーカーによる測定経路機能に関するパラメーター（大枠）について</para>
        /// <para>0：レシピ登録順</para>
        /// <para>1：TSPアルゴリズム</para>
        /// <para>2：選択したモデル経路</para>
        /// <para>---</para>
        /// <para>P[3]:レシピメーカーによる測定経路機能に関するパラメーター（P[2]=2のときのモデル経路）について</para>
        /// <para>0：縦方向</para>
        /// <para>1：横方向</para>
        /// <para>2：縦方向（２</para>
        /// <para>3：横方向（２）</para>
        /// <para>4：最近隣接方向 </para>
        /// <para>5：最短モデル経路</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RecipeMakerGeneralParameterSet(int niParamNum, int[] niP )
        {
            const string cstr_command = "/RecipeMakerGeneralParameterSet";
            List<string> list_send_data_list = new List<string>();

            if (niParamNum < 0 || niParamNum > 4)
            {
                return -5;
            }
            else if( niParamNum != niP.Count() )
            {
                return -5;
            }
            else
            {
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niParamNum.ToString());
                for (int i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    list_send_data_list.Add(niP[i_loop].ToString());
                }
            }

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerForSkipAreaParameterGet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// レシピメーカーによる自動測定におけるスキップエリア対応に関するパラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RecipeMakerForSkipAreaParameterGet()
        {
            const string cstr_command = "/RecipeMakerForSkipAreaParameterGet";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSaveHardcopyDialogboxParam
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ハードコピー保存に関するパラメーターを取得します
        /// </summary>
        /// 
        /// <param name="niDialogBoxType">対象ダイアログボックス（1：ソフトウェアジョイスティック、2：マップ表示</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSaveHardcopyDialogboxParam(int niDialogBoxType )
        {
            const string cstr_command = "/GetSaveHardcopyDialogboxParam";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDialogBoxType.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetStopAutoMeasurementProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	「自動実行条件の付加パラメーター」のうち、測定エラー数などの条件で自動実行を中止することに関するパラメーターを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetStopAutoMeasurementProp()
        {
            const string cstr_command = "/GetStopAutoMeasurementProp";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetLightStatusEx
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	指定した顕微鏡に属する照明装置の状態を取得します。このコマンドは既存の「LightStaus」コマンドの拡張版です
        /// </summary>
        /// 
        /// <param name="niLightType">
        /// 
        /// <para>どの顕微鏡に属する照明装置の状態を取得するか</para>
        /// <para>0：メイン顕微鏡に属する照明装置（非デュアル顕微鏡パラメーターの場合を含む）</para>
        /// <para>1：サブ顕微鏡に属する照明装置</para>
        /// <para>2：現在選択されているカメラ（倍率）が属する顕微鏡に属する照明装置</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetLightStatusEx(int niLightType)
        {
            const string cstr_command = "/GetLightStatusEx";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetLightONWhenFreezeEtcFunctionOnOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	<para>自動実行時は画像取り込み・コントラスト法AFなど必要なときのみ照明する機能に関する</para>
        ///	<para>パラメーターのうち、機能を作動させるか否かの設定を取得します</para>
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetLightONWhenFreezeEtcFunctionOnOff()
        {
            const string cstr_command = "/GetLightONWhenFreezeEtcFunctionOnOff";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetLightONWhenFreezeEtcFunctionOnOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	自動実行時は画像取り込み・コントラスト法AFなど必要なときのみ照明する機能に関するパラメーターのうち、機能を作動させるか否かの設定を設定します
        /// </summary>
        /// 
        /// <param name="niParam">
        /// 
        /// <para>0：自動実行時は画像取り込み・コントラスト法AFなど必要なときのみ照明する機能は無効である</para>
        /// <para>1：自動実行時は画像取り込み・コントラスト法AFなど必要なときのみ照明する機能は有効である。</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetLightONWhenFreezeEtcFunctionOnOff(int niParam)
        {
            const string cstr_command = "/SetLightONWhenFreezeEtcFunctionOnOff";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niParam.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SelectCamera
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>カメラ（倍率）切り替えを行います。</para>
        /// <para>このコマンドはSCAMの顕微鏡システム（デュアル顕微鏡パラメーター／高低倍パラメーター／（非デュアル）ズーム・レボルバーパラメーター）によらず </para>   
        /// <para>SCAMのルールに従ったカメラ（倍率）番号を指定することで処理を行います</para>
        /// </summary>
        /// 
        /// <param name="niCameraNo">
        /// 
        /// <para>切り替えるカメラ（倍率）番号</para>
        /// <para>＜デュアル顕微鏡パラメーターの場合＞</para>
        /// <para>メイン顕微鏡側のカメラ数をM、サブ顕微鏡側のカメラ数をSとすると0（メイン顕微鏡側最低倍率）,..,M-1（メイン顕微鏡側最高倍率）,M（サブ顕微鏡側最低倍率）,…,M+S-1（サブ顕微鏡側最高倍率）</para>
        /// <para>＜高低倍パラメーターの場合＞</para>
        /// <para>0：高倍、1：低倍</para>
        /// <para>＜（非デュアル）ズーム・レボルバーパラメーターの場合＞</para>
        /// <para>ズーム・レボルバー倍率数をMとすると、</para>
        /// <para>0（最低倍率）,..,M-1（最高倍率）</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SelectCamera(int niCameraNo)
        {
            const string cstr_command = "/SelectCamera";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region TableMoveNoWaitStart
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>XY軸テーブル移動を開始します。このコマンドでは開始のみ行います。</para>
        ///<para>移動完了の確認にはTableMoveNoWaitCheckコマンドを使用します</para>
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:絶対値移動　false:相対値移動</param>
        /// <param name="ndX">X座標</param>
        /// <param name="ndY">Y座標</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int TableMoveNoWaitStart(bool nbABSMove, double ndX, double ndY)
        {
            const string cstr_command = "/TableMoveNoWaitStart";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  絶対値移動
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  相対値移動
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat)); 
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region TableMoveNoWaitCheck
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	TableMoveNoWaitStartコマンドで移動開始したXY軸テーブル移動の状態を取得します
        /// </summary>
        /// 
        /// <param name="niBeforeCheckProcess">
        /// 
        /// <para>チェック前の待機処理</para>
        /// <para>1:待機する、0:待機しない</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int TableMoveNoWaitCheck(int niBeforeCheckProcess)
        {
            const string cstr_command = "/TableMoveNoWaitCheck";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niBeforeCheckProcess.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ReadRawCount
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	指定軸のカウント生値を取得します
        /// </summary>
        /// 
        /// <param name="niAxisNo">
        /// 
        /// <para>軸番号</para>
        /// <para>0:X</para>
        /// <para>1:Y</para>
        /// <para>2:Y2</para>
        /// <para>3:Z</para>
        /// <para>4:XP</para>
        /// <para>5:YP</para>
        /// <para>6:Tracker</para>
        /// <para>7:tiny_y</para>
        /// <para>それ以外:X</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int ReadRawCount(int niAxisNo)
        {
            const string cstr_command = "/ReadRawCount";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxisNo.ToString());
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ConvertWorkPos
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	指定ワーク座標を機械座標に変換します
        /// </summary>
        /// 
        /// <param name="niConvertType">
        /// 
        /// <para>変換種類</para>
        /// <para>0:ワーク座標->機械座標</para>
        /// <para>1:ワーク座標->テーブル補正を行わないパルス値</para>
        /// 
        /// </param>
        /// <param name="ndX">X座標(ワーク座標)</param>
        /// <param name="ndY">Y座標(ワーク座標)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int ConvertWorkPos(int niConvertType, double ndX, double ndY)
        {
            const string cstr_command = "/ConvertWorkPos";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niConvertType.ToString());
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  リストから送信文字列を作成
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTableSpeed
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在のテーブルスピードを取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTableSpeed()
        {
            const string cstr_command = "/GetTableSpeed";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTableSpeed
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	通常測定の移動スピード(pulse/sec)を設定します
        /// </summary>
        /// 
        /// <param name="niParamNum">
        /// 
        /// <para>パラメーター数</para>
        /// <para>Sinto LA、変位計なし…1を設定する(P2に設定)</para>
        /// <para>Sinto LA…2を設定する(P2、P3に設定)</para>
        /// <para>変位計…4を設定する(P2、P3、P4、P5に設定)</para>
        /// 
        /// </param>
        /// <param name="ndP">
        /// 
        /// ndP[0]:P2  通常測定用スピード
        /// ndP[1]:P3  変位計測定用(平均16回)スピード
        /// ndP[2]:P4  変位計測定用(平均128回)スピード
        /// ndP[3]:P5  変位計測定用(平均2回)スピード
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetTableSpeed(int niParamNum, double [] ndP )
        {
            int i_loop;
            const string cstr_command = "/SetTableSpeed";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 アスキーコード
            try
            {
                if (niParamNum != ndP.Count())
                {
                    return -5;
                }
                else if (niParamNum < 1 || niParamNum > 4)
                {
                    return -5;
                }
                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niParamNum.ToString());

                for (i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    list_send_data_list.Add(ndP[i_loop].ToString(strDoublePointFormat));
                }
                //  リストから送信文字列を作成
                string str_send_string = "";

                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1];
                for (i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    str_send_string += ((char)i_separetor).ToString();
                    str_send_string += list_send_data_list[2 + i_loop];
                }
                str_send_string += "\n";

                int i_ret;

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region GetZSoftwareLimitList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Z軸ソフトウェアリミット情報の一覧を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetZSoftwareLimitList()
        {
            const string cstr_command = "/GetZSoftwareLimitList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetZSoftwareLimitCurrentName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Z軸ソフトウェアリミットの名称を設定します。登録されているリミットデータと同じ名称が存在する場合は、そのデータに変更されます
        /// </summary>
        /// 
        /// <param name="nstrZSoftLimitName">
        /// 
        /// <para>	Z軸ソフトウェアリミットの名称</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetZSoftwareLimitCurrentName(string nstrZSoftLimitName)
        {
            const string cstr_command = "/SetZSoftwareLimitCurrentName";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrZSoftLimitName);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetZSoftwareLimitCurrentName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	現在のZ軸ソフトウェアリミットの名称を取得します
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetZSoftwareLimitCurrentName()
        {
            const string cstr_command = "/GetZSoftwareLimitCurrentName";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	自動画像保存機能に関するパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="nstrImageAutoSavePropArray">
        /// 
        /// <para>自動画像保存パラメーター配列</para>
        /// <para>nstrImageAutoSavePropArray=Pとして説明</para>
        /// <para>----</para>
        /// <para>P[0]:自動画像保存機能が有効（1）か否（0）か</para>
        /// <para>P[1]:0:エラー箇所、非エラー箇所ともに保存、1:エラー箇所のみ保存、2:非エラー箇所のみ保存</para>
        /// <para>P[2]:再測定が発生したときの保存方法について</para>
        /// <para>0:再測定前の画像は削除、1:再測定前の画像も残す</para>
        /// <para>（どちらの場合も再測定時の画像は保存される）</para>
        /// <para>P[3]:同一ファイル名の画像に関して</para>
        /// <para>0:上書きしない（プログラムが定義する別名で保存）、1:上書きする</para>
        /// <para>P[4]:0:自動画像保存高速モード無効、1:自動画像保存高速モード有効</para>
        /// <para>P[5]:0:JPEG形式で保存、1:BMP形式で保存</para>
        /// <para>P[6]:JPEG形式で保存する際の品質（1～100で指定）</para>
        /// <para>P[7]:画像保存先（ベースディレクトリー）</para>
        /// <para>P[8]:画像保存先（ディレクトリー１）、次の内、いずれかの文字列が返される</para>
        /// <para>「測定結果ファイル名」、「測定マクロファイル名」、「開始日時」、「任意」、「なし」</para>
        /// <para>P[9]:P[8]が「任意」の場合のディレクトリー名</para>
        /// <para>P[10]:画像保存先（ディレクトリー１）に関する日時フォーマットを示す文字列</para>
        /// <para>P[11]:画像保存先（ディレクトリー２）、次の内、いずれかの文字列が返される</para>
        /// <para>「測定結果ファイル名」、「測定マクロファイル名」、「開始日時」、「任意」、「なし」</para>
        /// <para>P[12]:P[11]が「任意」の場合のディレクトリー名</para>
        /// <para>P[13]:画像保存先（ディレクトリー２）に関する日時フォーマットを示す文字列</para>
        /// <para>P[14]:本機能における画像削除方法に関して  0:マニュアルモード、1:オートモード</para>
        /// <para>P[15]:画像保存日数を1～365の値で指定</para>
        /// <para>P[16]:オートモードで画像削除する際、チェックを行う周期日数を1～32767の値で指定</para>
        /// <para>P[17]:オートモードで画像削除する際、ユーザーへの確認ダイアログボックスを表示する（1）か否（0）かを指定</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetImageAutoSaveProp(string[] nstrImageAutoSavePropArray)
        {
            const string cstr_command = "/SetImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  引数の数が一致しなければエラー
            if (nstrImageAutoSavePropArray.Count() != 18)
            {
                return -1;
            }
            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            foreach (string str in nstrImageAutoSavePropArray)
            {
                list_send_data_list.Add(str);
            }
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetExImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	自動画像保存機能に関するパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="nstrImageAutoSavePropArray">
        /// 
        /// <para>自動画像保存パラメーター配列</para>
        /// <para>nstrImageAutoSavePropArray=Pとして説明</para>
        /// <para>----</para>
        /// 
        /// <para>P[0]:設定するパラメーターの個数（今回のバージョンでは最大28）</para>
        /// <para>----</para>
        /// <para>P[1]:自動画像保存機能が有効（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[2]:保存する画像に関して</para>
        /// <para>0：エラー箇所、非エラー箇所ともに保存</para>
        /// <para>1：エラー箇所のみ保存</para>
        /// <para>2：非エラー箇所のみ保存</para>
        /// <para>----</para>
        /// <para>P[3]:再測定が発生したときの保存方法に関して、0：再測定前の画像は削除、1：再測定前の画像も残す</para>
        /// <para>----</para>
        /// <para>P[4]:同一ファイル名の画像に関して、0：上書きしない（プログラムが定義する別名で保存）、1：上書きする</para>
        /// <para>----</para>
        /// <para>P[5]:自動画像保存高速モードが有効（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[6]:画像ファイルのフォーマット  0：JPEG形式、1：BMP形式</para>
        /// <para>----</para>
        /// <para>P[7]:JPEG形式で保存する際の品質（1～100で指定）</para>
        /// <para>----</para>
        /// <para>P[8]:画像保存先（ベースディレクトリー）</para>
        /// <para>----</para>
        /// <para>P[9]:画像保存先（ディレクトリー１）、①～⑤のいずれかを指定する。</para>
        /// <para>①測定結果ファイル名</para>
        /// <para>②測定マクロファイル名</para>
        /// <para>③開始日時</para>
        /// <para>④任意</para>
        /// <para>⑤なし</para>
        /// <para>----</para>
        /// <para>P[10]:P[9]が④の場合のディレクトリー名</para>
        /// <para>----</para>
        /// <para>P[11]:画像保存先（ディレクトリー１）に関する日時フォーマットを示す文字列</para>
        /// <para>----</para>
        /// <para>P[12]:画像保存先（ディレクトリー２）、①～⑤のいずれかを指定する。</para>
        /// <para>①測定結果ファイル名</para>
        /// <para>②測定マクロファイル名</para>
        /// <para>③開始日時</para>
        /// <para>④任意</para>
        /// <para>⑤なし</para>
        /// <para>----</para>
        /// <para>P[13]:P[12]が④の場合のディレクトリー名</para>
        /// <para>----</para>
        /// <para>P[14]:画像保存先（ディレクトリー２）に関する日時フォーマットを示す文字列</para>
        /// <para>----</para>
        /// <para>P[15]:本機能における画像削除方法に関して、0：マニュアルモード、1：オートモード</para>
        /// <para>----</para>
        /// <para>P[16]:画像保存日数（1～365）</para>
        /// <para>----</para>
        /// <para>P[17]:オートモードで画像を削除する際のチェックを行う周期日数（1～32767）</para>
        /// <para>----</para>
        /// <para>P[18]:オートモードで画像を削除する際、ユーザーへの確認ダイアログボックスを表示する（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[19]:画像ファイルにグラフィックスを含める方法について、</para>
        /// <para>0：グラフィックスを含めない画像のみを保存する。</para>
        /// <para>1：グラフィックスを含めた画像のみを保存する。</para>
        /// <para>2：グラフィックスを含めない／含めた画像ともに保存する</para>
        /// <para>----</para>
        /// <para>P[20]:画像ファイルに測定条件を記録する（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[21]:測定リトライ機能が作動したときにリトライ時の画像を保存する（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[22]:自動画像保存ファイル名の命名規則の拡張において、「標準のファイル名」と「拡張成分」の連動方法に関して、</para>
        /// <para>1：「標準のファイル名」のみ</para>
        /// <para>2：「拡張成分」のみ</para>
        /// <para>3：「標準のファイル名_拡張成分」</para>
        /// <para>4：「拡張成分_標準のファイル名」</para>
        /// <para>----</para>
        /// <para>P[23]:自動画像保存ファイル名の命名規則の拡張において、「拡張成分」の構成要素を次の1～8を組み合わせた文字列で返す。セパレーター文字列はASCIIコード0x02の文字とする。なお、構成要素が存在しない場合はUndefinedを返す</para>
        /// <para>1：測定の種類（アライメントと測定）と種類ごとの連番</para>
        /// <para>2：測定に使用したカメラ（倍率）の名称</para>
        /// <para>3：測定に使用した方法選択アイコン（SOKMAC）の番号</para>
        /// <para>4：測定に使用した方法選択アイコン（SOKMAC）の名称（最大31バイト）</para>
        /// <para>5：測定に使用した照明の名称</para>
        /// <para>6：測定に使用した照明の設定照度</para>
        /// <para>7：測定に使用した画像フィルターの方法</para>
        /// <para>8：測定結果に対するコメント（最大31バイト）</para>
        /// <para>----</para>
        /// <para>P[24]:「測定結果による画像保存条件を適用する方法」に関して、</para>
        /// <para>1：すべて適用する。</para>
        /// <para>2：最後のアライメントまで適用する。「最後のアライメント」とは次のように定義する。</para>
        /// <para>ⅰ：レシピメーカー以外で自動実行した場合</para>
        /// <para>最後に登場したアライメントのSOKMACが使用された測定結果番号</para>
        /// <para>ⅱ：レシピメーカーで自動実行した場合</para>
        /// <para>「アライメントマクロ」として実行された最後の測定結果番号（アライメントマクロがアライメントのSOKMACであるかは評価しない）</para>
        /// <para>3：最後のアライメントの次の測定から適用する。</para>
        /// <para>4：指定した測定結果番号について適用する。</para>
        /// <para>5：指定したSOKMAC番号について適用する。</para>
        /// <para>----</para>
        /// <para>P[25]:P[24]にある測定結果番号を表わす文字列（カンマ区切りによる複数指定、ハイフンによる連続指定）に先頭と末尾をダブルコーテーションマークを付加したもの。例えば、当該文字列が何もない場合は「””」となる（「」はない）。</para>
        /// <para>----</para>
        /// <para>P[26]:P[24]にあるSOKMAC番号を表わす文字列（カンマ区切りによる複数指定、ハイフンによる連続指定）に先頭と末尾をダブルコーテーションマークを付加したもの。例えば、当該文字列が何もない場合は「””」となる（「」はない）。</para>
        /// <para>----</para>
        /// <para>P[27]:適用条件を満たさない場合に、測定エラーでない画像を保存する（1）か否（0）か</para>
        /// <para>----</para>
        /// <para>P[28]:適用条件を満たさない場合に、測定エラーの画像を保存する（1）か否（0）か</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetExImageAutoSaveProp(string[] nstrImageAutoSavePropArray)
        {
            int i_wrk;
            int i_loop;
            const string cstr_command = "/SetExImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 アスキーコード
            try
            {
                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  引数の数が一致しなければエラー
                if (nstrImageAutoSavePropArray.Count() == 0)
                {
                    return -1;
                }
                if (int.TryParse(nstrImageAutoSavePropArray[0], out i_wrk) == false)
                {
                    return -1;
                }
                else
                {
                    if (i_wrk != nstrImageAutoSavePropArray.Count() - 1)
                    {
                        return -1;
                    }
                }

                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                foreach (string str in nstrImageAutoSavePropArray)
                {
                    list_send_data_list.Add(str);
                }

                //  リストから送信文字列を作成
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " ";
                for (i_loop = 0; i_loop < i_wrk + 1; i_loop++)
                {
                    if (i_loop != i_wrk)
                    {
                        str_send_string += list_send_data_list[i_loop + 1] + ((char)i_separetor).ToString();
                    }
                    else
                    {
                        str_send_string += list_send_data_list[i_loop + 1] + "\n";
                    }
                }

                //  送信
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	送ったコマンド文字列を覚えておく
					m_strLastSentNormalCommand = MethodBase.GetCurrentMethod().Name;
				}
                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

		#region RunRapidMode
		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// RapidModeを実行します
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">レシピファイル名(フルパス)</param>
		/// <param name="nstrLayerName">レイヤー名</param>
		/// <param name="nstrComment">コメント</param>
		/// <param name="nstrResultPath">測定結果ファイル名(フルパス)</param>
		/// <param name="nstrNameListArray">測定を実行する名前リスト（名前1[sep]名前2[sep]･･･名前N[sep]）</param>
		/// 
		/// <returns>
		///    0:正常
		///   -1:ソケット通信的に送信失敗
		///   -2:文字列オーバー
		///   -3:ソケットがオープンしていない(有効でない)
		///   -4:2度送りエラー
		///   -10:それ以外のエラー
		/// </returns>
		/// ----------------------------------------------------------------------------------------
		public int RunRapidMode( string nstrRecipePath, string nstrLayerName, string nstrComment, string nstrResultPath,
								 string[] nstrNameListArray)
		{
			const string cstr_command = "/RunRapidMode";
			List<string> list_send_data_list = new List<string>();
			int i_separetor = 1;    //  0x01 アスキーコード
			string str_name_list = "";

			try
			{
				//  送信コマンド+引数をリストに順番に格納
				list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrRecipePath);
				list_send_data_list.Add(nstrLayerName);
				list_send_data_list.Add(nstrComment);
                list_send_data_list.Add(nstrResultPath);
				foreach (string Name in nstrNameListArray)
				{
					str_name_list = str_name_list + Name + ((char)i_separetor).ToString();
				}
				list_send_data_list.Add(str_name_list);
				//  リストから送信文字列を作成
				//  送信
				return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
			}
			catch (Exception ex)
			{
				string str_err;
				str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
				OutputErrorLog(str_err);
				return -10;
			}
		}
		#endregion

		#region GetRapidModeNameList
		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// 	レシピファイルに設定されているRapidModeの名前リストを取得します
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">レシピファイル名(フルパス)</param>
		/// 
		/// <returns>
		///    0:正常
		///   -1:ソケット通信的に送信失敗
		///   -2:文字列オーバー
		///   -3:ソケットがオープンしていない(有効でない)
		///   -4:2度送りエラー
		///   -10:それ以外のエラー
		/// </returns>
		/// ---------------------------------------------------------------------------------------- 
		public int GetRapidModeNameList(string nstrRecipePath)
		{
			const string cstr_command = "/GetRapidModeNameList";
			List<string> list_send_data_list = new List<string>();

			//  送信コマンド+引数をリストに順番に格納
			list_send_data_list.Add(cstr_command);
			list_send_data_list.Add(nstrRecipePath);
			//  送信
			return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
		}
		#endregion

		#region GetRapidModeNameData
		/// ---------------------------------------------------------------------------------------- 
		/// <summary>
		/// 	指定した名前の測定位置リスト、コメント情報を取得します
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">レシピファイル名(フルパス)</param>
		/// <param name="nstrName">名前</param>
		/// 
		/// <returns>
		///    0:正常
		///   -1:ソケット通信的に送信失敗
		///   -2:文字列オーバー
		///   -3:ソケットがオープンしていない(有効でない)
		///   -4:2度送りエラー
		///   -10:それ以外のエラー
		/// </returns>
		/// ---------------------------------------------------------------------------------------- 		
		public int GetRapidModeNameData(string nstrRecipePath, string nstrName)
		{
			const string cstr_command = "/GetRapidModeNameData";
			List<string> list_send_data_list = new List<string>();

			//  送信コマンド+引数をリストに順番に格納
			list_send_data_list.Add(cstr_command);
			list_send_data_list.Add(nstrRecipePath);
			list_send_data_list.Add(nstrName);
			//  送信
			return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
		}
		#endregion

        #region SetRemeasureProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	オート１、リモートでの自動実行に対する再測定に関するパラメーターを設定します
        /// </summary>
        /// 
        /// <param name="nstrRemeasurePropArray">
        /// <para>自動実行条件パラメーター配列</para>
        /// <para>nstrRemeasurePropArray=Pとして説明</para>
        /// <para>----</para>
        /// <para>P[1]:アラインメントの再測定を行う（1）か否（0）か</para>
        /// <para>P[2]:アラインメント以外の再測定を行う（1）か否（0）か</para>
        /// <para>P[3]:公差判定エラーを再測定の対象とする（1）か否（0）か</para>
        /// <para>P[4]:自動実行終了後の再測定を行う（1）か否（0）か</para>
        /// <para>P[5]:測定エラーが発生するごとに再測定を行う（1）か否（0）か</para>
        /// <para>P[6]:測定エラーが発生するごとに再測定を行うときの対象となる測定</para>
        /// <para>      0：すべての測定,1：特定の測定結果番号より前のものすべて</para>
        /// <para>P[7]:測定エラーが発生するごとに再測定を行うときの対象となる測定が「特定の測定結果番号より前のものすべて」の場合の測定結果番号</para>
        /// <para>P[8]:自動実行条件ダイアログで、再測定項目の設定を許可する（1）か否（0）か</para>
        /// <para>P[9]:再測定時に測定ポイントごとに確認(OKボタン)操作をする（1）か否（0）か</para>
        /// <para>P[10]:測定エラー箇所がなくても再測定を行う（1）か否（0）か</para>
        /// <para>P[11]:自動実行中に再測定箇所の追加を行う（1）か否（0）か</para>
        /// <para>P[12]:「自動ステップ実行」の対象とする測定について、</para>
        /// <para>      0：すべての測定</para>
        /// <para>      1：特定の測定結果番号より前のものすべて</para>
        /// <para>      2：アライメントに関わる測定</para>
        /// <para>      3：指定した測定アイコン</para>
        /// <para>P[13]:P12=1のときの測定結果番号</para>
        /// <para>P[14]:P12=2のとき、アライメント測定のSOKMAC自身を自動ステップ実行の対象とする（1）か否（0）か</para>
        /// <para>P[15]:P12=2のとき、アライメント測定のSOKMACがメモリー引用している測定を自動ステップ実行の対象とする（1）か否（0）か</para>
        /// <para>P[16]:自動実行終了後の再測定モード開始時に「再測定しますか？」ダイアログボックスを表示する（1）か否（0）か</para>
        /// <para>P[17]:P16=1のとき、すべての再測定箇所がオートモードを含めて行う場合は除外する（1）か否（0）か</para>
        /// <para>P[18]:自動実行終了後の再測定モード開始時に再測定箇所設定ウィンドウを表示する（1）か否（0）か</para>
        /// <para>P[19]:P18=1のとき、すべての再測定箇所がオートモードを含めて行う場合は除外する（1）か否（0）か</para>
        /// <para>P[20]:自動実行時の測定結果が公差判定エラーの測定箇所の再測定の動作モードについて、</para>
        /// <para>      1：標準モード,2：オートモード,3：オートモード→標準モード</para>
        /// <para>P[21]:P20=2または3のとき、オートモードが作動する際の最大リトライ回数</para>
        /// <para>P[22]:自動実行時の測定結果が公差判定エラー以外の測定エラーの測定箇所の再測定の動作モードについて、</para>
        /// <para>      1：標準モード,2：オートモード,3：オートモード→標準モード</para>
        /// <para>P[23]:P22=2または3のとき、オートモードが作動する際の最大リトライ回数</para>
        /// <para>P[24]:自動実行時の測定結果が測定エラーでない測定箇所の再測定の動作モードについて、</para>
        /// <para>      1：標準モード,2：オートモード,3：オートモード→標準モード</para>
        /// <para>P[25]:自動実行終了後の再測定モード開始時に「再測定しますか？」ダイアログボックスが表示されたとき、所定時間経過後に自動応答を行う（1）か否（0）か</para>
        /// <para>P[26]:P25=1のときの所定時間（秒単位）</para>
        /// <para>P[27]:P25=1のとき所定時間が経過したときに自動応答を行う際の処理について、</para>
        /// <para>      0：「はい」を押下した処理を行う,1：「いいえ」を押下した処理を行う</para>
        /// <para>P[28]:再測定箇所設定ウィンドウが表示されたとき、かつ、再測定箇所に測定エラーを含むとき、所定時間経過後に自動応答を行う（1）か否（0）か</para>
        /// <para>P[29]:P28=1のときの所定時間（秒単位）</para>
        /// <para>P[30]:P28=1のとき所定時間が経過したときに自動応答を行う際の処理について、</para>
        /// <para>      0：「OK」を押下した処理を行う,1：「キャンセル」を押下した処理を行う</para>
        /// <para>P[31]:再測定箇所設定ウィンドウが表示されたとき、かつ、再測定箇所に測定エラーを含まないとき、所定時間経過後に自動応答を行う（1）か否（0）か</para>
        /// <para>P[32]:P31=1のときの所定時間（秒単位）</para>
        /// <para>P[33]:P31=1のとき所定時間が経過したときに自動応答を行う際の処理について、</para>
        /// <para>      0：「OK」を押下した処理を行う,1：「キャンセル」を押下した処理を行う</para>
        /// <para>P[34]:自動ステップ実行を行う（1）か否（0）か</para>
        /// <para>P[35]:スキップエリアエラーを再測定の対象とする（1）か否（0）か</para>
        /// <para>P[36]:P12=3のとき、対象とするSOKMAC番号のリストリストを “N1,N2,…” の形式で記述する（先頭と末尾のダブルコーテーションマークを含む）</para>
        /// <para>P[37]:測定エラーと公差判定エラーの合計数が設定した個数による再測定の実行の有無を決定することが有効である（1）か否（0）か</para>
        /// <para>P[38]:測定エラーと公差判定エラーの合計数が設定した個数による再測定の実行の有無が有効な場合の動作モードについて、</para>
        /// <para>      1：設定数より小さい場合再測定を行わない（設定した個数以上の場合再測定を行う）</para>
        /// <para>      2：設定数より大きい場合再測定を行わない（設定した個数以下の場合再測定を行う）</para>
        /// <para>P[39]:測定エラーと公差判定エラーの合計数が設定した個数による再測定の実行の有無を決定する場合の設定数</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemeasureProp(string[] nstrRemeasurePropArray)
        {
            const string cstr_command = "/SetRemeasureProp";
            List<string> list_send_data_list = new List<string>();
            try
            {
                int i_wrk;

                //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  引数の数が一致しなければエラー
                if (nstrRemeasurePropArray.Count() == 0)
                {
                    return -1;
                }
                if (int.TryParse(nstrRemeasurePropArray[0], out i_wrk) == false)
                {
                    return -1;
                }
                else
                {
                    if (i_wrk != nstrRemeasurePropArray.Count() - 1)
                    {
                        return -1;
                    }
                }
                
                //  送信コマンド+引数をリストに順番に格納
                list_send_data_list.Add(cstr_command);
                foreach (string str in nstrRemeasurePropArray)
                {
                    list_send_data_list.Add(str);
                }
                
                //  送信
                return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -10;
            }
        }
        #endregion

        #region GetSettingsSubComment
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// サブコメントに関する設定情報を取得する。
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsSubComment()
        {
            const string cstr_command = "/GetSettingsSubComment";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSettingsOutputCondition
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 出力条件に関する設定情報を取得する。
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsOutputCondition()
        {
            const string cstr_command = "/GetSettingsOutputCondition";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSettingsJudgement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 公差判定に関する設定情報を取得する。
        /// </summary>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsJudgement()
        {
            const string cstr_command = "/GetSettingsJudgement";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region PMSFeedbackRun
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 蒸着機フィードバックデータファイルを作成する
        /// </summary>
        /// 
        /// <param name="nstrCSVFileName">フィードバックデータファイルの作成に使用するCSVファイル名</param>
        /// <param name="nstrSaveBaseFilePath">出力するフィードバックデータファイル名のベースとなるファイル名</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int PMSFeedbackRun(List<string> nstrCSVFileName, string nstrSaveBaseFilePath)
        {
            const string cstr_command = "/PMSFeedbackRun";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            foreach (string str in nstrCSVFileName)
            {
                string value = str;
                if (value != nstrCSVFileName.Last())
                {
                    value += ((char)0x01).ToString();
                }
                else
                {
                    value += ((char)0x02).ToString();
                }
                list_send_data_list.Add(value);
            }
            list_send_data_list.Add(nstrSaveBaseFilePath);

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region MakeRecipeUsingFileList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 蒸着機フィードバックデータファイルを作成する
        /// </summary>
        /// 
        /// <param name="nstrRecipeFileName">レシピメーカー用レシピファイル名</param>
        /// <param name="nstrListFilePath">レシピファイルを実行するために必要なファイルのリストを出力するテキストファイル名</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int MakeRecipeUsingFileList(string nstrRecipeFileName, string nstrListFilePath)
        {
            const string cstr_command = "/MakeRecipeUsingFileList";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrRecipeFileName + ((char)0x01).ToString());
            list_send_data_list.Add(nstrListFilePath);

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region CreateUserGridCorrectFile
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ユーザーグリッド補正ファイルを作成する
        /// </summary>
        /// 
        /// <param name="nstrCheckGUI">GUIでのチェックを行う(1)か否(0)か</param>
        /// <param name="nstrMeasurementResultFilePaths">測定結果CSVファイルのフルパス(複数の場合あり)</param>
        /// <param name="nstrStandardValueFilePath">基準値ファイルのフルパス</param>
        /// <param name="nstrUseDesignValue">公差判定設計値を利用する(1)かしない(0)か</param>
        /// <param name="nstrCorrectFilePath">補正ファイルのフルパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int CreateUserGridCorrectFile(string nstrCheckGUI, string nstrMeasurementResultFilePaths,
                string nstrStandardValueFilePath, string nstrUseDesignValue, string nstrCorrectFilePath)
        {
            const string cstr_command = "/CreateUserGridCorrectFile";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrCheckGUI);
            list_send_data_list.Add(nstrMeasurementResultFilePaths);
            list_send_data_list.Add(nstrStandardValueFilePath);
            list_send_data_list.Add(nstrUseDesignValue);
            list_send_data_list.Add(nstrCorrectFilePath);

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AppendOrthogonalCorrection_RunCorrection
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 動的直交度補正値を求める測定を行う
        /// </summary>
        /// 
        /// <param name="nstrOutputFilePath">処理結果を出力するファイルのフルパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int AppendOrthogonalCorrection_RunCorrection(string nstrOutputFilePath)
        {
            const string cstr_command = "/AppendOrthogonalCorrection_RunCorrection";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrOutputFilePath);

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AppendOrthogonalCorrection_RunConfirmation
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 動的直交度補正値の妥当性を確認する測定を行う
        /// </summary>
        /// 
        /// <param name="nstrOutputFilePath">処理結果を出力するファイルのフルパス</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int AppendOrthogonalCorrection_RunConfirmation(string nstrOutputFilePath)
        {
            const string cstr_command = "/AppendOrthogonalCorrection_RunConfirmation";
            List<string> list_send_data_list = new List<string>();

            //  送信コマンド+引数をリストに順番に格納
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrOutputFilePath);

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #endregion

        #region MakeSimpleConnectedImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 単純な連結画像を作成する。
        /// </summary>
        /// 
        /// <param name="nintXDirectionImageNum">X方向の画像ファイル数</param>
        /// <param name="nintYDirectionImageNum">Y方向の画像ファイル数</param>
        /// <param name="nstrInputImageFileNameArray">入力画像ファイルフルパス名の配列</param>
        /// <param name="nstrOutputImageFileName">出力画像ファイルフルパス名</param>
        /// <param name="nbMakeJPG">出力画像ファイルがJPEGである(true)か否(false)か</param>
        /// <param name="nintJPGQuality">出力画像ファイルがJPEGである場合の品質パラメーター(1以上100以下で通常80)</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int MakeSimpleConnectedImage( int nintXDirectionImageNum, int nintYDirectionImageNum, List<string> nstrInputImageFileNameArray, string nstrOutputImageFileName, bool nbMakeJPG, int nintJPGQuality )
        {
            const string cstr_command = "/MakeSimpleConnectedImage";
            List<string> list_send_data_list = new List<string>();
            string str_sepa_0x01 = ( ( char )0x01 ).ToString();
            string str_wrk;

            //  送信コマンド+引数をリストに格納する。
            list_send_data_list.Add((cstr_command+str_sepa_0x01));
            //  ①X方向画像ファイル数
            str_wrk = nintXDirectionImageNum.ToString();
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  ②Y方向画像ファイル数
            str_wrk = nintYDirectionImageNum.ToString();
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  ③入力画像ファイルフルパス名のリスト
            foreach (string str in nstrInputImageFileNameArray)
            {
                str_wrk = str;
                str_wrk += str_sepa_0x01;
                list_send_data_list.Add(str_wrk);
            }
            //  ④出力画像ファイルフルパス名
            str_wrk = nstrOutputImageFileName;
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  ⑤出力画像がJPGならばJPEG品質パラメーター
            if (nbMakeJPG != false)
            {
                str_wrk = nintJPGQuality.ToString();
                list_send_data_list.Add(str_wrk);
            }

            //  送信
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

		#region WM_USER等のメッセージ送信関係

		#region WM_USER送信
		/// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     WM_USER送信
        /// </summary>
        /// 
        /// <param name="niMessageNo">WM_USERのメッセージ番号</param>
        /// <param name="niWParam">WPARAM</param>
        /// <param name="niLParam">LPARAM</param>
        /// 
        /// <returns>
        /// 0:正常      
        /// -1:ソケット通信的に送信失敗
        /// -2:文字列オーバー
        /// -3:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int sendWMUSER(int niMessageNo, int niWParam, int niLParam)
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  文字列作成 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + niMessageNo.ToString() + " " + niWParam.ToString() + " " + niLParam.ToString() + "\n";
                //  送信
                i_ret = Send(str_send_string);

                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -3;
            }
        }
        #endregion

        #region 測定を中止する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 測定を中止する
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:正常
        /// -1:ソケット通信的に送信失敗
        /// -2:文字列オーバー
        /// -3:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureStopByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  文字列作成 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "122 0 0\n";
                //  送信
                i_ret = Send(str_send_string);


                //  連続してメッセージ送信するとうまく受信されないので待つ
                System.Threading.Thread.Sleep(500);

                //  中止する前に、測定が中断されている可能性があるので再開しておく
                //  中断されてなくても再開メッセージ送信しても問題ないため
                MeasureRestartByWMUSER();

                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -3;
            }
        }
        #endregion

        #region 測定を中断する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 測定を中断する
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:正常
        /// -1:ソケット通信的に送信失敗
        /// -2:文字列オーバー
        /// -3:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureSuspendByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  文字列作成 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "139 1 0\n";
                //  送信
                i_ret = Send(str_send_string);

                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -3;
            }
        }
        #endregion

        #region 測定を再開する
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 測定を再開する
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:正常
        /// -1:ソケット通信的に送信失敗
        /// -2:文字列オーバー
        /// -3:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureRestartByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  文字列作成 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "139 2 0\n";
                //  送信
                i_ret = Send(str_send_string);

                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                return -3;
            }
        }
        #endregion
        #endregion

        #region 送信文字列の作成を行い、送信する。リモートコマンド限定
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// 送信文字列の作成を行い、送信する。リモートコマンド限定
        /// </summary>
        /// 
        /// <param name="nlistSendData">送信するコマンド+引数リスト</param>
        /// <param name="nstrMethodName">この関数を呼びだした関数名</param>
        /// <param name="nbExeptionCommand">true:2度送り防止の例外コマンド　false:通常のコマンド</param>
        /// 
        /// <returns>
        ///    0:正常
        ///   -1:ソケット通信的に送信失敗
        ///   -2:文字列オーバー
        ///   -3:ソケットがオープンしていない(有効でない)
        ///   -4:2度送りエラー
        ///   -10:それ以外のエラー
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        private int makeSendStringAndSendData(List<string> nlistSendData, string nstrMethodName, bool nbExeptionCommand )
        {
            const string strTerminate = "\n";
            string str_send_string = "";
            int i_ret;

            //  ソケットが有効でなければエラー
            if (m_bActiveSocket == false)
            {
                return -3;
            }

            //  まだ前回のリモートコマンドの返答が帰ってきてないのに再度送信しようとしたときはエラー
            //  例外コマンドでないことも条件
            if (m_bSentRemoteCommand == true && nbExeptionCommand == false)
            {
                return -4;
            }

            try
            {
                //  リストから送信文字列を作成
                str_send_string = nlistSendData[0];
                for(int i = 1; i < nlistSendData.Count; i++)
                {
                    string prev = nlistSendData[i - 1];
                    string cur = nlistSendData[i];

                    if (prev[prev.Length - 1] != 0x01 && prev[prev.Length - 1] != 0x02)
                    {
                        str_send_string += " ";
                    }

                    str_send_string += cur;
                }
                str_send_string += strTerminate;

                //  送信
                i_ret = Send(str_send_string);

                //  送信成功したら送信フラグを立てる
                if( i_ret == 0 )
                {
                    //  2度送り防止の例外コマンドである
                    if (nbExeptionCommand == true)
                    {
                        m_SentExeptionCommand = true;
						//	送ったコマンド文字列を覚えておく
						m_strLastSentExceptionCommand = nstrMethodName;
                    }
                    //  通常のコマンドである
                    else
                    {
                        m_bSentRemoteCommand = true;
						//	送ったコマンド文字列を覚えておく
						m_strLastSentNormalCommand = nstrMethodName;
                    }
                }

                return i_ret;
            }
            catch (Exception ex)
            {
                string str_err;
                if (str_send_string.Length > 0)
                {
                    str_err = nstrMethodName + "," + ex.Message + "," + "Send Data:" + str_send_string;
                }
                else
                {
                    str_err = nstrMethodName + "," + ex.Message;
                }
                OutputErrorLog(str_err);
                return -10;
            }

        }
        #endregion

    }
}
