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

//  XML�h�L�������g�̌x���͖���
#pragma warning disable 1591

namespace SPCommonSocket
{
    //  �C�x���g�n���h���p�f���Q�[�g
    public delegate void EventHandlerString(string str);
    public delegate void EventHandlerVoid();

    //  �f�[�^���M�p�̃f���Q�[�g
    delegate void ReceiveDataDelegate(string text);
    //  �\�P�b�g�N���[�Y�p�̃f���Q�[�g
    delegate int SocketCloseDelegate();

    public class CSocketCommunicationBase
    {
        #region �C�x���g��`
        //  �f�[�^��M�C�x���g
        public event EventHandlerString evReceiveData;
        public event EventHandlerString evReceiveDataForSCAM;
        //  �\�P�b�g�N���[�Y�C�x���g
        public event EventHandlerVoid evSocketClose;
        //  �T�[�o�[�ɃN���C�A���g����ڑ����������甭������C�x���g
        public event EventHandlerVoid evAcceptClient;

        #endregion

        #region �����o�[�ϐ�
        private TcpListener m_TcpServerListener = null;     //  TCP�T�[�o�[���X�i�[
        private TcpClient m_TCPServer = null;             //  �T�[�o�[
        private Thread m_ThreadServer = null;               //  �T�[�o�[�̃X���b�h
        private int m_iServerPortNo;                        //  �T�[�o�[���g�p����|�[�g�ԍ�

        protected TcpClient m_TcpClient = null;             //  �N���C�A���g
        protected Thread m_ThreadClient = null;             //  �N���C�A���g�̃X���b�h

        protected int m_iSokType;                           //  �\�P�b�g�^�C�v
        protected bool m_bActiveSocket;                     //  �\�P�b�g���L����Ԃ��ۂ�

        public const int CLIENT = 0;                        //  �N���C�A���g
        public const int SERVER = 1;                        //  �T�[�o�[
        public const int SCAM_CLIENT = 2;                   //  SCAM�ʐM�̂��߂̃N���C�A���g
        private const int MAX_DATA_SIZE = 2048;             //  �ő呗�M�f�[�^�o�C�g

        private bool m_OutputDataLog;                       //  �ʐM���O���o�͂��邩�ۂ�
        private int m_iInstanceIndex;                       //  �I�u�W�F�N�g�C���f�b�N�X


        public Form m_frmParent;    //  ����dll���Ă�ł���t�H�[��

        static private int m_siTotalInstanceIndex = 0;               //  ���܂ō쐬�����I�u�W�F�N�g��
        static object lockObject = new object();                    //  �C���X�^���X�Ԃ̔r������Ɏg�p

        private bool m_bAddPeriodAfterIPAddress = false;    //  IP�A�h���X�̖����Ƀs���I�h�u.�v�����邩�ۂ�

        #endregion

        #region �����R�[�h

        //  �{���C�u�������Ή����镶���R�[�h
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
        //  �w�肷�镶���R�[�h��
        private string m_strEncoding = "shift-jis";

        #endregion

        #region �R���X�g���N�^
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// 
        /// <param name="niSokType">
        /// 
        /// <para>�쐬���悤�Ƃ��Ă���\�P�b�g�^�C�v</para>
        /// <para>�N���C�A���g�FCSocketCommunicationBase.CLIENT</para>
        /// <para>�T�[�o�[:CSocketCommunicationBase.SERVER</para>
        /// 
        /// </param>
        /// <param name="nformParent">�e�t�H�[��</param>
        /// 
        /// ----------------------------------------------------------------------------------------
        public CSocketCommunicationBase( int niSokType, Form nformParent )
        {
            //  �\�P�b�g�^�C�v��ݒ�
            m_iSokType = niSokType;
            m_bActiveSocket = false;
            m_OutputDataLog = false;
            m_frmParent = nformParent;
            m_iInstanceIndex = m_siTotalInstanceIndex;
            m_siTotalInstanceIndex++;
        }
        #endregion

