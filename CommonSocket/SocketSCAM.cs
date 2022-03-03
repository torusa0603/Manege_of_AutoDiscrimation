using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

//  XML�h�L�������g�̌x���͖���
#pragma warning disable 1591

namespace SPCommonSocket
{
    #region SCAM����̎�M�f�[�^���\����
    //  SCAM����̎�M�f�[�^���\����
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

    //  �C�x���g�n���h���p�f���Q�[�g
    public delegate void EventHandlerReceiveSCAM(ReceiveFromSCAMInfo ReceiveInfo);

    public class CSocketSCAM : CSocketCommunicationBase
    {
        #region �C�x���g��`
        //  SCAM�f�[�^��M�C�x���g
        public event EventHandlerReceiveSCAM evReceiveSCAMDataInfo;
        #endregion

        #region �����o�[�ϐ�

        /// <summary>
        /// �����[�g�R�}���h�̕ԓ� /OK
        /// </summary>
        public const int eCommandReplyOK    = 0;
        /// <summary>
        /// �����[�g�R�}���h�̕ԓ� /NG
        /// </summary>
        public const int eCommandReplyNG    = 1;
        /// <summary>
        /// �����[�g�R�}���h�̕ԓ� /IL
        /// </summary>
        public const int eCommandReplyIL    = 2;
        /// <summary>
        /// �����[�g�R�}���h�̕ԓ� /ManualEnd
        /// </summary>
        public const int eCommandManualEnd = 6;
        /// <summary>
        /// :WM_USER���b�Z�[�W
        /// </summary>
        public const int eWMUSER            = 3;
        /// <summary>
        /// :SCAM_EVENT���b�Z�[�W
        /// </summary>
        public const int eSCAM_EVENT        = 4;
        /// <summary>
        /// :AF_getZAxisStatus���b�Z�[�W
        /// </summary>
        public const int eZAxisStatus       = 5;
        /// <summary>
        /// ���̑�
        /// </summary>
        public const int eOther             = 100;
        /// <summary>
        /// �s��l
        /// </summary>
        public const int eInvalid           = -9999;

        const string strCommandOK           = "/OK";
        const string strCommandNG           = "/NG";
        const string strCommandIL           = "/IL";
        const string strCommandManualEnd    = "/ManualEnd";
        const string strWM_USER             = ":WM_USER";
        const string strSCAM_EVENT          = ":SCAM_EVENT";
        const string strZAxisStatus         = ":AF_getZAxisStatus";

        public bool m_bSentRemoteCommand;   //  �����[�g�R�}���h�𑗐M�����true�ƂȂ�
                                            //  ���̕ԓ����󂯎���false�ɖ߂�
        const string strDoublePointFormat = "F6";   //  ���������_�𕶎���ɕϊ�����Ƃ��̏����_�ȉ��̌���

        public bool m_SentExeptionCommand;  //  2�x����h�~�̗�O�R�}���h�𑗐M�����true
                                            //  ���̕ԓ����󂯎���false
		public string m_strLastSentNormalCommand;	//	�Ō��SCAM�ɑ��M�����R�}���h��(��x����h�~�̕��ʂ̃R�}���h)
		public string m_strLastSentExceptionCommand;	//	�Ō��SCAM�ɑ��M�����R�}���h��(��x����h�~�̗�O�R�}���h)
        #endregion

        #region �R���X�g���N�^
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// 
        /// <param name="niType">
        /// 
        /// <para>�쐬���悤�Ƃ��Ă���\�P�b�g�^�C�v</para>
        /// <para>�K��CSocketCommunicationBase.SCAM_CLIENT��ݒ肵�Ă�������</para>
        /// 
        /// </param>
        /// 
        /// <param name="nformParent">�e�t�H�[��</param>
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

        #region �R���X�g���N�^2
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �R���X�g���N�^2
        /// </summary>
        /// 
        /// <param name="niType">
        /// 
        /// <para>�쐬���悤�Ƃ��Ă���\�P�b�g�^�C�v</para>
        /// <para>�K��CSocketCommunicationBase.SCAM_CLIENT��ݒ肵�Ă�������</para>
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

        #region �\�P�b�g�I�[�v��(IP�������̓z�X�g���w��)
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �\�P�b�g�I�[�v��(IP�������̓z�X�g���w��)
        /// </summary>
        /// 
        /// <param name="nstrIPHost">IP�A�h���X�������̓z�X�g��</param>
        /// <param name="niPort">�|�[�g�ԍ�</param>
        /// 
        /// <returns>
        /// <para>0:����</para>
        /// <para>-1:�T�[�o�[�Ƃ��ċN�������̂ɃN���C�A���g�̃\�P�b�g���I�[�v�����悤�Ƃ���</para>
        /// <para> -2:���Ƀ\�P�b�g���I�[�v�����Ă���</para>
        /// <para>-3:�\�P�b�g�I�[�v���G���[</para>
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        override public int OpenSocket(string nstrIPHost, int niPort)
        {
            //  2�x����t���O��false�ɂ���
            m_bSentRemoteCommand = false;
            return base.OpenSocket(nstrIPHost, niPort);
        }
        #endregion

        #region �\�P�b�g�I�[�v��
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�\�P�b�g�I�[�v��(�z�X�g���Ƃ��Ď������g��PC�̃z�X�g�����g�p����)</para>
        /// </summary>
        /// 
        /// <param name="niPort">�|�[�g�ԍ�</param>
        /// 
        /// <returns>
        /// <para>0:����</para>
        /// <para>-1:�T�[�o�[�@�\�͂Ȃ�</para>
        /// <para> -2:���Ƀ\�P�b�g���I�[�v�����Ă���</para>
        /// <para>-3:�\�P�b�g�I�[�v���G���[</para>
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        override public int OpenSocket(int niPort)
        {
            //  2�x����t���O��false�ɂ���
            m_bSentRemoteCommand = false;
            return base.OpenSocket(niPort);
        }
        #endregion

        #region �����[�g�R�}���h���M���t���O���N���A���A���M���Ă��Ȃ���Ԃɖ߂�
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �����[�g�R�}���h���M���t���O���N���A���A���M���Ă��Ȃ���Ԃɖ߂�
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void CancelCommandSent()
        {
            m_bSentRemoteCommand = false;
            m_SentExeptionCommand = false;
        }
        #endregion

        #region �f�[�^��M�C�x���g�n���h��
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �f�[�^��M�C�x���g�n���h��
        /// </summary>
        /// 
        /// <param name="strReceiveData">��M�f�[�^</param>
        /// ----------------------------------------------------------------------------------------
        public void ReceiveDataFromBase(string strReceiveData)
        {
            int i_index;
            ReceiveFromSCAMInfo info = new ReceiveFromSCAMInfo();

            try
            {
                //  �s��l�ŏ�����
                info.iCommandReply = info.iDataType = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;

                //  ��M��������

                //  �f�[�^����(���p�X�y�[�X)
                string[] stArrayData = strReceiveData.Split(' ');

                if (stArrayData.Count() > 0)
                {
                    //  ���������Ō�̃f�[�^�̖����ɏI�[��������s�����������Ă���Ώ���
                    stArrayData[stArrayData.Count() - 1] = stArrayData[stArrayData.Count() - 1].Trim('\0', '\n');

                    //  �f�[�^�^�C�v������
                    switch (stArrayData[0])
                    {
                        case strCommandOK:
                            info.iDataType = eCommandReplyOK;

                            //  2�x����h�~�̗�O�R�}���h�̕ԓ��ł������ꍇ
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	�R�}���h������ɂ���
								m_strLastSentExceptionCommand = "";
                            }
                            //  �ʏ�̃����[�g�R�}���h�̕ԓ��ł������ꍇ
                            else
                            {
                                //  �����[�g�R�}���h�ɑ΂���ԓ�����M�����̂ŁA�R�}���h���M�t���O��������
                                m_bSentRemoteCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	�R�}���h������ɂ���
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandNG:
                            info.iDataType = eCommandReplyNG;

                            //  2�x����h�~�̗�O�R�}���h�̕ԓ��ł������ꍇ
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	�R�}���h������ɂ���
								m_strLastSentExceptionCommand = "";
                            }
                            //  �ʏ�̃����[�g�R�}���h�̕ԓ��ł������ꍇ
                            else
                            {
                                //  �����[�g�R�}���h�ɑ΂���ԓ�����M�����̂ŁA�R�}���h���M�t���O��������
                                m_bSentRemoteCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	�R�}���h������ɂ���
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandIL:
                            info.iDataType = eCommandReplyIL;

                            //  2�x����h�~�̗�O�R�}���h�̕ԓ��ł������ꍇ
                            if (m_SentExeptionCommand == true)
                            {
                                m_SentExeptionCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentExceptionCommand;
								//	�R�}���h������ɂ���
								m_strLastSentExceptionCommand = "";
                            }
                            //  �ʏ�̃����[�g�R�}���h�̕ԓ��ł������ꍇ
                            else
                            {
                                //  �����[�g�R�}���h�ɑ΂���ԓ�����M�����̂ŁA�R�}���h���M�t���O��������
                                m_bSentRemoteCommand = false;
								//	�R�}���h����Ԃ��B
								info.strLastSentCommand = m_strLastSentNormalCommand;
								//	�R�}���h������ɂ���
								m_strLastSentNormalCommand = "";
                            }
                            break;
                        case strCommandManualEnd:
                            info.iDataType = eCommandManualEnd;
                            //  �����[�g�R�}���h�ɑ΂���ԓ�����M�����̂ŁA�R�}���h���M�t���O��������
                            m_bSentRemoteCommand = false;
							//	�R�}���h����Ԃ��B
							info.strLastSentCommand = m_strLastSentNormalCommand;
							//	�R�}���h������ɂ���
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
                    //  ���̑��ɕ��ނ���Ă�:AF_getZAxisStatus���b�Z�[�W�����͓���Ȃ̂ł����Ń`�F�b�N
                    //  �܂��A/OK,x�Ƃ����J���}��؂�ŕԂ��Ă���R�}���h������̂ł���������Ń`�F�b�N
                    if (info.iDataType == eOther)
                    {
                        //  ���������:AF_getZAxisStatus�Ƃ����������܂܂�邩�m�F
                        if (stArrayData[0].IndexOf(strZAxisStatus) >= 0)
                        {
                            //  �܂܂�Ă���ꍇ
                            info.iDataType = eZAxisStatus;
                            //  =�ȍ~�̐��l�����o��
                            i_index = stArrayData[0].IndexOf("=");
                            if (i_index > 0)
                            {
                                info.iCommandReply = int.Parse(stArrayData[0].Substring(i_index + 1));
                            }
                        }
                        //  /OK���܂܂�邩�m�F
                        if (stArrayData[0].IndexOf(strCommandOK) >= 0)
                        {
                            //  �܂܂�Ă���ꍇ
                            info.iDataType = eCommandReplyOK;
                            //  �����[�g�R�}���h�ɑ΂���ԓ�����M�����̂ŁA�R�}���h���M�t���O��������
                            m_bSentRemoteCommand = false;   
                        }
                    }

                    //  �����[�g�R�}���h�����ł��ANG�AIL�ł������ꍇ
                    if (info.iDataType == eCommandReplyNG || info.iDataType == eCommandReplyIL)
                    {
                        //  �G���[�ԍ���ݒ肷��
                        if (stArrayData.Count() > 1)
                        {
                            info.iCommandReply = int.Parse(stArrayData[1]);
                        }
                    }
                    //  WM_USER���b�Z�[�W�ł������ꍇ
                    else if (info.iDataType == eWMUSER)
                    {
                        //  �K���@:WM_USER xxx yyy zzz��4�̕�����ɕ�������Ă���K�v����
                        if (stArrayData.Count() == 4)
                        {
                            info.iMessageNo = int.Parse(stArrayData[1]);
                            info.iWparam = int.Parse(stArrayData[2]);
                            info.iLparam = int.Parse(stArrayData[3]);
                        }
                    }
                }

                //  ����M�f�[�^�������ݒ�
                info.strRawReceiveData = strReceiveData;
                //  �X�y�[�X��؂�ŕ������ꂽ������z���ݒ�
                info.strReceiveDataArray = new string[stArrayData.Count()];
                for (int i_loop = 0; i_loop < stArrayData.Count(); i_loop++)
                {
                    info.strReceiveDataArray[i_loop] = stArrayData[i_loop];
                }
            }
            catch (FormatException)
            {
                //  ��O���������ꍇ�́ADataType�Ɛ��f�[�^�����n�����Ƃɂ���
                info.iCommandReply = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;
                info.strRawReceiveData = strReceiveData;
            }
            catch (Exception)
            {
                //  ��O���������ꍇ�́ADataType�Ɛ��f�[�^�����n�����Ƃɂ���
                info.iCommandReply = info.iMessageNo = info.iWparam = info.iLparam = eInvalid;
                info.strRawReceiveData = strReceiveData;
            }
            finally
            {
                //  �Ō�ɃC�x���g���M
                if (evReceiveSCAMDataInfo != null)    //  �o�^���ĂȂ��\������̂ł���K�{
                {
                    //  �C�x���g���M
                    evReceiveSCAMDataInfo(info);
                }
            }
        }
        #endregion

        #region SCAM�����[�g�R�}���h���M�ꗗ     (�V���ɃR�}���h���`����ꍇ�͂����ɒǉ�����)

        #region Connect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���̃R�}���h���󂯕t����ƃ����[�g�@�\���J�n���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Connect()
        {
            const string cstr_command = "/Connect";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);     
        }
        #endregion