        #region �R���X�g���N�^2
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �R���X�g���N�^2
        /// </summary>
        /// 
        /// <param name="niSokType">
        /// 
        /// <para>�쐬���悤�Ƃ��Ă���\�P�b�g�^�C�v</para>
        /// <para>�N���C�A���g�FCSocketCommunicationBase.CLIENT</para>
        /// <para>�T�[�o�[:CSocketCommunicationBase.SERVER</para>
        /// 
        /// </param>
        /// 
        /// ----------------------------------------------------------------------------------------
        public CSocketCommunicationBase(int niSokType)
        {
            //  �\�P�b�g�^�C�v��ݒ�
            m_iSokType = niSokType;
            m_bActiveSocket = false;
            m_OutputDataLog = false;
            m_frmParent = null;
            m_iInstanceIndex = m_siTotalInstanceIndex;
            m_siTotalInstanceIndex++;
        }
        #endregion

        #region �\�P�b�g�I�[�v��(�N���C�A���g) IP�A�h���X�w��
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        ///  �\�P�b�g�I�[�v��(�N���C�A���g)
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
        virtual public int OpenSocket(string nstrIPHost, int niPort )
        {
            //  �T�[�o�[�Ƃ��ċN�������̂ɃN���C�A���g�̃\�P�b�g���I�[�v�����悤�Ƃ�����G���[
            if (m_iSokType == SERVER)
            {
                return -1;
            }
            //  ���Ƀ\�P�b�g���I�[�v�����Ă���΃G���[
            else if (m_bActiveSocket == true)
            {
                return -2;
            }
            try
            {
                //  IP�A�h���X�w��̏ꍇ
                if (IsIPAddress(nstrIPHost) == true)
                {
                    //  �s���I�h��t������ꍇ
                    if( m_bAddPeriodAfterIPAddress == true )
                    {
                        //  �����񖖔��Ɂu.�v���Ȃ���Εt����
                        if (nstrIPHost.EndsWith(".") == false)
                        {
                            nstrIPHost = nstrIPHost + ".";
                        }
                    }
                }

                //�N���C�A���g�̃\�P�b�g��p��
                m_TcpClient = new TcpClient(nstrIPHost, niPort);

                //�T�[�o����̃f�[�^����M���郋�[�v���X���b�h�ŏ���
                m_ThreadClient = new Thread(new ThreadStart(this.ClientListenThread));
                //  �\�P�b�g���L���ɂȂ���
                m_bActiveSocket = true;
                //  ��M�X���b�h�J�n
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

        #region �\�P�b�g�I�[�v��(�N���C�A���g and �T�[�o�[)
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// <para>�\�P�b�g�I�[�v��(�N���C�A���g and �T�[�o�[)</para>
        /// <para>�N���C�A���g�̏ꍇ�́A�z�X�g���͎������g��PC�̃z�X�g�����g�p����</para>
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
        virtual public int OpenSocket(int niPort)
        {
            //  ���Ƀ\�P�b�g���I�[�v�����Ă���΃G���[
            if (m_bActiveSocket == true)
            {
                return -2;
            }


            //  �T�[�o�[�Ƃ��ăI�[�v��
            if (m_iSokType == SERVER)
            {
                try
                {
                    //�N���C�A���g����̐ڑ���ҋ@����T�[�o�N���X�ݒ�
                    if (m_TcpServerListener == null)
                    {
                        m_TcpServerListener = new TcpListener(IPAddress.Any, niPort);
                    }
                    m_iServerPortNo = niPort;
                    //  �\�P�b�g���L���ɂȂ���
                    m_bActiveSocket = true;
                    //�T�[�o�̊J�n�@�N���C�A���g����ڑ������܂őҋ@
                    m_TcpServerListener.Start();
                    //  �X���b�h�쐬
                    m_ThreadServer = new Thread(new ThreadStart(this.ServerListenThread));
                    //  �X���b�h�J�n
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
            //  �N���C�A���g�Ƃ��ăI�[�v��
            else
            {
                try
                {
                    //�N���C�A���g�̃\�P�b�g��p��
                    m_TcpClient = new TcpClient(System.Net.Dns.GetHostName(), niPort);

                    //�T�[�o����̃f�[�^����M���郋�[�v���X���b�h�ŏ���
                    m_ThreadClient = new Thread(new ThreadStart(this.ClientListenThread));
                    //  �\�P�b�g���L���ɂȂ���
                    m_bActiveSocket = true;
                    //  ��M�X���b�h�J�n
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

        #region �T�[�o�[�p�X���b�h
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �T�[�o�[�p�X���b�h
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        protected void ServerListenThread()
        {
            //  �\�P�b�g���L���ɂȂ��ĂȂꂯ�ΏI��
            if (m_bActiveSocket == false)
            {
                return;
            }

            NetworkStream stream;

            try
            {
                //�N���C�A���g�̗v������������A�ڑ����m������
                //�N���C�A���g�̗v�����L��܂ł����őҋ@���� 
                m_TCPServer = m_TcpServerListener.AcceptTcpClient();
                //�N���C�A���g�Ƃ̊Ԃ̒ʐM�Ɏg�p����X�g���[�����擾
                stream = m_TCPServer.GetStream();
            }
            catch (Exception ex)
            {
                string str_err;
                str_err = MethodBase.GetCurrentMethod().Name + "," + ex.Message;
                OutputErrorLog(str_err);
                //  �\�P�b�g�N���[�Y
                CloseSocket();

                return;
            }


            //  �N���C�A���g�Ƃ̐ڑ������ꂽ�C�x���g�𔭐�
            AcceptClientEvent();

            Byte[] bytes = new Byte[4096];

            while (true)
            {
                try
                {
                    int intCount = stream.Read(bytes, 0, bytes.Length);
                    if (intCount != 0)
                    {
                        //��M���������؂�o��
                        Byte[] getByte = new byte[intCount];
                        for (int i = 0; i < intCount; i++)
                            getByte[i] = bytes[i];

                        string str;
                        //�o�C�g�z��𕶎���ɕϊ�
                        str = System.Text.Encoding.GetEncoding(m_strEncoding).GetString(getByte);
                        //��M�C�x���g����
                        if (m_frmParent != null)
                        {
                            m_frmParent.BeginInvoke(new ReceiveDataDelegate(ReceiveData), new object[] { str });
                        }
                        else
                        {
                            ReceiveData(str);
                        }
                    }
                    //  �N���C�A���g���ڑ���؂���
                    else
                    {
                        //���[�v�𔲂���
                        stream.Close();
                        //  �T�[�o�[���ĊJ����B
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
                    //  �X���b�h�������ؒf����Ƃ��̃G���[����������
                    //  �����������邽�߂̃G���[����
                    //  �ؒf���ꂽ�̂Ń��[�v�𔲂���
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
                    //  �Ȃ�炩�̌����ŁA�X���b�h���I�������ɃA�v���P�[�V�������I�����Ă��܂����ꍇ
                    //  �e�t�H�[���͊��ɏI�����Ă���̂�BeginInvoke���g�p�����ɒ��ڃX���b�h/�\�P�b�g��
                    //  �I������
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


        #region �N���C�A���g�f�[�^��M�X���b�h
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �N���C�A���g�f�[�^��M�X���b�h
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        protected void ClientListenThread()
        {
            //  �\�P�b�g���L���ɂȂ��ĂȂꂯ�ΏI��
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
                        //��M���������؂�o��
                        Byte[] getByte = new byte[intCount];
                        for (int i = 0; i < intCount; i++)
                            getByte[i] = bytes[i];

                        string str;
                        //�o�C�g�z��𕶎���ɕϊ�
                        str = System.Text.Encoding.GetEncoding(m_strEncoding).GetString(getByte);
                        //��M�C�x���g����
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
                        //���[�v�𔲂���
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
                    //  �X���b�h�������ؒf����Ƃ��̃G���[����������
                    //  �����������邽�߂̃G���[����
                    //  �ؒf���ꂽ�̂Ń��[�v�𔲂���
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
                    //  �Ȃ�炩�̌����ŁA�X���b�h���I�������ɃA�v���P�[�V�������I�����Ă��܂����ꍇ
                    //  �e�t�H�[���͊��ɏI�����Ă���̂�BeginInvoke���g�p�����ɒ��ڃX���b�h/�\�P�b�g��
                    //  �I������
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

        #region �\�P�b�g�I��
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �\�P�b�g�I��
        /// </summary>
        /// 
        ///<returns>
        ///
        ///<para>0:����</para>
        ///<para>-1:�\�P�b�g���L���łȂ�</para>
        ///<para>-2:�\�P�b�g�N���[�Y�G���[</para>
        ///
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int CloseSocket()
        {
            //  �\�P�b�g���L���ɂȂ��ĂȂꂯ�ΏI��
            if (m_bActiveSocket == false)
            {
                return -1;
            }

            try
            {
                //  �T�[�o�[�ł���
                if(m_iSokType == SERVER)
                {
                    m_bActiveSocket = false;

                    //�T�[�o�[�̃C���X�^���X���L���āA�ڑ�����Ă�����
                    if (m_TCPServer != null && m_TCPServer.Connected)
                    {
                        m_TCPServer.Close();
                        m_TCPServer = null;
                    }

                    CloseSocketEvent();

                    //�X���b�h�͕K���I�������邱��
                    if (m_ThreadServer != null)
                    {
                        m_TcpServerListener.Stop();
                        m_TcpServerListener = null;
                        m_ThreadServer.Abort();
                        m_ThreadServer = null;
                    }

                }
                //  �N���C�A���g�ł���
                else
                {
                    m_bActiveSocket = false;

                    //�N���C�A���g�̃C���X�^���X���L���āA�ڑ�����Ă�����
                    if (m_TcpClient != null && m_TcpClient.Connected)
                    {
                        m_TcpClient.Close();
                        m_TcpClient = null;
                    }

                    CloseSocketEvent();

                    //�X���b�h�͕K���I�������邱��
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

        #region �\�P�b�g���L�����ǂ������ׂ�
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �\�P�b�g���L�����ǂ������ׂ�
        /// </summary>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        public bool getSocketActive()
        {
            return m_bActiveSocket;
        }
        #endregion

        #region �f�[�^���M
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �f�[�^���M
        /// </summary>
        /// 
        /// <param name="nstrSendData">���M������</param>
        /// 
        /// <returns>
        /// 
        /// <para>0:����</para>
        /// <para>-1:�f�[�^���M�G���[</para>
        /// <para>-2:�f�[�^���I�[�o�[</para>
        /// <para>-3:�\�P�b�g���I�[�v�����Ă��Ȃ�(�L���łȂ�)</para>
        /// </returns>
        /// ----------------------------------------------------------------------------------------
        public int Send( string nstrSendData )
        {

            //sift-jis�ɕϊ����đ���
            Byte[] data = Encoding.GetEncoding(m_strEncoding).GetBytes(nstrSendData);

            //  �\�P�b�g���L���łȂ���΃G���[
            if (m_bActiveSocket == false)
            {
                return -3;
            }

            //  �o�C�g�����I�[�o�[���Ă���΃G���[
            if (data.Count() > MAX_DATA_SIZE)
            {
                return -2;
            }

            //���Mstream���쐬
            NetworkStream stream = null;

            //  �T�[�o�[�ł���
            if(m_iSokType == SERVER)
            {
                stream = m_TCPServer.GetStream();
            }
            //  �N���C�A���g�ł���
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
                //Stream���g���đ��M
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

        #region �f�[�^��M�C�x���g����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �f�[�^��M�C�x���g�𔭐�������
        /// </summary>
        /// 
        /// <param name="strReceiveData">��M�f�[�^������</param>
        /// ----------------------------------------------------------------------------------------
        public void ReceiveData(string strReceiveData)
        {
            try
            {
                if (m_OutputDataLog == true)
                {
                    OutputDataLog(strReceiveData, 0);
                }

                //  SCAM�ȊO�̒ʐM
                if (m_iSokType != SCAM_CLIENT)
                {
                    if (evReceiveData != null)    //  �o�^���ĂȂ��\������̂ł���K�{
                    {
                        //  �C�x���g���M
                        evReceiveData(strReceiveData);
                    }
                }
                //  SCAM�Ƃ̃����[�g�ʐM
                else
                {
                    if (evReceiveDataForSCAM != null)    //  �o�^���ĂȂ��\������̂ł���K�{
                    {
                        //  �C�x���g���M
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

        #region �\�P�b�g�N���[�Y�C�x���g����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �\�P�b�g�N���[�Y�C�x���g����
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void CloseSocketEvent()
        {
            try
            {
                if (evSocketClose != null)    //  �o�^���ĂȂ��\������̂ł���K�{
                {
                    //  �C�x���g���M
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

        #region �T�[�o�[�ɃN���C�A���g����ڑ����������C�x���g����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �T�[�o�[�ɃN���C�A���g����ڑ����������C�x���g����evAcceptClient
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public void AcceptClientEvent()
        {
            try
            {
                if (evAcceptClient != null)    //  �o�^���ĂȂ��\������̂ł���K�{
                {
                    //  �C�x���g���M
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

        #region ���͂��ꂽ�̂�IP�A�h���X�Ȃ̂��z�X�g���Ȃ̂��𔻒f����
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// ���͂��ꂽ�̂�IP�A�h���X�Ȃ̂��z�X�g���Ȃ̂��𔻒f����
        /// </summary>
        /// <param name="nstrIPHost">���͂��ꂽ������</param>
        /// <returns>true:IP�A�h���X  false:�z�X�g��</returns>
        /// ----------------------------------------------------------------------------------------
        private bool IsIPAddress(string nstrIPHost)
        {
            int i_loop;
            int i_wrk;
            //  IP�A�h���X���ǂ����̔��f�́A�u.�v�ŕ����񕪉����A4�̐����ɕ����ł��A
            //  ���̐�����0�`255�ł����IP�A�h���X�Ɣ��f����

            //  �f�[�^����(���p�X�y�[�X)
            string[] stArrayData = nstrIPHost.Split('.');
            //  �Ō�̕���������""�Ȃ當����Ō�̕����́u.�v�ł������Ƃ�������
            if (stArrayData[stArrayData.Count() - 1] == "")
            {
                //  ���̏ꍇ�A����܂ł̕�����������g�p
                if (stArrayData.Count() == 5)
                {
                    for (i_loop = 0; i_loop < 4; i_loop++)
                    {
                        //  0�`255�̒l�łȂ���΃z�X�g��
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
                //  ��������5�łȂ���΃z�X�g��
                else
                {
                    return false;
                }
            }
            else
            {
                //  ���̏ꍇ�A����܂ł̕�����������g�p
                if (stArrayData.Count() == 4)
                {
                    for (i_loop = 0; i_loop < 4; i_loop++)
                    {
                        //  0�`255�̒l�łȂ���΃z�X�g��
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
                //  ��������5�łȂ���΃z�X�g��
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region �G���[���O���o�͂���
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �G���[���O���o�͂���
        /// </summary>
        /// <param name="nstrLogMessage">�o�͂��郍�O������</param>
        /// ----------------------------------------------------------------------------------------
        protected void OutputErrorLog(string nstrLogMessage)
        {
            const string str_directory_path = "log";
            string str_file_path;

            try
            {
                //  �C���X�^���X�Ԃ̔r������
                lock (lockObject)
                {
                    //  �t�@�C���p�X����
                    str_file_path = str_directory_path + "\\" + "SPCommonSocket_ERROR_" + DateTime.Today.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".log";

                    //  log�o�̓f�B���N�g���̑��݃`�F�b�N�B�Ȃ���΍쐬
                    if (Directory.Exists(str_directory_path) == false)
                    {
                        Directory.CreateDirectory(str_directory_path);
                    }

                    //  �t�@�C���I�[�v��
                    Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                    StreamWriter writer = new StreamWriter(str_file_path, true, sjisEnc);

                    //  ���O�ɓ��t��t��
                    nstrLogMessage = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "," + m_iInstanceIndex.ToString() + "," + nstrLogMessage;
                    //  ��������
                    writer.WriteLine(nstrLogMessage);
                    //  �N���[�Y
                    writer.Close();
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        #region �ʐM���O���o�͂���
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �ʐM���O���o�͂���
        /// </summary>
        /// 
        /// <param name="nstrLogMessage">��M/���M������</param>
        /// <param name="niSendRecv">0:��M�@1:���M</param>
        /// ----------------------------------------------------------------------------------------        
        protected void OutputDataLog(string nstrLogMessage, int niSendRecv )
        {
            const string str_directory_path = "log";
            string str_file_path;
            string str_send_recv;

            try
            {
                //  �C���X�^���X�Ԃ̔r������
                lock (lockObject)
                {
                    //  �t�@�C���p�X����
                    str_file_path = str_directory_path + "\\" + "SPCommonSocket_DATA_" + DateTime.Today.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".log";

                    //  log�o�̓f�B���N�g���̑��݃`�F�b�N�B�Ȃ���΍쐬
                    if (Directory.Exists(str_directory_path) == false)
                    {
                        Directory.CreateDirectory(str_directory_path);
                    }

                    //  �t�@�C���I�[�v��
                    Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                    StreamWriter writer = new StreamWriter(str_file_path, true, sjisEnc);

                    //  ��M
                    if (niSendRecv == 0)
                    {
                        str_send_recv = "Recv>";
                    }
                    //  ���M
                    else
                    {
                        str_send_recv = "Send>";
                    }

                    //  ���O�ɓ��t��t��
                    nstrLogMessage = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "," + m_iInstanceIndex.ToString() + "," + str_send_recv + "," + nstrLogMessage;

                    //  ��������
                    writer.WriteLine(nstrLogMessage);
                    //  �N���[�Y
                    writer.Close();
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        #endregion

        #region �ʐM���O���o�͂��邩�ۂ���ݒ肷��
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// �ʐM���O���o�͂��邩�ۂ���ݒ肷��
        /// </summary>
        /// 
        /// <param name="nbEnableOutputLog">true: �ʐM���O���o�͂���@�@false: �ʐM���O���o�͂��Ȃ�</param>
        /// ---------------------------------------------------------------------------------------- 
        public void SetOutputDataLogMode(bool nbEnableOutputLog)
        {
            m_OutputDataLog = nbEnableOutputLog;
        }
        #endregion

        #region IP�A�h���X�̖����Ƀs���I�h�u.�v������
        /// <summary>
        /// IP�A�h���X�̖����Ƀs���I�h�u.�v������(���Ȃ��ƃ\�P�b�g�I�[�v���o���Ȃ����ۂ����������߂̑Ή�)
        /// </summary>
        public void AddPeriodAfterIPAddress(bool nbAddPeriodAfterIPAddress)
        {
            m_bAddPeriodAfterIPAddress = nbAddPeriodAfterIPAddress;
        }
        #endregion

        #region �T�[�o�[�̍ĊJ������

        /// <summary>
        /// �T�[�o�[�̍ĊJ������
        /// </summary>
        /// <returns></returns>
        public int RestartServer()
        {
            //  ��U�\�P�b�g���N���[�Y����
            CloseSocket();
            //  �ăI�[�v������
            OpenSocket(m_iServerPortNo);

            return 0;
        }

        /// <summary>
        /// �����R�[�h��ݒ肷��
        /// </summary>
        /// <param name="nEncode"></param>
        public void SetEncoding(eEncode nEncode)
        {
            m_strEncoding = m_dictEncode[nEncode];
        }

        #endregion


    }
}