        #region DisConnect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �����[�g�@�\���I�����܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DisConnect()
        {
            const string cstr_command = "/DisConnect";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);  
        }
        #endregion

        #region Auto1
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �I�[�g�P�����s���܂�
        /// </summary>
        /// 
        /// <param name="niMacroNum">���s����}�N���t�@�C����</param>
        /// <param name="nstrMacroFile">�}�N���t�@�C�����z��</param>
        /// <param name="nstrOutputFile">���ʃt�@�C����</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Auto1(int niMacroNum, string[] nstrMacroFile, string nstrOutputFile)
        {
            const string cstr_command = "/Auto1";
            List<string> list_send_data_list = new List<string>();

            //  �}�N�����ƁA���ۂ̃}�N���t�@�C�����z��̗v�f�����قȂ��Ă���΃G���[
            if (niMacroNum != nstrMacroFile.Count())
            {
                return -1;
            }
            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMacroNum.ToString());
            foreach (string macro in nstrMacroFile)
            {
                list_send_data_list.Add(macro);
            }
            list_send_data_list.Add(nstrOutputFile);
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false); 
        }
        #endregion

        #region AFMoveTo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	Z�̈ړ�����
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:��Βl�ړ��@false:���Βl�ړ�</param>
        /// <param name="ndZPosition">Z���W</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int AFMoveTo(bool nbABSMove, double ndZPosition)
        {
            const string cstr_command = "/AFMoveTo";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ��Βl�ړ�
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  ���Βl�ړ�
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndZPosition.ToString(strDoublePointFormat)); 

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Status
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �₢���킹���s���܂�
        /// </summary>
        /// 
        /// <param name="nRequestNo">
        /// 
        /// <para>�₢���킹�ԍ�(1�`)</para>
        /// 
        /// <para>1:���s���[�h</para>
        /// <para>2:���s���̃}�N��</para>
        /// <para>3:���ʃ��R�[�h����</para>
        /// <para>4:�\�����</para>
        /// <para>5:���_���A�Ȃǂ̏������������I���������ۂ�</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int Status( int nRequestNo )
        {
            const string cstr_command = "/Status";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nRequestNo.ToString());

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region TableMoveTo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �e�[�u���̈ړ�����
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:��Βl�ړ��@false:���Βl�ړ�</param>
        /// <param name="ndX">X���W</param>
        /// <param name="ndY">Y���W</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int TableMoveTo(bool nbABSMove, double ndX, double ndY)
        {
            const string cstr_command = "/TableMoveTo";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ��Βl�ړ�
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  ���Βl�ړ�
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Through
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �摜�̃X���[���s���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Through()
        {
            const string cstr_command = "/Through";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Freeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �摜�̃t���[�Y���s���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Freeze()
        {
            const string cstr_command = "/Freeze";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AFocusA
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �I�[�g�t�H�[�J�X�`�����s���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int AFocusA()
        {
            const string cstr_command = "/AFocusA";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AFocusB
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �I�[�g�t�H�[�J�XB�����s���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int AFocusB()
        {
            const string cstr_command = "/AFocusB";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RunMacro
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     �}�N�����s����
        /// </summary>
        /// 
        /// <param name="nstrMacroFileName">���s����}�N���t�@�C����</param>
        /// <param name="niFlg">0�ȊO�Ȃ�A���C�����g�������Ȃ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RunMacro(string nstrMacroFileName, int niFlg)
        {
            const string cstr_command = "/RunMacro";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrMacroFileName);
            list_send_data_list.Add(niFlg.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightOn
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �Ɩ�������s���܂�
        /// </summary>
        /// 
        /// <param name="niLightType">0: ����  1: ����  2: ����</param>
        /// <param name="niValue">�Ɠx �i0-100�j</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightOn(int niLightType, int niValue)
        {
            const string cstr_command = "/LightOn";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            list_send_data_list.Add(niValue.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �Ɩ�������s���܂�
        /// </summary>
        /// 
        /// <param name="niLightType">0: ����  1: ����  2: ����</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightOff(int niLightType)
        {
            const string cstr_command = "/LightOff";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Light2On
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �Ɩ�������s���܂�
        /// </summary>
        /// 
        /// <param name="niDirection">0:��  1:�E  2:�O  3:��</param>
        /// <param name="niValue">�Ɠx (0-100)</param>
        /// <param name="niAngle">�p�x  0:�R�O�x  1:�S�T�x  2:�U�O�x</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Light2On(int niDirection, int niValue, int niAngle)
        {
            const string cstr_command = "/Light2On";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDirection.ToString());
            list_send_data_list.Add(niValue.ToString());
            list_send_data_list.Add(niAngle.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region Light2Off
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �Ɩ�������s���܂�
        /// </summary>
        /// 
        /// <param name="niDirection">0:��  1:�E  2:�O  3:��</param>
        /// <param name="niAngle">�p�x  0:�R�O�x  1:�S�T�x  2:�U�O�x</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Light2Off(int niDirection, int niAngle)
        {
            const string cstr_command = "/Light2Off";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDirection.ToString());
            list_send_data_list.Add(niAngle.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     �A���C�����g���Z�b�g���܂�
        /// </summary>
        /// 
        /// <param name="ndX">�ݒ���W X</param>
        /// <param name="ndY">�ݒ���W Y</param>
        /// <param name="ndAngle">�ݒ�p�x</param>
        /// <param name="ndAxis">�ݒ莲</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetAlign(double ndX, double ndY, double ndAngle, double ndAxis)
        {
            const string cstr_command = "/SetAlign";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAngle.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAxis.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ResetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �A���C�����g�����Z�b�g���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetAlign()
        {
            const string cstr_command = "/ResetAlign";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetFaceAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    �ʃA���C�����g���Z�b�g���܂�
        /// </summary>
        /// 
        /// <param name="ndXOrigin">X�����_�i���[���h���W�n�j</param>
        /// <param name="ndYOrigin">Y�����_�i���[���h���W�n�j</param>
        /// <param name="ndZOrigin">Z�����_</param>
        /// <param name="ndXAxisAngle">�ʏ��X���̌X��</param>
        /// <param name="ndYAxisAngle">�ʂ�X���̌X��(Z��)</param>
        /// <param name="ndZAxisAngle">�ʂ�Y���̌X��(Z��)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetFaceAlign(double ndXOrigin, double ndYOrigin, double ndZOrigin,
                                double ndXAxisAngle, double ndYAxisAngle, double ndZAxisAngle)
        {
            const string cstr_command = "/SetFaceAlign";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(ndXOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndYOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZOrigin.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZAxisAngle.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndYAxisAngle.ToString(strDoublePointFormat));
                list_send_data_list.Add(ndZAxisAngle.ToString(strDoublePointFormat));

                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + " " +
                                   list_send_data_list[2] + " " + list_send_data_list[3] + " " +
                                    list_send_data_list[4] + " " + list_send_data_list[5] + "," +
                                     list_send_data_list[6] + "\n";

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        ///  �ʃA���C�����g�����Z�b�g���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetFaceAlign()
        {
            const string cstr_command = "/ResetFaceAlign";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetManual
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///   �����[�g�ڑ����Ƀ}�j���A������������܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetManual()
        {
            const string cstr_command = "/SetManual";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ShowWindow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM�\����Ԃ�ύX���܂�
        /// </summary>
        /// 
        /// <param name="niMode">0:�ʏ�\�� 1:�ŏ��� 2:�ő剻</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int ShowWindow(int niMode)
        {
            const string cstr_command = "/ShowWindow";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ErrorAction
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �����[�g�ڑ����G���[�̃_�C�A���O�̕\���A��\���̐ݒ�A�đ���̖⍇�킹�_�C�A���O�̕\���A��\���̐ݒ���s���܂�
        /// </summary>
        /// 
        /// <param name="niMode">0:�ʏ�\�� 1:��\��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int ErrorAction(int niMode)
        {
            const string cstr_command = "/ErrorAction";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region PixelToTable
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �n���ꂽ�l���A���������߂���W���烏�[�N���W�ɕϊ����ĕԂ��܂�
        /// </summary>
        /// 
        /// <param name="ndPixelX">���������߂���W X</param>
        /// <param name="ndPixelY">���������߂���W Y</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int PixelToTable(double ndPixelX, double ndPixelY)
        {
            const string cstr_command = "/PixelToTable";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndPixelX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndPixelY.ToString(strDoublePointFormat));

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetHardError
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM���Ńn�[�h�G���[���������Ă��邩�ǂ����̏�Ԃ�Ԃ��܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetHardError()
        {
            const string cstr_command = "/GetHardError";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region Filter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �t�B���^�����O�̎��s����
        /// </summary>
        /// 
        /// <param name="niFilterGroup">�t�B���^�[�O���[�v</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int Filter(int niFilterGroup)
        {
            const string cstr_command = "/Filter";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niFilterGroup.ToString());

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LAFocusTrace
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  LAF�g���[�X���[�h�̐�����s����
        /// </summary>
        /// 
        /// <param name="niTraceMode">0:LAF�g���[�X���[�h�̏I�����w��    1:�J�n���w��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------       
        public int LAFocusTrace(int niTraceMode)
        {
            const string cstr_command = "/LAFocusTrace";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niTraceMode.ToString());

            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetWorld
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	����W��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="ndX">X���W(mm)</param>
        /// <param name="ndY">Y���W(mm)</param>
        /// <param name="ndAngle">�p�x(��)</param>
        /// <param name="ndReverseXFlg">x�����]�t���O(0:off, 1:on)</param>
        /// <param name="ndReverseYFlg">x�����]�t���O(0:off, 1:on)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetWorld(double ndX, double ndY, double ndAngle, double ndReverseXFlg, double ndReverseYFlg)
        {
            const string cstr_command = "/SetWorld";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndAngle.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndReverseXFlg.ToString());
            list_send_data_list.Add(ndReverseYFlg.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWorld
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ����W���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWorld()
        {
            const string cstr_command = "/GetWorld";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAlign
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���[�J���A���C�����g���_�ʒu���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAlign()
        {
            const string cstr_command = "/GetAlign";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetContBri
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     <para>�摜�����{�[�h�̃R���g���X�g�E�u���C�g�l�X��ݒ肵�܂��B</para>
        ///     <para>�J�����ԍ��Ƀn�[�h�E�F�A�ɑ��݂��Ȃ��ԍ����w�肷��Ƃ��ׂẴJ�����ɑ΂��Đݒ���s���܂�</para>
        /// </summary>
        /// 
        /// <param name="niCameraNo">�J�����ԍ�(0:Main-Lo, 1:Main-Hi, 2:Sub-Lo, 3:Sub-Hi)</param>
        /// <param name="ndContrast">�R���g���X�g(0.0 -> 1.0)</param>
        /// <param name="ndBrightness">�u���C�g�l�X(0.0 -> 1.0)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetContBri(int niCameraNo, double ndContrast, double ndBrightness)
        {
            const string cstr_command = "/SetContBri";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            list_send_data_list.Add(ndContrast.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndBrightness.ToString(strDoublePointFormat));
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetContBri
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	�摜�����{�[�h�̃R���g���X�g�E�u���C�g�l�X���擾���܂�
        /// </summary>
        /// 
        /// <param name="niCameraNo">�J�����ԍ�(0:Main-Lo, 1:Main-Hi, 2:Sub-Lo, 3:Sub-Hi)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetContBri(int niCameraNo)
        {
            const string cstr_command = "/GetContBri";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMMACPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ����SCAM�ɐݒ肳��Ă���MAC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ�����t���p�X�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMMACPath()
        {
            const string cstr_command = "/GetSCAMMACPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMCSVPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	����SCAM�ɐݒ肳��Ă���CSV�`���t�@�C���̕ۑ�����t���p�X�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMCSVPath()
        {
            const string cstr_command = "/GetSCAMCSVPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMMACPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  ����SCAM�ɐݒ肳��Ă���MAC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ�����p�����[�^�[�Ŏw�肵���p�X���ōX�V���܂�
        /// </summary>
        /// 
        /// <param name="nstrMACPath">MAC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMMACPath(string nstrMACPath)
        {
            const string cstr_command = "/SetSCAMMACPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrMACPath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMCSVPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	����SCAM�ɐݒ肳��Ă���CSV�`���t�@�C���̕ۑ�����p�����[�^�[�Ŏw�肵���p�X���ōX�V���܂�
        /// </summary>
        /// 
        /// <param name="nstrCSVPath">CSV�t�@�C���̕ۑ���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMCSVPath(string nstrCSVPath)
        {
            const string cstr_command = "/SetSCAMCSVPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrCSVPath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SaveImageJPGNow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �Ō�Ɏ�荞�܂ꂽ�摜���AJPEG�`���t�@�C���Ƃ��ĕۑ����܂�
        /// </summary>
        /// 
        /// <param name="nstrSaveFileFullPath">�ۑ�����t�@�C�����i�t���p�X�Ŋg���q�t���Ŏw��)</param>
        /// <param name="niColor">
        /// 
        /// <para>0�F���m�N��</para>
        /// <para>1�FR+G+B </para>
        /// <para>2�FR</para>
        /// <para>3�FG</para>
        /// <para>4�FB</para>
        /// 
        /// </param>
        /// <param name="niGraphic">�摜�ɃO���t�B�b�N�X���I�[�o���C����Ƃ���1�A���Ȃ��ꍇ��0���w��</param>
        /// <param name="niQuality">JPEG�̕i����1�i��i���A�T�C�Y���j�`100�i���i���A�T�C�Y��j�Ŏw��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SaveImageJPGNow(string nstrSaveFileFullPath, int niColor, int niGraphic, int niQuality )
        {
            const string cstr_command = "/SaveImageJPGNow";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveFileFullPath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            list_send_data_list.Add(niQuality.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StartSaveImageJPGOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�}�N���R�}���hFreeze�����s����邲�ƂɁAJPEG�`���t�@�C����</para>
        /// <para>�w�肵���x�[�X�� �{ �A���_�[�o�[ �{ �V�[�P���V�����ԍ� �{ �DJPG</para>
        /// <para>�Ƃ������O�ŕۑ����܂��B�V�[�P���V�����ԍ��͂��̃R�}���h����M���Ă���ŏ���Freeze��1�ɏ���������A�Ȍ�A�ۑ�����x��1�C���N�������g����܂�</para>
        /// </summary>
        /// 
        /// <param name="nstrSaveBaseFilePath">�ۑ�����x�[�X�t�@�C�����i�p�X�t���Ŏw��A�g���q�͕s�v)</param>
        /// <param name="niColor">
        /// 
        /// <para>0�F���m�N��</para>
        /// <para>1�FR+G+B </para>
        /// <para>2�FR</para>
        /// <para>3�FG</para>
        /// <para>4�FB</para>
        /// 
        /// </param>
        /// <param name="niGraphic">�摜�ɃO���t�B�b�N�X���I�[�o���C����Ƃ���1�A���Ȃ��ꍇ��0���w��</param>
        /// <param name="niQuality">JPEG�̕i����1�i��i���A�T�C�Y���j�`100�i���i���A�T�C�Y��j�Ŏw��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StartSaveImageJPGOnFreeze(string nstrSaveBaseFilePath, int niColor, int niGraphic, int niQuality)
        {
            const string cstr_command = "/StartSaveImageJPGOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveBaseFilePath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            list_send_data_list.Add(niQuality.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StopSaveImageJPGOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �R�}���hStartSaveImageJPGOnFreeze�ɂ��摜��荞�݂��Ƃ̎����摜�ۑ����[�h���I�����܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StopSaveImageJPGOnFreeze()
        {
            const string cstr_command = "/StopSaveImageJPGOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SaveImageBMPNow
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�Ō�Ɏ�荞�܂ꂽ�摜���ABMP�`���t�@�C���Ƃ��ĕۑ����܂�
        /// </summary>
        /// 
        /// <param name="nstrSaveFileFullPath">�ۑ�����t�@�C�����i�t���p�X�Ŋg���q�t���Ŏw��)</param>
        /// <param name="niColor">0�F���m�N���A1�FR+G+B 2�FR�A3�FG�A4�FB</param>
        /// <param name="niGraphic">�摜�ɃO���t�B�b�N�X���I�[�o���C����Ƃ���1�A���Ȃ��ꍇ��0���w��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SaveImageBMPNow(string nstrSaveFileFullPath, int niColor, int niGraphic)
        {
            const string cstr_command = "/SaveImageBMPNow";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveFileFullPath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StartSaveImageBMPOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�}�N���R�}���hFreeze�����s����邲�ƂɁABMP�`���t�@�C����</para>
        /// <para>�w�肵���x�[�X�� �{ �A���_�[�o�[ �{ �V�[�P���V�����ԍ� �{ �DBMP</para>
        /// <para>�Ƃ������O�ŕۑ����܂��B�V�[�P���V�����ԍ��͂��̃R�}���h����M���Ă���ŏ���Freeze��1�ɏ���������A�Ȍ�A�ۑ�����x��1�C���N�������g����܂�</para>
        /// </summary>
        /// 
        /// <param name="nstrSaveBaseFilePath">�ۑ�����x�[�X�t�@�C�����i�p�X�t���Ŏw��A�g���q�͕s�v)</param>
        /// <param name="niColor">
        /// 
        /// <para>0�F���m�N��</para>
        /// <para>1�FR+G+B </para>
        /// <para>2�FR</para>
        /// <para>3�FG</para>
        /// <para>4�FB</para>
        /// 
        /// </param>
        /// <param name="niGraphic">�摜�ɃO���t�B�b�N�X���I�[�o���C����Ƃ���1�A���Ȃ��ꍇ��0���w��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StartSaveImageBMPOnFreeze(string nstrSaveBaseFilePath, int niColor, int niGraphic )
        {
            const string cstr_command = "/StartSaveImageBMPOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrSaveBaseFilePath);
            list_send_data_list.Add(niColor.ToString());
            list_send_data_list.Add(niGraphic.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region StopSaveImageBMPOnFreeze
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�R�}���hStartSaveImageBMPOnFreeze�ɂ��摜��荞�݂��Ƃ̎����摜�ۑ����[�h���I�����܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int StopSaveImageBMPOnFreeze()
        {
            const string cstr_command = "/StopSaveImageBMPOnFreeze";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMATCPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	����SCAM�ɐݒ肳��Ă���ATC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ�����t���p�X�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSCAMATCPath()
        {
            const string cstr_command = "/GetSCAMATCPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetSCAMATCPath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	����SCAM�ɐݒ肳��Ă���ATC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ�����p�����[�^�[�Ŏw�肵���p�X���ōX�V���܂�
        /// </summary>
        /// 
        /// <param name="nstrATCPath">ATC�`���e�B�[�`���O�}�N���t�@�C���̕ۑ���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMATCPath(string nstrATCPath)
        {
            const string cstr_command = "/SetSCAMATCPath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrATCPath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetNowWorkPos
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	���݂̃e�[�u���ʒu�����[�N���W�n�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetNowWorkPos()
        {
            const string cstr_command = "/GetNowWorkPos";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ShutteOffOnDisconnect
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///�����[�g�f�B�X�R�l�N�g�i�R�}���h��disconnect�ł͂Ȃ��j���ɃV���b�^��Close����i�ʏ��Close����j���ۂ���ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niMode">0�ȊO��Close����B0�̏ꍇ��Close���Ȃ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ShutteOffOnDisconnect(int niMode)
        {
            const string cstr_command = "/ShutteOffOnDisconnect";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niMode.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region DilateFilter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///Dilate filter�����s���܂�
        /// </summary>
        /// 
        /// <param name="niCount">0Dilate filter�̌J��Ԃ���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DilateFilter(int niCount)
        {
            const string cstr_command = "/DilateFilter";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCount.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ErodeFilter
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    Erode filter�����s���܂�
        /// </summary>
        /// 
        /// <param name="niCount">ErodeFilter�̌J��Ԃ���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ErodeFilter(int niCount)
        {
            const string cstr_command = "/ErodeFilter";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCount.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ResetImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �t�B���^�[�������s���O�̉摜�ɖ߂��܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int ResetImage()
        {
            const string cstr_command = "/ResetImage";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAcqPixelArea
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���݂̉摜��荞�݉�ʂ̉�f�����擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAcqPixelArea()
        {
            const string cstr_command = "/GetAcqPixelArea";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRemoteNoRemeasureOther
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �I�[�g�P�A�����[�g�ɂ�鎩�����s�ɑ΂���đ���i�A���C�������g�ȊO�j�̗L���t���O���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteNoRemeasureOther()
        {
            const string cstr_command = "/GetRemoteNoRemeasureOther";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRemoteNoRemeasureAlignment
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�I�[�g�P�A�����[�g�ɂ�鎩�����s�ɑ΂���đ���i�A���C�������g�j�̗L���t���O���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteNoRemeasureAlignment()
        {
            const string cstr_command = "/GetRemoteNoRemeasureAlignment";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetRemoteNoRemeasureOther
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�I�[�g�P�A�����[�g�ɂ�鎩�����s�ɑ΂���đ���i�A���C�������g�ȊO�j�̗L���t���O��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niRemeasure">	0�F�đ�����s���A1�F�đ�����s��Ȃ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemoteNoRemeasureOther(int niRemeasure )
        {
            const string cstr_command = "/SetRemoteNoRemeasureOther";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRemeasure.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetRemoteNoRemeasureAlignment
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  	�I�[�g�P�A�����[�g�ɂ�鎩�����s�ɑ΂���đ���i�A���C�������g�j�̗L���t���O��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niRemeasure">	0�F�đ�����s���A1�F�đ�����s��Ȃ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemoteNoRemeasureAlignment(int niRemeasure)
        {
            const string cstr_command = "/SetRemoteNoRemeasureAlignment";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRemeasure.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightInformation
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �Ɩ��̃n�[�h�E�F�A��`�����擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightInformation()
        {
            const string cstr_command = "/LightInformation";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LightStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	�w�肵���������̏Ɩ���Ԃ��擾���܂�
        /// </summary>
        /// 
        /// <param name="niCameraNo">
        /// <para>0:���C��������</para>
        /// <para>1:�T�u������</para>
        /// <para>2:���ݑI������Ă��錰����</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LightStatus(int niCameraNo)
        {
            const string cstr_command = "/LightStatus";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region CameraStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �J�����̃n�[�h�E�F�A��`��񂨂�сA���ݎg�p���Ă���J�����ԍ����擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int CameraStatus()
        {
            const string cstr_command = "/CameraStatus";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region MaterialHandStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM���}�e�n�����[�h�����ǂ������擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MaterialHandStatus()
        {
            const string cstr_command = "/MaterialHandStatus";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RunTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///    	TFTMeasure�̃��V�s�����s���܂�
        /// </summary>
        /// 
        /// <param name="nstrRecipeFilePath">���V�s�t�@�C����(�t���p�X)</param>
        /// <param name="nstrLayerName">���C���[��</param>
        /// <param name="nstrComment">���茋�ʃt�@�C���ɋL�q�����R�����g     
        ///                           �K��TITLE=xxxxx�̌`���Ŏw�肵�܂��iTITLE�͑啶���ł��j</param>
        /// <param name="nstrResultFilePath">���茋�ʃt�@�C����(�t���p�X)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int RunTFTMeasure(string nstrRecipeFilePath, string nstrLayerName, string nstrComment,
                                 string nstrResultFilePath)
        {
            const string cstr_command = "/RunTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrRecipeFilePath);
                list_send_data_list.Add(nstrLayerName);
                list_send_data_list.Add(nstrComment);
                list_send_data_list.Add(nstrResultFilePath);

                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + "\t" +
                                   list_send_data_list[2] + "\t" + list_send_data_list[3] + "\t" +
                                    list_send_data_list[4] + "\n";

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        /// TFTMeasure�̃��C���[���̂��擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTFTMeasureLayer()
        {
            const string cstr_command = "/GetTFTMeasureLayer";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTimelyDataSaveInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �������s�ɂ����đ��肪�m�肷�邲�Ƃɑ��茋�ʂ𒀎��ۑ����邽�߂̃p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTimelyDataSaveInfo()
        {
            const string cstr_command = "/GetTimelyDataSaveInfo";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTimelyDataSaveInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�������s�ɂ����đ��肪�m�肷�邲�Ƃɑ��茋�ʂ𒀎��ۑ����邽�߂̃p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niExecute">���茋�ʂ̒����ۑ����s���i1�j���ہi0�j��</param>
        /// <param name="niFileSaveType">�t�@�C���������ݕ��@�i1�F�w�葪�萔���ƁA�Q�F�w��o�ߎ��Ԃ���)</param>
        /// <param name="niMeasuerNumber">�t�@�C���������ݕ��@���u�w�葪�萔���Ɓv�̂Ƃ��̎w�葪�萔</param>
        /// <param name="niTime">�t�@�C���������ݕ��@���u�w��o�ߎ��Ԃ��Ɓv�̂Ƃ��̎w��o�ߎ��ԁi�b�P��)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetTimelyDataSaveInfo(int niExecute, int niFileSaveType, int niMeasuerNumber, int niTime )
        {
            const string cstr_command = "/SetTimelyDataSaveInfo";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecute.ToString());
            list_send_data_list.Add(niFileSaveType.ToString());
            list_send_data_list.Add(niMeasuerNumber.ToString());
            list_send_data_list.Add(niTime.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAutoSaveDataByStopMeasurement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�������s���~���Ɏ����I�Ƀf�[�^�ۑ����s�����߂̃p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAutoSaveDataByStopMeasurement()
        {
            const string cstr_command = "/GetAutoSaveDataByStopMeasurement";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAutoSaveDataByStopMeasurement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�������s���~���Ɏ����I�Ƀf�[�^�ۑ����s�����߂̃p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niRun">
        /// 
        /// <para>0�F�������s���~���Ɏ����I�Ƀf�[�^�ۑ����s��Ȃ�</para>
        /// <para>1�F�������s���~���Ɏ����I�Ƀf�[�^�ۑ����s��</para>
        ///                     
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetAutoSaveDataByStopMeasurement(int niRun)
        {
            const string cstr_command = "/SetAutoSaveDataByStopMeasurement";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRun.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�����摜�ۑ��@�\�Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetImageAutoSaveProp()
        {
            const string cstr_command = "/GetImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetRawDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�ȈՏƖ��ώZ�^�C�}�@�\�ɂ����āA�e�Ɩ��̐ώZ�l�i�b�P�ʂł̃J�E���g���j���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRawDataLightTimer()
        {
            const string cstr_command = "/GetRawDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetWarningDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �ȈՏƖ��ώZ�^�C�}�@�\�ɂ����āA�e�Ɩ��̐ώZ�l�̌x����Ԃ��擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWarningDataLightTimer()
        {
            const string cstr_command = "/GetWarningDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region LAFocusSearch
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	LA�T�[�`���[�h�����s���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int LAFocusSearch()
        {
            const string cstr_command = "/LAFocusSearch";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetFormattedRawDataLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �ȈՏƖ��ώZ�^�C�}�@�\�ɂ����āA�e�Ɩ��̐ώZ�l�����������ꂽ�`���i���F���F�b�Ȃǁj�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetFormattedRawDataLightTimer()
        {
            const string cstr_command = "/GetFormattedRawDataLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetWarningSettingsLightTimer
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�ȈՏƖ��ώZ�^�C�}�@�\�ɂ����āA�e�Ɩ��̌x���Ƃ���ώZ�l�����������ꂽ�`���i���F���F�b�Ȃǁj�Ŏ擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWarningSettingsLightTimer()
        {
            const string cstr_command = "/GetWarningSettingsLightTimer";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetRemeasureProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�I�[�g�P�A�����[�g�ł̎������s�ɑ΂���đ���Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemeasureProp()
        {
            const string cstr_command = "/GetRemeasureProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleItemNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �T�u�^�C�g���f�[�^�̍��ڐ����擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleItemNum()
        {
            const string cstr_command = "/GetSubTitleItemNum";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleTitleList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̍��ڂ��Ƃ̃^�C�g�����X�g���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleTitleList()
        {
            const string cstr_command = "/GetSubTitleTitleList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleInputMethodList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̍��ڂ��Ƃ̓��͕��@���X�g���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleInputMethodList()
        {
            const string cstr_command = "/GetSubTitleInputMethodList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMaxLengthList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̍��ڂ��Ƃ̍ő咷���X�g���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMaxLengthList()
        {
            const string cstr_command = "/GetSubTitleMaxLengthList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMinLengthList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̍��ڂ��Ƃ̍ŏ������X�g���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMinLengthList()
        {
            const string cstr_command = "/GetSubTitleMinLengthList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleMaxInputHistoryNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ��̓��͗����̍ő吔���擾���܂�
        /// </summary>
        /// 
        /// <param name="niNth">�P����n�܂�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleMaxInputHistoryNum(int niNth)
        {
            const string cstr_command = "/GetSubTitleMaxInputHistoryNum";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleSelectList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ��̑I�������X�g���擾���܂�
        /// </summary>
        /// 
        /// <param name="niNth">�P����n�܂�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleSelectList(int niNth)
        {
            const string cstr_command = "/GetSubTitleSelectList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSubTitleInputHistory
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ��̓��͗������X�g���擾���܂�
        /// </summary>
        /// 
        /// <param name="niNth">�P����n�܂�T�u�^�C�g���f�[�^�̎w�肵�����ڔԍ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSubTitleInputHistory(int niNth)
        {
            const string cstr_command = "/GetSubTitleInputHistory";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNth.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWeatherSensorData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�C�ۃZ���T�[�f�[�^�i���x�A�C���A���x�ACO2�Z�x�j���擾���܂�
        /// </summary>
        /// 
        /// <param name="niAxis">���̔ԍ��i1�FX,�@2�FY�iY1�j,�@3�FY2,�@4�FZ)</param>
        /// <param name="niDigitForTempData">���x�f�[�^���󂯎��Ƃ��̏����_�ȉ��̌���</param>
        /// <param name="niDigitForPressureData">�C���f�[�^���󂯎��Ƃ��̏����_�ȉ��̌���</param>
        /// <param name="niDigitForHumidityData">���x�f�[�^���󂯎��Ƃ��̏����_�ȉ��̌���</param>
        /// <param name="niDigitForCO2densityData">CO�Q�Z�x�f�[�^���󂯎��Ƃ��̏����_�ȉ��̌���</param>
        /// <param name="niPressureUnit">�C���f�[�^���󂯎��Ƃ��̒P�ʁi1�FhPa,�@2�FmmHg)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetWeatherSensorData(int niAxis, int niDigitForTempData, int niDigitForPressureData,
                                         int niDigitForHumidityData, int niDigitForCO2densityData,
                                          int niPressureUnit )
        {
            const string cstr_command = "/GetWeatherSensorData";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxis.ToString());
            list_send_data_list.Add(niDigitForTempData.ToString());
            list_send_data_list.Add(niDigitForPressureData.ToString());
            list_send_data_list.Add(niDigitForHumidityData.ToString());
            list_send_data_list.Add(niDigitForCO2densityData.ToString());
            list_send_data_list.Add(niPressureUnit.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetChamberTemperatureData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�`�����o�[�̉��x�f�[�^���擾���܂�
        /// </summary>
        /// 
        /// <param name="niSensorNo">�Z���T�[�ԍ�</param>
        /// <param name="niDigitForTempData">���x�f�[�^���󂯎��Ƃ��̏����_�ȉ��̌���</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetChamberTemperatureData(int niSensorNo, int niDigitForTempData)
        {
            const string cstr_command = "/GetChamberTemperatureData";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niSensorNo.ToString());
            list_send_data_list.Add(niDigitForTempData.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetMasterRecipeForTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Recipe Maker�ɂ�����Auto Recipe Update�i�ʏ́FMaster Recipe�j�@�\�Ɋւ���p�����[�^�[���擾����
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetMasterRecipeForTFTMeasure()
        {
            const string cstr_command = "/GetMasterRecipeForTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetMasterRecipeForTFTMeasure
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Recipe Maker�ɂ�����Auto Recipe Update�i�ʏ́FMaster Recipe�j�@�\�Ɋւ���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niNum">�ݒ肷��p�����[�^�[�̌��i����̃o�[�W�����ł͍ő�5�j</param>
        /// <param name="niAutoRecipeUpdateEnable">Auto Recipe Update �@�\���L���ł���(1)����(0)��</param>
        /// <param name="niSaveFileBeforeUpdate">�X�V�O�̃t�@�C����ʖ��ŕۑ�����(1)����(0)��</param>
        /// <param name="niUpdateAfterAutoMeas">�������s�I����i�đ���O�j�ɍX�V���s��(1)����(0)��</param>
        /// <param name="niUpdateAfterReMeas">�đ���I����ɍX�V���s��(1)����(0)��</param>
        /// <param name="niAlignFileUpdate">�X�V���s���ہA�i����ӏ������ł͂Ȃ��j�A���C�����g�t�@�C���̍X�V���s��(1)����(0)��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetMasterRecipeForTFTMeasure(int niNum, int niAutoRecipeUpdateEnable, int niSaveFileBeforeUpdate,
                                         int niUpdateAfterAutoMeas, int niUpdateAfterReMeas,
                                          int niAlignFileUpdate )
        {
            const string cstr_command = "/SetMasterRecipeForTFTMeasure";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niNum.ToString());
            list_send_data_list.Add(niAutoRecipeUpdateEnable.ToString());
            list_send_data_list.Add(niSaveFileBeforeUpdate.ToString());
            list_send_data_list.Add(niUpdateAfterAutoMeas.ToString());
            list_send_data_list.Add(niUpdateAfterReMeas.ToString());
            list_send_data_list.Add(niAlignFileUpdate.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTFTMeasureZInputSupportPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>���V�s���s����Z���W�̓��͎x���@�\���g�p����t���O��ݒ肵�A���A</para>
        ///<para>���V�s���s����1�_�ڂ�Z���W���ŏI�̃A���C�����g����̉摜�t���[�YZ���W����̃I�t�Z�b�gZ���W��ݒ肵�܂�</para>
        /// </summary>
        /// 
        /// <param name="niParamNum">�ݒ�p�����[�^�[��</param>
        /// <param name="niMode">
        /// 
        /// <para>0 : ���V�s���s����Z���W������̂܂܂ɂ��܂��B</para>
        /// <para>1 : ���V�s���s����Z���W���ŏI�̃A���C�����g����̉摜�t���[�Y���W����̃I�t�Z�b�g�ɂ��܂�</para>
        ///                      
        /// </param>
        /// <param name="ndZOffset">���V�s���s����1�_�ڂ�Z���W���ŏI�̃A���C�����g����̉摜�t���[�Y
        ///                          Z���W����̃I�t�Z�b�gZ���W(mm�P��)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetTFTMeasureZInputSupportPrm(int niParamNum, int niMode, double ndZOffset)
        {
            const string cstr_command = "/SetTFTMeasureZInputSupportPrm";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niParamNum.ToString());
            list_send_data_list.Add(niMode.ToString());
            list_send_data_list.Add(ndZOffset.ToString(strDoublePointFormat));
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTFTMeasureZInputSupportPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>���V�s���s����Z���W�̓��͎x���@�\���g�p����t���O��ݒ肵�A���A</para>
        ///<para>���V�s���s����1�_�ڂ�Z���W���ŏI�̃A���C�����g����̉摜�t���[�YZ���W����̃I�t�Z�b�gZ���W���擾���܂�</para>
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTFTMeasureZInputSupportPrm()
        {
            const string cstr_command = "/GetTFTMeasureZInputSupportPrm";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndFileName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///BND�t�@�C�������擾����
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndFileName()
        {
            const string cstr_command = "/GetBndFileName";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndFileName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BND�t�@�C������ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrBNDFilePath">BND�t�@�C���p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndFileName(string nstrBNDFilePath)
        {
            const string cstr_command = "/SetBndFileName";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrBNDFilePath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BND�t�@�C�������݂���t�H���_�p�X���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndFilePath()
        {
            const string cstr_command = "/GetBndFilePath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// BND�t�@�C�������݂���t�H���_�p�X��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrBNDFolderPath">BND�t�@�C�������݂���t�H���_�p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndFilePath(string nstrBNDFolderPath)
        {
            const string cstr_command = "/SetBndFilePath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrBNDFolderPath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetBndUseFlg
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///���ݕ␳�@�\�̎��s��Ԃ��擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetBndUseFlg()
        {
            const string cstr_command = "/GetBndUseFlg";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetBndUseFlg
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���ݕ␳�@�\�̎��s��Ԃ�ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niExecCalib">
        /// 
        /// <para>1�F���ݕ␳�@�\�����s���� </para>
        /// <para>0:���s���Ȃ�</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetBndUseFlg(int niExecCalib )
        {
            const string cstr_command = "/SetBndUseFlg";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecCalib.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetUserGridCorrectFlag
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///���[�U�[�O���b�h�␳�@�\�̎��s��Ԃ�ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niExecGridCorrect">1�F���[�U�[�O���b�h�␳�@�\�����s���� 0:���s���Ȃ�</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetUserGridCorrectFlag(int niExecGridCorrect)
        {
            const string cstr_command = "/SetUserGridCorrectFlag";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niExecGridCorrect.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetUserGridCorrectFlag
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///���[�U�[�O���b�h�␳�@�\�̎��s��Ԃ��擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetUserGridCorrectFlag()
        {
            const string cstr_command = "/GetUserGridCorrectFlag";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetUserGridCorrectFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���[�U�[�O���b�h�␳�t�@�C���̃t���p�X��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrUserGridCorrectFilePath">���[�U�[�O���b�h�␳�t�@�C���̃t���p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetUserGridCorrectFilePath(string nstrUserGridCorrectFilePath)
        {
            const string cstr_command = "/SetUserGridCorrectFilePath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrUserGridCorrectFilePath);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetUserGridCorrectFilePath
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���[�U�[�O���b�h�␳�t�@�C���̃t���p�X���擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetUserGridCorrectFilePath()
        {
            const string cstr_command = "/GetUserGridCorrectFilePath";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region LoadImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�摜�t�@�C����ǂݍ����Image window�ɕ\�����܂�
        /// </summary>
        /// 
        /// <param name="nstrImageFilePath">�摜�t�@�C���̃t���p�X��</param>
        /// <param name="nstrMemo1">�C�ӂ̃����p������1�i�P�o�C�g�ȏ�U�R�o�C�g�ȉ�)</param>
        /// <param name="nstrMemo2">�C�ӂ̃����p������2�i�P�o�C�g�ȏ�U�R�o�C�g�ȉ�)</param>
        /// <param name="nstrMemo3">�C�ӂ̃����p������3�i�P�o�C�g�ȏ�U�R�o�C�g�ȉ�)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int LoadImage(string nstrImageFilePath, string nstrMemo1, string nstrMemo2,string nstrMemo3)
        {
            const string cstr_command = "/LoadImage";
            List<string> list_send_data_list = new List<string>();

            try
            {
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrImageFilePath);
                list_send_data_list.Add(nstrMemo1);
                list_send_data_list.Add(nstrMemo2);
                list_send_data_list.Add(nstrMemo3);

                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + "\t" + list_send_data_list[1] + "\t" +
                                   list_send_data_list[2] + "\t" + list_send_data_list[3] + "\t" +
                                    list_send_data_list[4] + "\n";

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        ///	�Ō�Ɏ��s�����u/RunMacro�A/Auto1�A/RunTFTMeasure�v�̏������ʂ��擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetRemoteAutoRunStatus()
        {
            const string cstr_command = "/GetRemoteAutoRunStatus";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region DriveFreeCheckerGetStatus
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���݂́i��Ƀn�[�h�f�B�X�N�j�󂫗e�ʃ`�F�b�N�����擾���܂�
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int DriveFreeCheckerGetStatus()
        {
            const string cstr_command = "/DriveFreeCheckerGetStatus";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region MoveHandlingPosition
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�J�������}�e�n���ޔ��ʒu�Ɉړ����܂�
        /// </summary>
        /// 
        /// <param name="niHandlingPosXY">X�AY���̑ޔ𓮍���s���i1�j���ہi0�j��</param>
        /// <param name="niHandlingPosZ">Z���̑ޔ𓮍���s���i1�j���ہi0�j��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int MoveHandlingPosition(int niHandlingPosXY, int niHandlingPosZ)
        {
            const string cstr_command = "/MoveHandlingPosition";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niHandlingPosXY.ToString());
            list_send_data_list.Add(niHandlingPosZ.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetCylinderMode
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�iSMIC�n�iMGV5�j�́j�V�����_�[�̎g�p�̋��^�s����ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niUseCylinderAccept">�V�����_�[�̎g�p��������i1�j���s���ɂ���i0�j</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetCylinderMode(int niUseCylinderAccept)
        {
            const string cstr_command = "/SetCylinderMode";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niUseCylinderAccept.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSCAMPCTime
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�iKEISOKU.EXE�����삵�Ă���jPC�̌��݂̓������擾���܂�
        /// </summary>
        /// 
        /// <param name="niTimeMode">
        /// 
        /// <para>�V�X�e�������i���E�W�������j�Ŏ擾����ꍇ��0</para>
        /// <para>���[�J�������i����PC�̐ݒ�n��̎����j�Ŏ擾����ꍇ��1</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int GetSCAMPCTime(int niTimeMode)
        {
            const string cstr_command = "/GetSCAMPCTime";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niTimeMode.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region SetSCAMPCTime
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�iKEISOKU.EXE�����삵�Ă���jPC�̌��݂̓������X�V���܂�
        /// </summary>
        /// 
        /// <param name="niTimeMode">
        /// 
        /// <para>�V�X�e�������i���E�W�������j�Ŏ擾����ꍇ��0</para>
        /// <para>���[�J�������i����PC�̐ݒ�n��̎����j�Ŏ擾����ꍇ��1</para>
        /// 
        /// </param>
        /// <param name="niYear">�N�i����S��)</param>
        /// <param name="niMonth">���i�P����1...�P�Q����12�j</param>
        /// <param name="niDay">�j���i���j����0�A���j����1�A�y�j����6)</param>
        /// <param name="niDate">��</param>
        /// <param name="niHour">��</param>
        /// <param name="niMinute">��</param>
        /// <param name="niSecond">�b</param>
        /// <param name="nimmSecond">�~���b</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetSCAMPCTime(int niTimeMode, int niYear, int niMonth, int niDay, int niDate,
                                 int niHour, int niMinute, int niSecond, int nimmSecond)
        {
            const string cstr_command = "/SetSCAMPCTime";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
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
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region SetMeasurementResultCorrection
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	���茋�ʕ␳�@�\�̐���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// <param name="niMeasurementResultCorrectionEnable">	���茋�ʕ␳�@�\��L���ɂ���(1)����(0)��</param>
        /// <param name="nstrMeasurementResultCorrectionDataFilePath">
        /// 
        /// <para>(niMeasurementResultCorrectionEnable��1�̏ꍇ)���茋�ʕ␳�̃f�[�^�Z�b�g�t�@�C���p�X��</para>
        /// <para>niMeasurementResultCorrectionEnable��0�̏ꍇ�́i���݂���t�@�C���p�X���ł���K�v�͂Ȃ����j���炩�̕�������Z�b�g����K�v������</para>
        ///  
        /// </param>
        /// <param name="niOutputFileNameOnHeader">CSV�t�@�C���̃w�b�_�[�����ɕ␳�t�@�C�������o�͂���i1�j���ہi0�j��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------  
        public int SetMeasurementResultCorrection(int niMeasurementResultCorrectionEnable,
                                                  string nstrMeasurementResultCorrectionDataFilePath, 
                                                  int niOutputFileNameOnHeader )
        {
            const string cstr_command = "/SetMeasurementResultCorrection";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 �A�X�L�[�R�[�h
            try
            {
                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                if (niMeasurementResultCorrectionEnable == 0)
                {
                    nstrMeasurementResultCorrectionDataFilePath = "dummy";
                }

                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niMeasurementResultCorrectionEnable.ToString());
                list_send_data_list.Add(nstrMeasurementResultCorrectionDataFilePath);
                list_send_data_list.Add(niOutputFileNameOnHeader.ToString());

                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + ((char)i_separetor).ToString() +
                                   list_send_data_list[2] + ((char)i_separetor).ToString() + list_send_data_list[3] + "\n";

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        ///	���茋�ʕ␳�@�\�̐���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetMeasurementResultCorrection()
        {
            const string cstr_command = "/GetMeasurementResultCorrection";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetExImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�����摜�ۑ��@�\�Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetExImageAutoSaveProp()
        {
            const string cstr_command = "/GetExImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetAroundSearchPrm
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	����p�^�[���}�b�`���O�@�\�Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAroundSearchPrm()
        {
            const string cstr_command = "/GetAroundSearchPrm";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetChamberStatusData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���݂̃`�����o�[�̃X�e�[�^�X�����擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetChamberStatusData()
        {
            const string cstr_command = "/GetChamberStatusData";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetAgingProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�u�������s�����̕t���p�����[�^�[�v�̂����A�G�[�W���O�@�\�Ɋւ���p�����[�^�[���擾���܂��B</para>
        /// <para> ���̃p�����[�^�[�́A�u��������|�������s�����|��ʍ��ځv�́u�G�[�W���O���ԁv�����</para>
        /// <para>�u�g�u�G�[�W���O���~�v�{�^����\������g�`�F�b�N�{�b�N�X�v�̒l�ł��B</para>
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetAgingProp()
        {
            const string cstr_command = "/GetAgingProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetAgingProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�u�������s�����̕t���p�����[�^�[�v�̂����A�G�[�W���O�@�\�Ɋւ���p�����[�^�[���X�V���܂��B</para>
        /// <para> ���̃p�����[�^�[�́A�u��������|�������s�����|��ʍ��ځv�́u�G�[�W���O���ԁv�����</para>
        /// <para>�u�g�u�G�[�W���O���~�v�{�^����\������g�`�F�b�N�{�b�N�X�v�̒l�ł��B</para>
        /// </summary>
        /// 
        /// <param name="niAgingTime">�G�[�W���O���ԁi�b�P�ʁj</param>
        /// <param name="niShowAgingStopButton">
        /// 
        /// <para>1�F�u�G�[�W���O���~�v�{�^����\������</para>
        /// <para>0�F�u�G�[�W���O���~�v�{�^����\�����Ȃ�</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetAgingProp(int niAgingTime, int niShowAgingStopButton )
        {
            const string cstr_command = "/SetAgingProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAgingTime.ToString());
            list_send_data_list.Add(niShowAgingStopButton.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWatchParamFileNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�p�����[�^�[�t�@�C���Ď����X�g�ɓo�^����Ă���t�@�C�������擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWatchParamFileNum()
        {
            const string cstr_command = "/GetWatchParamFileNum";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetWatchParamFileInfo
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�w�肳�ꂽ�Ď��Ώۃt�@�C���̏����擾���܂�
        /// </summary>
        /// 
        /// <param name="niRegisterFileNoOnList">�p�����[�^�[�t�@�C���Ď����X�g��X�Ԗڂɓo�^���ꂽ�t�@�C��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetWatchParamFileInfo(int niRegisterFileNoOnList)
        {
            const string cstr_command = "/GetWatchParamFileInfo";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niRegisterFileNoOnList.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetCurrentData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�w�肳�ꂽ���̃��[�^�[���ח����擾���܂�
        /// </summary>
        /// 
        /// <param name="niAxisNo">���[�^�[���ח����擾���鎲�ԍ�(0�`)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetCurrentData(int niAxisNo)
        {
            const string cstr_command = "/GetCurrentData";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxisNo.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region UserManager_Login
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	<para>�w�肵�����[�U�[���A�p�X���[�h��SCAM�̃��[�U�[�}�l�[�W���[�Ƀ��O�C�����܂��B</para>
        ///	<para>���ɕʂ̃��[�U�[�����O�C���ς݂̏ꍇ�́A���[�U�[�̓���ւ����������܂�</para>
        /// </summary>
        /// 
        /// <param name="nstrUserName">���[�U�[��</param>
        /// <param name="nstrPassword">�p�X���[�h</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int UserManager_Login(string nstrUserName, string nstrPassword)
        {
            const string cstr_command = "/UserManager_Login";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 �A�X�L�[�R�[�h
            try
            {
                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(nstrUserName);
                list_send_data_list.Add(nstrPassword);

                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";
                int i_ret;
                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1] + ((char)i_separetor).ToString() +
                                   list_send_data_list[2] + "\n";

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        ///	����SCAM�̃��[�U�[�}�l�[�W���[�Ƀ��O�C������Ă��郆�[�U�[�����擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetCurrentLoginUserName()
        {
            const string cstr_command = "/UserManager_GetCurrentLoginUserName";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region UserManager_GetUserNum
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// SCAM�̃��[�U�[�}�l�[�W���[�Ɍ��ݓo�^����Ă���Г��G���W�j�A�p�ȊO�̃��[�U�[�����擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetUserNum()
        {
            const string cstr_command = "/UserManager_GetUserNum";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region UserManager_GetUserList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	SCAM�̃��[�U�[�}�l�[�W���[�Ɍ��ݓo�^����Ă���Г��G���W�j�A�p�ȊO�̃��[�U�[�̃��[�U�[���A�p�X���[�h�A���������̃��X�g���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int UserManager_GetUserList()
        {
            const string cstr_command = "/UserManager_GetUserList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ReadCodeData
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	2�����R�[�h��ǂݎ��
        /// </summary>
        /// 
        /// <param name="niCode">
        /// 
        /// <para>0�FDataMatrix</para>
        /// <para>1�FQR code</para>
        /// <para>2�FVeri Code</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int ReadCodeData(int niCode)
        {
            const string cstr_command = "/ReadCodeData";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCode.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerGeneralParameterGet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���V�s���[�J�[�ɂ�鎩������Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RecipeMakerGeneralParameterGet()
        {
            const string cstr_command = "/RecipeMakerGeneralParameterGet";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerGeneralParameterSet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���V�s���[�J�[�ɂ�鎩������Ɋւ���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niParamNum">���̈����������A�̃p�����[�^�[�̌��i�����_�ł͍ő�4�j</param>
        /// <param name="niP">
        /// 
        /// <para>P[0]:���V�s���[�J�[�ɂ��u�A���C�����g�{����v�̎�������Łu�A���C�����g�}�N���v�̎��s���I�������Ƃ��ɁA</para>
        /// <para>     ���̎��_�ō쐬���ꂽ���[�N���W�n������W�n�Ƃ���i1�j���ہi0�j��</para>
        /// <para>---</para>
        /// <para>P[1]:���V�s���[�J�[�ɂ��u�A���C�����g�{����v�̎�������Łu�A���C�����g�}�N���v�̎��s���I�������Ƃ��ɁA</para>
        /// <para>     ���̎��_�ō쐬���ꂽ���[�N���W�n������W�n�Ƃ���ۂ̎����]���@�ɂ���</para>
        /// <para>0�F���]���Ȃ�</para>
        /// <para>1�FX���𔽓]����</para>
        /// <para>2�FY���𔽓]����</para>
        /// <para>---</para>
        /// <para>P[2]:���V�s���[�J�[�ɂ�鑪��o�H�@�\�Ɋւ���p�����[�^�[�i��g�j�ɂ���</para>
        /// <para>0�F���V�s�o�^��</para>
        /// <para>1�FTSP�A���S���Y��</para>
        /// <para>2�F�I���������f���o�H</para>
        /// <para>---</para>
        /// <para>P[3]:���V�s���[�J�[�ɂ�鑪��o�H�@�\�Ɋւ���p�����[�^�[�iP[2]=2�̂Ƃ��̃��f���o�H�j�ɂ���</para>
        /// <para>0�F�c����</para>
        /// <para>1�F������</para>
        /// <para>2�F�c�����i�Q</para>
        /// <para>3�F�������i�Q�j</para>
        /// <para>4�F�ŋߗאڕ��� </para>
        /// <para>5�F�ŒZ���f���o�H</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
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
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niParamNum.ToString());
                for (int i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    list_send_data_list.Add(niP[i_loop].ToString());
                }
            }

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region RecipeMakerForSkipAreaParameterGet
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���V�s���[�J�[�ɂ�鎩������ɂ�����X�L�b�v�G���A�Ή��Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int RecipeMakerForSkipAreaParameterGet()
        {
            const string cstr_command = "/RecipeMakerForSkipAreaParameterGet";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSaveHardcopyDialogboxParam
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �n�[�h�R�s�[�ۑ��Ɋւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// <param name="niDialogBoxType">�Ώۃ_�C�A���O�{�b�N�X�i1�F�\�t�g�E�F�A�W���C�X�e�B�b�N�A2�F�}�b�v�\��</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSaveHardcopyDialogboxParam(int niDialogBoxType )
        {
            const string cstr_command = "/GetSaveHardcopyDialogboxParam";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niDialogBoxType.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetStopAutoMeasurementProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�u�������s�����̕t���p�����[�^�[�v�̂����A����G���[���Ȃǂ̏����Ŏ������s�𒆎~���邱�ƂɊւ���p�����[�^�[���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetStopAutoMeasurementProp()
        {
            const string cstr_command = "/GetStopAutoMeasurementProp";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetLightStatusEx
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�w�肵���������ɑ�����Ɩ����u�̏�Ԃ��擾���܂��B���̃R�}���h�͊����́uLightStaus�v�R�}���h�̊g���łł�
        /// </summary>
        /// 
        /// <param name="niLightType">
        /// 
        /// <para>�ǂ̌������ɑ�����Ɩ����u�̏�Ԃ��擾���邩</para>
        /// <para>0�F���C���������ɑ�����Ɩ����u�i��f���A���������p�����[�^�[�̏ꍇ���܂ށj</para>
        /// <para>1�F�T�u�������ɑ�����Ɩ����u</para>
        /// <para>2�F���ݑI������Ă���J�����i�{���j�������錰�����ɑ�����Ɩ����u</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetLightStatusEx(int niLightType)
        {
            const string cstr_command = "/GetLightStatusEx";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niLightType.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, true);
        }
        #endregion

        #region GetLightONWhenFreezeEtcFunctionOnOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	<para>�������s���͉摜��荞�݁E�R���g���X�g�@AF�ȂǕK�v�ȂƂ��̂ݏƖ�����@�\�Ɋւ���</para>
        ///	<para>�p�����[�^�[�̂����A�@�\���쓮�����邩�ۂ��̐ݒ���擾���܂�</para>
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetLightONWhenFreezeEtcFunctionOnOff()
        {
            const string cstr_command = "/GetLightONWhenFreezeEtcFunctionOnOff";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetLightONWhenFreezeEtcFunctionOnOff
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�������s���͉摜��荞�݁E�R���g���X�g�@AF�ȂǕK�v�ȂƂ��̂ݏƖ�����@�\�Ɋւ���p�����[�^�[�̂����A�@�\���쓮�����邩�ۂ��̐ݒ��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niParam">
        /// 
        /// <para>0�F�������s���͉摜��荞�݁E�R���g���X�g�@AF�ȂǕK�v�ȂƂ��̂ݏƖ�����@�\�͖����ł���</para>
        /// <para>1�F�������s���͉摜��荞�݁E�R���g���X�g�@AF�ȂǕK�v�ȂƂ��̂ݏƖ�����@�\�͗L���ł���B</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetLightONWhenFreezeEtcFunctionOnOff(int niParam)
        {
            const string cstr_command = "/SetLightONWhenFreezeEtcFunctionOnOff";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niParam.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SelectCamera
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�J�����i�{���j�؂�ւ����s���܂��B</para>
        /// <para>���̃R�}���h��SCAM�̌������V�X�e���i�f���A���������p�����[�^�[�^����{�p�����[�^�[�^�i��f���A���j�Y�[���E���{���o�[�p�����[�^�[�j�ɂ�炸 </para>   
        /// <para>SCAM�̃��[���ɏ]�����J�����i�{���j�ԍ����w�肷�邱�Ƃŏ������s���܂�</para>
        /// </summary>
        /// 
        /// <param name="niCameraNo">
        /// 
        /// <para>�؂�ւ���J�����i�{���j�ԍ�</para>
        /// <para>���f���A���������p�����[�^�[�̏ꍇ��</para>
        /// <para>���C�����������̃J��������M�A�T�u���������̃J��������S�Ƃ����0�i���C�����������Œ�{���j,..,M-1�i���C�����������ō��{���j,M�i�T�u���������Œ�{���j,�c,M+S-1�i�T�u���������ō��{���j</para>
        /// <para>������{�p�����[�^�[�̏ꍇ��</para>
        /// <para>0�F���{�A1�F��{</para>
        /// <para>���i��f���A���j�Y�[���E���{���o�[�p�����[�^�[�̏ꍇ��</para>
        /// <para>�Y�[���E���{���o�[�{������M�Ƃ���ƁA</para>
        /// <para>0�i�Œ�{���j,..,M-1�i�ō��{���j</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SelectCamera(int niCameraNo)
        {
            const string cstr_command = "/SelectCamera";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niCameraNo.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region TableMoveNoWaitStart
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///<para>XY���e�[�u���ړ����J�n���܂��B���̃R�}���h�ł͊J�n�̂ݍs���܂��B</para>
        ///<para>�ړ������̊m�F�ɂ�TableMoveNoWaitCheck�R�}���h���g�p���܂�</para>
        /// </summary>
        /// 
        /// <param name="nbABSMove">true:��Βl�ړ��@false:���Βl�ړ�</param>
        /// <param name="ndX">X���W</param>
        /// <param name="ndY">Y���W</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int TableMoveNoWaitStart(bool nbABSMove, double ndX, double ndY)
        {
            const string cstr_command = "/TableMoveNoWaitStart";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ��Βl�ړ�
            if (nbABSMove == true)
            {
                list_send_data_list.Add("0");
            }
            //  ���Βl�ړ�
            else
            {
                list_send_data_list.Add("1");
            }
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat)); 
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region TableMoveNoWaitCheck
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	TableMoveNoWaitStart�R�}���h�ňړ��J�n����XY���e�[�u���ړ��̏�Ԃ��擾���܂�
        /// </summary>
        /// 
        /// <param name="niBeforeCheckProcess">
        /// 
        /// <para>�`�F�b�N�O�̑ҋ@����</para>
        /// <para>1:�ҋ@����A0:�ҋ@���Ȃ�</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int TableMoveNoWaitCheck(int niBeforeCheckProcess)
        {
            const string cstr_command = "/TableMoveNoWaitCheck";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niBeforeCheckProcess.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ReadRawCount
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�w�莲�̃J�E���g���l���擾���܂�
        /// </summary>
        /// 
        /// <param name="niAxisNo">
        /// 
        /// <para>���ԍ�</para>
        /// <para>0:X</para>
        /// <para>1:Y</para>
        /// <para>2:Y2</para>
        /// <para>3:Z</para>
        /// <para>4:XP</para>
        /// <para>5:YP</para>
        /// <para>6:Tracker</para>
        /// <para>7:tiny_y</para>
        /// <para>����ȊO:X</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int ReadRawCount(int niAxisNo)
        {
            const string cstr_command = "/ReadRawCount";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niAxisNo.ToString());
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region ConvertWorkPos
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�w�胏�[�N���W���@�B���W�ɕϊ����܂�
        /// </summary>
        /// 
        /// <param name="niConvertType">
        /// 
        /// <para>�ϊ����</para>
        /// <para>0:���[�N���W->�@�B���W</para>
        /// <para>1:���[�N���W->�e�[�u���␳���s��Ȃ��p���X�l</para>
        /// 
        /// </param>
        /// <param name="ndX">X���W(���[�N���W)</param>
        /// <param name="ndY">Y���W(���[�N���W)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int ConvertWorkPos(int niConvertType, double ndX, double ndY)
        {
            const string cstr_command = "/ConvertWorkPos";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(niConvertType.ToString());
            list_send_data_list.Add(ndX.ToString(strDoublePointFormat));
            list_send_data_list.Add(ndY.ToString(strDoublePointFormat));
            //  ���X�g���瑗�M��������쐬
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetTableSpeed
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���݂̃e�[�u���X�s�[�h���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetTableSpeed()
        {
            const string cstr_command = "/GetTableSpeed";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetTableSpeed
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�ʏ푪��̈ړ��X�s�[�h(pulse/sec)��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="niParamNum">
        /// 
        /// <para>�p�����[�^�[��</para>
        /// <para>Sinto LA�A�ψʌv�Ȃ��c1��ݒ肷��(P2�ɐݒ�)</para>
        /// <para>Sinto LA�c2��ݒ肷��(P2�AP3�ɐݒ�)</para>
        /// <para>�ψʌv�c4��ݒ肷��(P2�AP3�AP4�AP5�ɐݒ�)</para>
        /// 
        /// </param>
        /// <param name="ndP">
        /// 
        /// ndP[0]:P2  �ʏ푪��p�X�s�[�h
        /// ndP[1]:P3  �ψʌv����p(����16��)�X�s�[�h
        /// ndP[2]:P4  �ψʌv����p(����128��)�X�s�[�h
        /// ndP[3]:P5  �ψʌv����p(����2��)�X�s�[�h
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetTableSpeed(int niParamNum, double [] ndP )
        {
            int i_loop;
            const string cstr_command = "/SetTableSpeed";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 �A�X�L�[�R�[�h
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
                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }
                
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                list_send_data_list.Add(niParamNum.ToString());

                for (i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    list_send_data_list.Add(ndP[i_loop].ToString(strDoublePointFormat));
                }
                //  ���X�g���瑗�M��������쐬
                string str_send_string = "";

                str_send_string = list_send_data_list[0] + " " + list_send_data_list[1];
                for (i_loop = 0; i_loop < niParamNum; i_loop++)
                {
                    str_send_string += ((char)i_separetor).ToString();
                    str_send_string += list_send_data_list[2 + i_loop];
                }
                str_send_string += "\n";

                int i_ret;

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
        /// Z���\�t�g�E�F�A���~�b�g���̈ꗗ���擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetZSoftwareLimitList()
        {
            const string cstr_command = "/GetZSoftwareLimitList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetZSoftwareLimitCurrentName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	Z���\�t�g�E�F�A���~�b�g�̖��̂�ݒ肵�܂��B�o�^����Ă��郊�~�b�g�f�[�^�Ɠ������̂����݂���ꍇ�́A���̃f�[�^�ɕύX����܂�
        /// </summary>
        /// 
        /// <param name="nstrZSoftLimitName">
        /// 
        /// <para>	Z���\�t�g�E�F�A���~�b�g�̖���</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int SetZSoftwareLimitCurrentName(string nstrZSoftLimitName)
        {
            const string cstr_command = "/SetZSoftwareLimitCurrentName";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrZSoftLimitName);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetZSoftwareLimitCurrentName
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	���݂�Z���\�t�g�E�F�A���~�b�g�̖��̂��擾���܂�
        /// </summary>
        /// 
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetZSoftwareLimitCurrentName()
        {
            const string cstr_command = "/GetZSoftwareLimitCurrentName";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�����摜�ۑ��@�\�Ɋւ���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrImageAutoSavePropArray">
        /// 
        /// <para>�����摜�ۑ��p�����[�^�[�z��</para>
        /// <para>nstrImageAutoSavePropArray=P�Ƃ��Đ���</para>
        /// <para>----</para>
        /// <para>P[0]:�����摜�ۑ��@�\���L���i1�j���ہi0�j��</para>
        /// <para>P[1]:0:�G���[�ӏ��A��G���[�ӏ��Ƃ��ɕۑ��A1:�G���[�ӏ��̂ݕۑ��A2:��G���[�ӏ��̂ݕۑ�</para>
        /// <para>P[2]:�đ��肪���������Ƃ��̕ۑ����@�ɂ���</para>
        /// <para>0:�đ���O�̉摜�͍폜�A1:�đ���O�̉摜���c��</para>
        /// <para>�i�ǂ���̏ꍇ���đ��莞�̉摜�͕ۑ������j</para>
        /// <para>P[3]:����t�@�C�����̉摜�Ɋւ���</para>
        /// <para>0:�㏑�����Ȃ��i�v���O��������`����ʖ��ŕۑ��j�A1:�㏑������</para>
        /// <para>P[4]:0:�����摜�ۑ��������[�h�����A1:�����摜�ۑ��������[�h�L��</para>
        /// <para>P[5]:0:JPEG�`���ŕۑ��A1:BMP�`���ŕۑ�</para>
        /// <para>P[6]:JPEG�`���ŕۑ�����ۂ̕i���i1�`100�Ŏw��j</para>
        /// <para>P[7]:�摜�ۑ���i�x�[�X�f�B���N�g���[�j</para>
        /// <para>P[8]:�摜�ۑ���i�f�B���N�g���[�P�j�A���̓��A�����ꂩ�̕����񂪕Ԃ����</para>
        /// <para>�u���茋�ʃt�@�C�����v�A�u����}�N���t�@�C�����v�A�u�J�n�����v�A�u�C�Ӂv�A�u�Ȃ��v</para>
        /// <para>P[9]:P[8]���u�C�Ӂv�̏ꍇ�̃f�B���N�g���[��</para>
        /// <para>P[10]:�摜�ۑ���i�f�B���N�g���[�P�j�Ɋւ�������t�H�[�}�b�g������������</para>
        /// <para>P[11]:�摜�ۑ���i�f�B���N�g���[�Q�j�A���̓��A�����ꂩ�̕����񂪕Ԃ����</para>
        /// <para>�u���茋�ʃt�@�C�����v�A�u����}�N���t�@�C�����v�A�u�J�n�����v�A�u�C�Ӂv�A�u�Ȃ��v</para>
        /// <para>P[12]:P[11]���u�C�Ӂv�̏ꍇ�̃f�B���N�g���[��</para>
        /// <para>P[13]:�摜�ۑ���i�f�B���N�g���[�Q�j�Ɋւ�������t�H�[�}�b�g������������</para>
        /// <para>P[14]:�{�@�\�ɂ�����摜�폜���@�Ɋւ���  0:�}�j���A�����[�h�A1:�I�[�g���[�h</para>
        /// <para>P[15]:�摜�ۑ�������1�`365�̒l�Ŏw��</para>
        /// <para>P[16]:�I�[�g���[�h�ŉ摜�폜����ہA�`�F�b�N���s������������1�`32767�̒l�Ŏw��</para>
        /// <para>P[17]:�I�[�g���[�h�ŉ摜�폜����ہA���[�U�[�ւ̊m�F�_�C�A���O�{�b�N�X��\������i1�j���ہi0�j�����w��</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetImageAutoSaveProp(string[] nstrImageAutoSavePropArray)
        {
            const string cstr_command = "/SetImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();

            //  �����̐�����v���Ȃ���΃G���[
            if (nstrImageAutoSavePropArray.Count() != 18)
            {
                return -1;
            }
            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            foreach (string str in nstrImageAutoSavePropArray)
            {
                list_send_data_list.Add(str);
            }
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region SetExImageAutoSaveProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// 	�����摜�ۑ��@�\�Ɋւ���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrImageAutoSavePropArray">
        /// 
        /// <para>�����摜�ۑ��p�����[�^�[�z��</para>
        /// <para>nstrImageAutoSavePropArray=P�Ƃ��Đ���</para>
        /// <para>----</para>
        /// 
        /// <para>P[0]:�ݒ肷��p�����[�^�[�̌��i����̃o�[�W�����ł͍ő�28�j</para>
        /// <para>----</para>
        /// <para>P[1]:�����摜�ۑ��@�\���L���i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[2]:�ۑ�����摜�Ɋւ���</para>
        /// <para>0�F�G���[�ӏ��A��G���[�ӏ��Ƃ��ɕۑ�</para>
        /// <para>1�F�G���[�ӏ��̂ݕۑ�</para>
        /// <para>2�F��G���[�ӏ��̂ݕۑ�</para>
        /// <para>----</para>
        /// <para>P[3]:�đ��肪���������Ƃ��̕ۑ����@�Ɋւ��āA0�F�đ���O�̉摜�͍폜�A1�F�đ���O�̉摜���c��</para>
        /// <para>----</para>
        /// <para>P[4]:����t�@�C�����̉摜�Ɋւ��āA0�F�㏑�����Ȃ��i�v���O��������`����ʖ��ŕۑ��j�A1�F�㏑������</para>
        /// <para>----</para>
        /// <para>P[5]:�����摜�ۑ��������[�h���L���i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[6]:�摜�t�@�C���̃t�H�[�}�b�g  0�FJPEG�`���A1�FBMP�`��</para>
        /// <para>----</para>
        /// <para>P[7]:JPEG�`���ŕۑ�����ۂ̕i���i1�`100�Ŏw��j</para>
        /// <para>----</para>
        /// <para>P[8]:�摜�ۑ���i�x�[�X�f�B���N�g���[�j</para>
        /// <para>----</para>
        /// <para>P[9]:�摜�ۑ���i�f�B���N�g���[�P�j�A�@�`�D�̂����ꂩ���w�肷��B</para>
        /// <para>�@���茋�ʃt�@�C����</para>
        /// <para>�A����}�N���t�@�C����</para>
        /// <para>�B�J�n����</para>
        /// <para>�C�C��</para>
        /// <para>�D�Ȃ�</para>
        /// <para>----</para>
        /// <para>P[10]:P[9]���C�̏ꍇ�̃f�B���N�g���[��</para>
        /// <para>----</para>
        /// <para>P[11]:�摜�ۑ���i�f�B���N�g���[�P�j�Ɋւ�������t�H�[�}�b�g������������</para>
        /// <para>----</para>
        /// <para>P[12]:�摜�ۑ���i�f�B���N�g���[�Q�j�A�@�`�D�̂����ꂩ���w�肷��B</para>
        /// <para>�@���茋�ʃt�@�C����</para>
        /// <para>�A����}�N���t�@�C����</para>
        /// <para>�B�J�n����</para>
        /// <para>�C�C��</para>
        /// <para>�D�Ȃ�</para>
        /// <para>----</para>
        /// <para>P[13]:P[12]���C�̏ꍇ�̃f�B���N�g���[��</para>
        /// <para>----</para>
        /// <para>P[14]:�摜�ۑ���i�f�B���N�g���[�Q�j�Ɋւ�������t�H�[�}�b�g������������</para>
        /// <para>----</para>
        /// <para>P[15]:�{�@�\�ɂ�����摜�폜���@�Ɋւ��āA0�F�}�j���A�����[�h�A1�F�I�[�g���[�h</para>
        /// <para>----</para>
        /// <para>P[16]:�摜�ۑ������i1�`365�j</para>
        /// <para>----</para>
        /// <para>P[17]:�I�[�g���[�h�ŉ摜���폜����ۂ̃`�F�b�N���s�����������i1�`32767�j</para>
        /// <para>----</para>
        /// <para>P[18]:�I�[�g���[�h�ŉ摜���폜����ہA���[�U�[�ւ̊m�F�_�C�A���O�{�b�N�X��\������i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[19]:�摜�t�@�C���ɃO���t�B�b�N�X���܂߂���@�ɂ��āA</para>
        /// <para>0�F�O���t�B�b�N�X���܂߂Ȃ��摜�݂̂�ۑ�����B</para>
        /// <para>1�F�O���t�B�b�N�X���܂߂��摜�݂̂�ۑ�����B</para>
        /// <para>2�F�O���t�B�b�N�X���܂߂Ȃ��^�܂߂��摜�Ƃ��ɕۑ�����</para>
        /// <para>----</para>
        /// <para>P[20]:�摜�t�@�C���ɑ���������L�^����i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[21]:���胊�g���C�@�\���쓮�����Ƃ��Ƀ��g���C���̉摜��ۑ�����i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[22]:�����摜�ۑ��t�@�C�����̖����K���̊g���ɂ����āA�u�W���̃t�@�C�����v�Ɓu�g�������v�̘A�����@�Ɋւ��āA</para>
        /// <para>1�F�u�W���̃t�@�C�����v�̂�</para>
        /// <para>2�F�u�g�������v�̂�</para>
        /// <para>3�F�u�W���̃t�@�C����_�g�������v</para>
        /// <para>4�F�u�g������_�W���̃t�@�C�����v</para>
        /// <para>----</para>
        /// <para>P[23]:�����摜�ۑ��t�@�C�����̖����K���̊g���ɂ����āA�u�g�������v�̍\���v�f������1�`8��g�ݍ��킹��������ŕԂ��B�Z�p���[�^�[�������ASCII�R�[�h0x02�̕����Ƃ���B�Ȃ��A�\���v�f�����݂��Ȃ��ꍇ��Undefined��Ԃ�</para>
        /// <para>1�F����̎�ށi�A���C�����g�Ƒ���j�Ǝ�ނ��Ƃ̘A��</para>
        /// <para>2�F����Ɏg�p�����J�����i�{���j�̖���</para>
        /// <para>3�F����Ɏg�p�������@�I���A�C�R���iSOKMAC�j�̔ԍ�</para>
        /// <para>4�F����Ɏg�p�������@�I���A�C�R���iSOKMAC�j�̖��́i�ő�31�o�C�g�j</para>
        /// <para>5�F����Ɏg�p�����Ɩ��̖���</para>
        /// <para>6�F����Ɏg�p�����Ɩ��̐ݒ�Ɠx</para>
        /// <para>7�F����Ɏg�p�����摜�t�B���^�[�̕��@</para>
        /// <para>8�F���茋�ʂɑ΂���R�����g�i�ő�31�o�C�g�j</para>
        /// <para>----</para>
        /// <para>P[24]:�u���茋�ʂɂ��摜�ۑ�������K�p������@�v�Ɋւ��āA</para>
        /// <para>1�F���ׂēK�p����B</para>
        /// <para>2�F�Ō�̃A���C�����g�܂œK�p����B�u�Ō�̃A���C�����g�v�Ƃ͎��̂悤�ɒ�`����B</para>
        /// <para>�@�F���V�s���[�J�[�ȊO�Ŏ������s�����ꍇ</para>
        /// <para>�Ō�ɓo�ꂵ���A���C�����g��SOKMAC���g�p���ꂽ���茋�ʔԍ�</para>
        /// <para>�A�F���V�s���[�J�[�Ŏ������s�����ꍇ</para>
        /// <para>�u�A���C�����g�}�N���v�Ƃ��Ď��s���ꂽ�Ō�̑��茋�ʔԍ��i�A���C�����g�}�N�����A���C�����g��SOKMAC�ł��邩�͕]�����Ȃ��j</para>
        /// <para>3�F�Ō�̃A���C�����g�̎��̑��肩��K�p����B</para>
        /// <para>4�F�w�肵�����茋�ʔԍ��ɂ��ēK�p����B</para>
        /// <para>5�F�w�肵��SOKMAC�ԍ��ɂ��ēK�p����B</para>
        /// <para>----</para>
        /// <para>P[25]:P[24]�ɂ��鑪�茋�ʔԍ���\�킷������i�J���}��؂�ɂ�镡���w��A�n�C�t���ɂ��A���w��j�ɐ擪�Ɩ������_�u���R�[�e�[�V�����}�[�N��t���������́B�Ⴆ�΁A���Y�����񂪉����Ȃ��ꍇ�́u�h�h�v�ƂȂ�i�u�v�͂Ȃ��j�B</para>
        /// <para>----</para>
        /// <para>P[26]:P[24]�ɂ���SOKMAC�ԍ���\�킷������i�J���}��؂�ɂ�镡���w��A�n�C�t���ɂ��A���w��j�ɐ擪�Ɩ������_�u���R�[�e�[�V�����}�[�N��t���������́B�Ⴆ�΁A���Y�����񂪉����Ȃ��ꍇ�́u�h�h�v�ƂȂ�i�u�v�͂Ȃ��j�B</para>
        /// <para>----</para>
        /// <para>P[27]:�K�p�����𖞂����Ȃ��ꍇ�ɁA����G���[�łȂ��摜��ۑ�����i1�j���ہi0�j��</para>
        /// <para>----</para>
        /// <para>P[28]:�K�p�����𖞂����Ȃ��ꍇ�ɁA����G���[�̉摜��ۑ�����i1�j���ہi0�j��</para>
        /// 
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetExImageAutoSaveProp(string[] nstrImageAutoSavePropArray)
        {
            int i_wrk;
            int i_loop;
            const string cstr_command = "/SetExImageAutoSaveProp";
            List<string> list_send_data_list = new List<string>();
            int i_separetor = 1;    //  0x01 �A�X�L�[�R�[�h
            try
            {
                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  �����̐�����v���Ȃ���΃G���[
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

                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                foreach (string str in nstrImageAutoSavePropArray)
                {
                    list_send_data_list.Add(str);
                }

                //  ���X�g���瑗�M��������쐬
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

                //  ���M
                i_ret = Send(str_send_string);
				if (i_ret == 0)
				{
					m_bSentRemoteCommand = true;
					//	�������R�}���h��������o���Ă���
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
		/// RapidMode�����s���܂�
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">���V�s�t�@�C����(�t���p�X)</param>
		/// <param name="nstrLayerName">���C���[��</param>
		/// <param name="nstrComment">�R�����g</param>
		/// <param name="nstrResultPath">���茋�ʃt�@�C����(�t���p�X)</param>
		/// <param name="nstrNameListArray">��������s���閼�O���X�g�i���O1[sep]���O2[sep]������ON[sep]�j</param>
		/// 
		/// <returns>
		///    0:����
		///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
		///   -2:������I�[�o�[
		///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
		///   -4:2�x����G���[
		///   -10:����ȊO�̃G���[
		/// </returns>
		/// ----------------------------------------------------------------------------------------
		public int RunRapidMode( string nstrRecipePath, string nstrLayerName, string nstrComment, string nstrResultPath,
								 string[] nstrNameListArray)
		{
			const string cstr_command = "/RunRapidMode";
			List<string> list_send_data_list = new List<string>();
			int i_separetor = 1;    //  0x01 �A�X�L�[�R�[�h
			string str_name_list = "";

			try
			{
				//  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
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
				//  ���X�g���瑗�M��������쐬
				//  ���M
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
		/// 	���V�s�t�@�C���ɐݒ肳��Ă���RapidMode�̖��O���X�g���擾���܂�
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">���V�s�t�@�C����(�t���p�X)</param>
		/// 
		/// <returns>
		///    0:����
		///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
		///   -2:������I�[�o�[
		///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
		///   -4:2�x����G���[
		///   -10:����ȊO�̃G���[
		/// </returns>
		/// ---------------------------------------------------------------------------------------- 
		public int GetRapidModeNameList(string nstrRecipePath)
		{
			const string cstr_command = "/GetRapidModeNameList";
			List<string> list_send_data_list = new List<string>();

			//  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
			list_send_data_list.Add(cstr_command);
			list_send_data_list.Add(nstrRecipePath);
			//  ���M
			return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
		}
		#endregion

		#region GetRapidModeNameData
		/// ---------------------------------------------------------------------------------------- 
		/// <summary>
		/// 	�w�肵�����O�̑���ʒu���X�g�A�R�����g�����擾���܂�
		/// </summary>
		/// 
		/// <param name="nstrRecipePath">���V�s�t�@�C����(�t���p�X)</param>
		/// <param name="nstrName">���O</param>
		/// 
		/// <returns>
		///    0:����
		///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
		///   -2:������I�[�o�[
		///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
		///   -4:2�x����G���[
		///   -10:����ȊO�̃G���[
		/// </returns>
		/// ---------------------------------------------------------------------------------------- 		
		public int GetRapidModeNameData(string nstrRecipePath, string nstrName)
		{
			const string cstr_command = "/GetRapidModeNameData";
			List<string> list_send_data_list = new List<string>();

			//  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
			list_send_data_list.Add(cstr_command);
			list_send_data_list.Add(nstrRecipePath);
			list_send_data_list.Add(nstrName);
			//  ���M
			return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
		}
		#endregion

        #region SetRemeasureProp
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///	�I�[�g�P�A�����[�g�ł̎������s�ɑ΂���đ���Ɋւ���p�����[�^�[��ݒ肵�܂�
        /// </summary>
        /// 
        /// <param name="nstrRemeasurePropArray">
        /// <para>�������s�����p�����[�^�[�z��</para>
        /// <para>nstrRemeasurePropArray=P�Ƃ��Đ���</para>
        /// <para>----</para>
        /// <para>P[1]:�A���C�������g�̍đ�����s���i1�j���ہi0�j��</para>
        /// <para>P[2]:�A���C�������g�ȊO�̍đ�����s���i1�j���ہi0�j��</para>
        /// <para>P[3]:��������G���[���đ���̑ΏۂƂ���i1�j���ہi0�j��</para>
        /// <para>P[4]:�������s�I����̍đ�����s���i1�j���ہi0�j��</para>
        /// <para>P[5]:����G���[���������邲�Ƃɍđ�����s���i1�j���ہi0�j��</para>
        /// <para>P[6]:����G���[���������邲�Ƃɍđ�����s���Ƃ��̑ΏۂƂȂ鑪��</para>
        /// <para>      0�F���ׂĂ̑���,1�F����̑��茋�ʔԍ����O�̂��̂��ׂ�</para>
        /// <para>P[7]:����G���[���������邲�Ƃɍđ�����s���Ƃ��̑ΏۂƂȂ鑪�肪�u����̑��茋�ʔԍ����O�̂��̂��ׂāv�̏ꍇ�̑��茋�ʔԍ�</para>
        /// <para>P[8]:�������s�����_�C�A���O�ŁA�đ��荀�ڂ̐ݒ��������i1�j���ہi0�j��</para>
        /// <para>P[9]:�đ��莞�ɑ���|�C���g���ƂɊm�F(OK�{�^��)���������i1�j���ہi0�j��</para>
        /// <para>P[10]:����G���[�ӏ����Ȃ��Ă��đ�����s���i1�j���ہi0�j��</para>
        /// <para>P[11]:�������s���ɍđ���ӏ��̒ǉ����s���i1�j���ہi0�j��</para>
        /// <para>P[12]:�u�����X�e�b�v���s�v�̑ΏۂƂ��鑪��ɂ��āA</para>
        /// <para>      0�F���ׂĂ̑���</para>
        /// <para>      1�F����̑��茋�ʔԍ����O�̂��̂��ׂ�</para>
        /// <para>      2�F�A���C�����g�Ɋւ�鑪��</para>
        /// <para>      3�F�w�肵������A�C�R��</para>
        /// <para>P[13]:P12=1�̂Ƃ��̑��茋�ʔԍ�</para>
        /// <para>P[14]:P12=2�̂Ƃ��A�A���C�����g�����SOKMAC���g�������X�e�b�v���s�̑ΏۂƂ���i1�j���ہi0�j��</para>
        /// <para>P[15]:P12=2�̂Ƃ��A�A���C�����g�����SOKMAC���������[���p���Ă��鑪��������X�e�b�v���s�̑ΏۂƂ���i1�j���ہi0�j��</para>
        /// <para>P[16]:�������s�I����̍đ��胂�[�h�J�n���Ɂu�đ��肵�܂����H�v�_�C�A���O�{�b�N�X��\������i1�j���ہi0�j��</para>
        /// <para>P[17]:P16=1�̂Ƃ��A���ׂĂ̍đ���ӏ����I�[�g���[�h���܂߂čs���ꍇ�͏��O����i1�j���ہi0�j��</para>
        /// <para>P[18]:�������s�I����̍đ��胂�[�h�J�n���ɍđ���ӏ��ݒ�E�B���h�E��\������i1�j���ہi0�j��</para>
        /// <para>P[19]:P18=1�̂Ƃ��A���ׂĂ̍đ���ӏ����I�[�g���[�h���܂߂čs���ꍇ�͏��O����i1�j���ہi0�j��</para>
        /// <para>P[20]:�������s���̑��茋�ʂ���������G���[�̑���ӏ��̍đ���̓��샂�[�h�ɂ��āA</para>
        /// <para>      1�F�W�����[�h,2�F�I�[�g���[�h,3�F�I�[�g���[�h���W�����[�h</para>
        /// <para>P[21]:P20=2�܂���3�̂Ƃ��A�I�[�g���[�h���쓮����ۂ̍ő僊�g���C��</para>
        /// <para>P[22]:�������s���̑��茋�ʂ���������G���[�ȊO�̑���G���[�̑���ӏ��̍đ���̓��샂�[�h�ɂ��āA</para>
        /// <para>      1�F�W�����[�h,2�F�I�[�g���[�h,3�F�I�[�g���[�h���W�����[�h</para>
        /// <para>P[23]:P22=2�܂���3�̂Ƃ��A�I�[�g���[�h���쓮����ۂ̍ő僊�g���C��</para>
        /// <para>P[24]:�������s���̑��茋�ʂ�����G���[�łȂ�����ӏ��̍đ���̓��샂�[�h�ɂ��āA</para>
        /// <para>      1�F�W�����[�h,2�F�I�[�g���[�h,3�F�I�[�g���[�h���W�����[�h</para>
        /// <para>P[25]:�������s�I����̍đ��胂�[�h�J�n���Ɂu�đ��肵�܂����H�v�_�C�A���O�{�b�N�X���\�����ꂽ�Ƃ��A���莞�Ԍo�ߌ�Ɏ����������s���i1�j���ہi0�j��</para>
        /// <para>P[26]:P25=1�̂Ƃ��̏��莞�ԁi�b�P�ʁj</para>
        /// <para>P[27]:P25=1�̂Ƃ����莞�Ԃ��o�߂����Ƃ��Ɏ����������s���ۂ̏����ɂ��āA</para>
        /// <para>      0�F�u�͂��v�����������������s��,1�F�u�������v�����������������s��</para>
        /// <para>P[28]:�đ���ӏ��ݒ�E�B���h�E���\�����ꂽ�Ƃ��A���A�đ���ӏ��ɑ���G���[���܂ނƂ��A���莞�Ԍo�ߌ�Ɏ����������s���i1�j���ہi0�j��</para>
        /// <para>P[29]:P28=1�̂Ƃ��̏��莞�ԁi�b�P�ʁj</para>
        /// <para>P[30]:P28=1�̂Ƃ����莞�Ԃ��o�߂����Ƃ��Ɏ����������s���ۂ̏����ɂ��āA</para>
        /// <para>      0�F�uOK�v�����������������s��,1�F�u�L�����Z���v�����������������s��</para>
        /// <para>P[31]:�đ���ӏ��ݒ�E�B���h�E���\�����ꂽ�Ƃ��A���A�đ���ӏ��ɑ���G���[���܂܂Ȃ��Ƃ��A���莞�Ԍo�ߌ�Ɏ����������s���i1�j���ہi0�j��</para>
        /// <para>P[32]:P31=1�̂Ƃ��̏��莞�ԁi�b�P�ʁj</para>
        /// <para>P[33]:P31=1�̂Ƃ����莞�Ԃ��o�߂����Ƃ��Ɏ����������s���ۂ̏����ɂ��āA</para>
        /// <para>      0�F�uOK�v�����������������s��,1�F�u�L�����Z���v�����������������s��</para>
        /// <para>P[34]:�����X�e�b�v���s���s���i1�j���ہi0�j��</para>
        /// <para>P[35]:�X�L�b�v�G���A�G���[���đ���̑ΏۂƂ���i1�j���ہi0�j��</para>
        /// <para>P[36]:P12=3�̂Ƃ��A�ΏۂƂ���SOKMAC�ԍ��̃��X�g���X�g�� �gN1,N2,�c�h �̌`���ŋL�q����i�擪�Ɩ����̃_�u���R�[�e�[�V�����}�[�N���܂ށj</para>
        /// <para>P[37]:����G���[�ƌ�������G���[�̍��v�����ݒ肵�����ɂ��đ���̎��s�̗L�������肷�邱�Ƃ��L���ł���i1�j���ہi0�j��</para>
        /// <para>P[38]:����G���[�ƌ�������G���[�̍��v�����ݒ肵�����ɂ��đ���̎��s�̗L�����L���ȏꍇ�̓��샂�[�h�ɂ��āA</para>
        /// <para>      1�F�ݒ萔��菬�����ꍇ�đ�����s��Ȃ��i�ݒ肵�����ȏ�̏ꍇ�đ�����s���j</para>
        /// <para>      2�F�ݒ萔���傫���ꍇ�đ�����s��Ȃ��i�ݒ肵�����ȉ��̏ꍇ�đ�����s���j</para>
        /// <para>P[39]:����G���[�ƌ�������G���[�̍��v�����ݒ肵�����ɂ��đ���̎��s�̗L�������肷��ꍇ�̐ݒ萔</para>
        /// </param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int SetRemeasureProp(string[] nstrRemeasurePropArray)
        {
            const string cstr_command = "/SetRemeasureProp";
            List<string> list_send_data_list = new List<string>();
            try
            {
                int i_wrk;

                //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
                if (m_bSentRemoteCommand == true)
                {
                    return -4;
                }

                //  �����̐�����v���Ȃ���΃G���[
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
                
                //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
                list_send_data_list.Add(cstr_command);
                foreach (string str in nstrRemeasurePropArray)
                {
                    list_send_data_list.Add(str);
                }
                
                //  ���M
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
        /// �T�u�R�����g�Ɋւ���ݒ�����擾����B
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsSubComment()
        {
            const string cstr_command = "/GetSettingsSubComment";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSettingsOutputCondition
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �o�͏����Ɋւ���ݒ�����擾����B
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsOutputCondition()
        {
            const string cstr_command = "/GetSettingsOutputCondition";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region GetSettingsJudgement
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ��������Ɋւ���ݒ�����擾����B
        /// </summary>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetSettingsJudgement()
        {
            const string cstr_command = "/GetSettingsJudgement";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region PMSFeedbackRun
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �����@�t�B�[�h�o�b�N�f�[�^�t�@�C�����쐬����
        /// </summary>
        /// 
        /// <param name="nstrCSVFileName">�t�B�[�h�o�b�N�f�[�^�t�@�C���̍쐬�Ɏg�p����CSV�t�@�C����</param>
        /// <param name="nstrSaveBaseFilePath">�o�͂���t�B�[�h�o�b�N�f�[�^�t�@�C�����̃x�[�X�ƂȂ�t�@�C����</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int PMSFeedbackRun(List<string> nstrCSVFileName, string nstrSaveBaseFilePath)
        {
            const string cstr_command = "/PMSFeedbackRun";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
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

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region MakeRecipeUsingFileList
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �����@�t�B�[�h�o�b�N�f�[�^�t�@�C�����쐬����
        /// </summary>
        /// 
        /// <param name="nstrRecipeFileName">���V�s���[�J�[�p���V�s�t�@�C����</param>
        /// <param name="nstrListFilePath">���V�s�t�@�C�������s���邽�߂ɕK�v�ȃt�@�C���̃��X�g���o�͂���e�L�X�g�t�@�C����</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int MakeRecipeUsingFileList(string nstrRecipeFileName, string nstrListFilePath)
        {
            const string cstr_command = "/MakeRecipeUsingFileList";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrRecipeFileName + ((char)0x01).ToString());
            list_send_data_list.Add(nstrListFilePath);

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region CreateUserGridCorrectFile
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���[�U�[�O���b�h�␳�t�@�C�����쐬����
        /// </summary>
        /// 
        /// <param name="nstrCheckGUI">GUI�ł̃`�F�b�N���s��(1)����(0)��</param>
        /// <param name="nstrMeasurementResultFilePaths">���茋��CSV�t�@�C���̃t���p�X(�����̏ꍇ����)</param>
        /// <param name="nstrStandardValueFilePath">��l�t�@�C���̃t���p�X</param>
        /// <param name="nstrUseDesignValue">��������݌v�l�𗘗p����(1)�����Ȃ�(0)��</param>
        /// <param name="nstrCorrectFilePath">�␳�t�@�C���̃t���p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int CreateUserGridCorrectFile(string nstrCheckGUI, string nstrMeasurementResultFilePaths,
                string nstrStandardValueFilePath, string nstrUseDesignValue, string nstrCorrectFilePath)
        {
            const string cstr_command = "/CreateUserGridCorrectFile";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrCheckGUI);
            list_send_data_list.Add(nstrMeasurementResultFilePaths);
            list_send_data_list.Add(nstrStandardValueFilePath);
            list_send_data_list.Add(nstrUseDesignValue);
            list_send_data_list.Add(nstrCorrectFilePath);

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AppendOrthogonalCorrection_RunCorrection
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���I����x�␳�l�����߂鑪����s��
        /// </summary>
        /// 
        /// <param name="nstrOutputFilePath">�������ʂ��o�͂���t�@�C���̃t���p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int AppendOrthogonalCorrection_RunCorrection(string nstrOutputFilePath)
        {
            const string cstr_command = "/AppendOrthogonalCorrection_RunCorrection";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrOutputFilePath);

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #region AppendOrthogonalCorrection_RunConfirmation
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���I����x�␳�l�̑Ó������m�F���鑪����s��
        /// </summary>
        /// 
        /// <param name="nstrOutputFilePath">�������ʂ��o�͂���t�@�C���̃t���p�X</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ---------------------------------------------------------------------------------------- 
        public int AppendOrthogonalCorrection_RunConfirmation(string nstrOutputFilePath)
        {
            const string cstr_command = "/AppendOrthogonalCorrection_RunConfirmation";
            List<string> list_send_data_list = new List<string>();

            //  ���M�R�}���h+���������X�g�ɏ��ԂɊi�[
            list_send_data_list.Add(cstr_command);
            list_send_data_list.Add(nstrOutputFilePath);

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

        #endregion

        #region MakeSimpleConnectedImage
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �P���ȘA���摜���쐬����B
        /// </summary>
        /// 
        /// <param name="nintXDirectionImageNum">X�����̉摜�t�@�C����</param>
        /// <param name="nintYDirectionImageNum">Y�����̉摜�t�@�C����</param>
        /// <param name="nstrInputImageFileNameArray">���͉摜�t�@�C���t���p�X���̔z��</param>
        /// <param name="nstrOutputImageFileName">�o�͉摜�t�@�C���t���p�X��</param>
        /// <param name="nbMakeJPG">�o�͉摜�t�@�C����JPEG�ł���(true)����(false)��</param>
        /// <param name="nintJPGQuality">�o�͉摜�t�@�C����JPEG�ł���ꍇ�̕i���p�����[�^�[(1�ȏ�100�ȉ��Œʏ�80)</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------      
        public int MakeSimpleConnectedImage( int nintXDirectionImageNum, int nintYDirectionImageNum, List<string> nstrInputImageFileNameArray, string nstrOutputImageFileName, bool nbMakeJPG, int nintJPGQuality )
        {
            const string cstr_command = "/MakeSimpleConnectedImage";
            List<string> list_send_data_list = new List<string>();
            string str_sepa_0x01 = ( ( char )0x01 ).ToString();
            string str_wrk;

            //  ���M�R�}���h+���������X�g�Ɋi�[����B
            list_send_data_list.Add((cstr_command+str_sepa_0x01));
            //  �@X�����摜�t�@�C����
            str_wrk = nintXDirectionImageNum.ToString();
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  �AY�����摜�t�@�C����
            str_wrk = nintYDirectionImageNum.ToString();
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  �B���͉摜�t�@�C���t���p�X���̃��X�g
            foreach (string str in nstrInputImageFileNameArray)
            {
                str_wrk = str;
                str_wrk += str_sepa_0x01;
                list_send_data_list.Add(str_wrk);
            }
            //  �C�o�͉摜�t�@�C���t���p�X��
            str_wrk = nstrOutputImageFileName;
            str_wrk += str_sepa_0x01;
            list_send_data_list.Add(str_wrk);
            //  �D�o�͉摜��JPG�Ȃ��JPEG�i���p�����[�^�[
            if (nbMakeJPG != false)
            {
                str_wrk = nintJPGQuality.ToString();
                list_send_data_list.Add(str_wrk);
            }

            //  ���M
            return makeSendStringAndSendData(list_send_data_list, MethodBase.GetCurrentMethod().Name, false);
        }
        #endregion

		#region WM_USER���̃��b�Z�[�W���M�֌W

		#region WM_USER���M
		/// ----------------------------------------------------------------------------------------
        /// <summary>
        ///     WM_USER���M
        /// </summary>
        /// 
        /// <param name="niMessageNo">WM_USER�̃��b�Z�[�W�ԍ�</param>
        /// <param name="niWParam">WPARAM</param>
        /// <param name="niLParam">LPARAM</param>
        /// 
        /// <returns>
        /// 0:����      
        /// -1:�\�P�b�g�ʐM�I�ɑ��M���s
        /// -2:������I�[�o�[
        /// -3:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int sendWMUSER(int niMessageNo, int niWParam, int niLParam)
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  ������쐬 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + niMessageNo.ToString() + " " + niWParam.ToString() + " " + niLParam.ToString() + "\n";
                //  ���M
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

        #region ����𒆎~����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ����𒆎~����
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:����
        /// -1:�\�P�b�g�ʐM�I�ɑ��M���s
        /// -2:������I�[�o�[
        /// -3:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureStopByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  ������쐬 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "122 0 0\n";
                //  ���M
                i_ret = Send(str_send_string);


                //  �A�����ă��b�Z�[�W���M����Ƃ��܂���M����Ȃ��̂ő҂�
                System.Threading.Thread.Sleep(500);

                //  ���~����O�ɁA���肪���f����Ă���\��������̂ōĊJ���Ă���
                //  ���f����ĂȂ��Ă��ĊJ���b�Z�[�W���M���Ă����Ȃ�����
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

        #region ����𒆒f����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ����𒆒f����
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:����
        /// -1:�\�P�b�g�ʐM�I�ɑ��M���s
        /// -2:������I�[�o�[
        /// -3:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureSuspendByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  ������쐬 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "139 1 0\n";
                //  ���M
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

        #region ������ĊJ����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ������ĊJ����
        /// </summary>
        /// 
        /// <returns>		    
        /// 0:����
        /// -1:�\�P�b�g�ʐM�I�ɑ��M���s
        /// -2:������I�[�o�[
        /// -3:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int MeasureRestartByWMUSER()
        {
            int i_ret;
            string str_send_string;
            try
            {
                //  ������쐬 :WM_USER xxx yyy zzz\n
                str_send_string = strWM_USER + " " + "139 2 0\n";
                //  ���M
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

        #region ���M������̍쐬���s���A���M����B�����[�g�R�}���h����
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���M������̍쐬���s���A���M����B�����[�g�R�}���h����
        /// </summary>
        /// 
        /// <param name="nlistSendData">���M����R�}���h+�������X�g</param>
        /// <param name="nstrMethodName">���̊֐����Ăт������֐���</param>
        /// <param name="nbExeptionCommand">true:2�x����h�~�̗�O�R�}���h�@false:�ʏ�̃R�}���h</param>
        /// 
        /// <returns>
        ///    0:����
        ///   -1:�\�P�b�g�ʐM�I�ɑ��M���s
        ///   -2:������I�[�o�[
        ///   -3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)
        ///   -4:2�x����G���[
        ///   -10:����ȊO�̃G���[
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        private int makeSendStringAndSendData(List<string> nlistSendData, string nstrMethodName, bool nbExeptionCommand )
        {
            const string strTerminate = "\n";
            string str_send_string = "";
            int i_ret;

            //  �\�P�b�g���L���łȂ���΃G���[
            if (m_bActiveSocket == false)
            {
                return -3;
            }

            //  �܂��O��̃����[�g�R�}���h�̕ԓ����A���Ă��ĂȂ��̂ɍēx���M���悤�Ƃ����Ƃ��̓G���[
            //  ��O�R�}���h�łȂ����Ƃ�����
            if (m_bSentRemoteCommand == true && nbExeptionCommand == false)
            {
                return -4;
            }

            try
            {
                //  ���X�g���瑗�M��������쐬
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

                //  ���M
                i_ret = Send(str_send_string);

                //  ���M���������瑗�M�t���O�𗧂Ă�
                if( i_ret == 0 )
                {
                    //  2�x����h�~�̗�O�R�}���h�ł���
                    if (nbExeptionCommand == true)
                    {
                        m_SentExeptionCommand = true;
						//	�������R�}���h��������o���Ă���
						m_strLastSentExceptionCommand = nstrMethodName;
                    }
                    //  �ʏ�̃R�}���h�ł���
                    else
                    {
                        m_bSentRemoteCommand = true;
						//	�������R�}���h��������o���Ă���
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
