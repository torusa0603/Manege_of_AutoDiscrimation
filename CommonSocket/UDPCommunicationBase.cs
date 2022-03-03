using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

//  XML�h�L�������g�̌x���͖���
#pragma warning disable 1591

namespace SPCommonSocket
{
    public class UDPCommunicationBase
    {
        #region �C�x���g
        
        //  �f�[�^��M�C�x���g(string)
        public Action<string> evReceiveUDPStringData;
        //  �f�[�^��M�C�x���g(�o�C�i��)
        public Action<byte[]> evReceiveUDPBinaryData;

        #endregion

        #region �����o�[�ϐ�

        public const int RECEIVE = 0;           //  ��M��pUDP
        public const int SEND = 1;              //  ���M��pUDP
        public const int SEND_RECEIVE = 2;      //  ����MUDP

        public const int DATA_TYPE_STRING = 0;  //  ����M�f�[�^(������)
        public const int DATA_TYPE_BINARY = 1;  //  ����M�f�[�^(�o�C�i��)

        private int m_iCommunicationType = -1;  //  �ʐM�^�C�v(��Monly�A���Monly�A����M)
        private int m_iDataType = -1;           //  ����M�f�[�^�^�C�v(������A�o�C�i��)

        public UdpClient m_UdpReceive;          //  ��MUDP�N���C�A���g
        public UdpClient m_UdpSend;             //  ���MUDP�N���C�A���g

        private IPEndPoint m_SendEndPoint;      //  ���M�G���h�|�C���g
        private IPEndPoint m_ReceiveEndPoint;   //  ��M�G���h�|�C���g

        private object m_lock = new object();   //  �r�������plock�I�u�W�F�N�g

        Form m_formParent;                      //  �A�v���t�H�[��

        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="niCommunicationType">�ʐM�^�C�v</param>
        /// <param name="niDataType">����M�f�[�^�^�C�v</param>
        /// <param name="nformParent">�e�t�H�[��</param>
        public UDPCommunicationBase(int niCommunicationType, int niDataType, Form nformParent)
        {
            //  �ʐM�^�C�v��ݒ肷��
            m_iCommunicationType = niCommunicationType;
            //  ����M�f�[�^�^�C�v��ݒ肷��
            m_iDataType = niDataType;
            //  �G���h�|�C���g�����������Ă���
            m_SendEndPoint = m_ReceiveEndPoint = null;
            //  UDP�I�u�W�F�N�g�����������Ă���
            m_UdpReceive = m_UdpSend = null;
            //  ����dll���Ă�ł���t�H�[�����o���Ă���
            m_formParent = nformParent;
        }

        /// <summary>
        /// UDP�I�[�v��(��M��p�^�C�v)
        /// </summary>
        /// <param name="niReceivePort">��M�p�|�[�g�ԍ�</param>
        /// <returns></returns>
        public int openUDP(int niReceivePort)
        {
            //  ��M��p�^�C�v�ŃI�u�W�F�N�g������ĂȂ���΃G���[
            if (m_iCommunicationType != RECEIVE)
            {
                return -1;
            }

            try
            {
                //  ��M�G���h�|�C���g�쐬
                m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, niReceivePort);
                //  ��MUDP�I�u�W�F�N�g�Ƀo�C���h������
                m_UdpReceive = new UdpClient(m_ReceiveEndPoint);
                //  ��M�C�x���g�n���h����ݒ肷��
                m_UdpReceive.BeginReceive(receiveHandler, this);
            }
            catch (System.Exception)
            {
                //  �I�[�v���G���[
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDP�I�[�v��(���M��p�^�C�v)
        /// </summary>
        /// <param name="nstrIPAddress">���M���IP�A�h���X</param>
        /// <param name="niSendPort">���M��̃|�[�g�ԍ�</param>
        /// <returns></returns>
        public int openUDP(string nstrIPAddress,int niSendPort)
        {
            IPAddress ip_address;

            //  ���M��p�^�C�v�ŃI�u�W�F�N�g������ĂȂ���΃G���[
            if (m_iCommunicationType != SEND)
            {
                return -1;
            }

            //  IP�A�h���X���Ԉ���Ă�����G���[
            if (IPAddress.TryParse(nstrIPAddress, out ip_address) == false)
            {
                return -2;
            }

            try
            {
                //  ���M�G���h�|�C���g�쐬
                m_SendEndPoint = new IPEndPoint(ip_address, niSendPort);
                //  ���MUDP�I�u�W�F�N�g�쐬
                m_UdpSend = new UdpClient();
                //  ���M���ݒ�
                m_UdpSend.Connect(m_SendEndPoint);
            }
            catch (System.Exception)
            {
                //  �I�[�v���G���[
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDP�I�[�v��(����M�^�C�v)
        /// </summary>
        /// <param name="nstrIPAddress">���M���IP�A�h���X</param>
        /// <param name="niSendPort">���M��̃|�[�g�ԍ�</param>
        /// <param name="niReceivePort">��M�p�|�[�g�ԍ�</param>
        /// <returns></returns>
        public int openUDP(string nstrIPAddress, int niSendPort, int niReceivePort)
        {
            IPAddress ip_address;

            //  ����M�^�C�v�ŃI�u�W�F�N�g������ĂȂ���΃G���[
            if (m_iCommunicationType != SEND_RECEIVE)
            {
                return -1;
            }
            //  IP�A�h���X���Ԉ���Ă�����G���[
            if (IPAddress.TryParse(nstrIPAddress, out ip_address) == false)
            {
                return -2;
            }

            try
            {
                //  ��M�G���h�|�C���g�쐬
                m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, niReceivePort);
                //  ��MUDP�I�u�W�F�N�g�Ƀo�C���h������
                m_UdpReceive = new UdpClient(m_ReceiveEndPoint);
                //  ��M�C�x���g�n���h����ݒ肷��
                m_UdpReceive.BeginReceive(receiveHandler, this);

                //  ���M�G���h�|�C���g�쐬
                m_SendEndPoint = new IPEndPoint(ip_address, niSendPort);
                //  ���MUDP�I�u�W�F�N�g�쐬
                m_UdpSend = new UdpClient();
                //  ���M���ݒ�
                m_UdpSend.Connect(m_SendEndPoint);
            }
            catch (System.Exception)
            {
                //  �I�[�v���G���[
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// UDP�ʐM�����
        /// </summary>
        /// <returns></returns>
        public int closeUDP()
        {
            //  UDP�I�u�W�F�N�g����������ƂɃC�x���g�n���h���ɔ�Ԃ��Ƃ���̂�lock���Ă���
            lock(m_lock)
            {
                //UPD�N���[�Y
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
        /// ���M(������)
        /// </summary>
        /// <param name="nstrSend">���M�f�[�^(������)</param>
        /// <returns></returns>
        public int sendData(string nstrSend)
        {
            //  ���M�I�u�W�F�N�g���Ȃ���΃G���[
            if (m_UdpSend == null)
            {
                return -1;
            }
            //  ����M�f�[�^�^�C�v��������łȂ���΃G���[
            if(m_iDataType != DATA_TYPE_STRING)
            {
                return -2;
            }

            //��������o�C�g��ɕϊ�
            byte[] bt_send_data = System.Text.Encoding.UTF8.GetBytes(nstrSend);
            //���M
            m_UdpSend.Send(bt_send_data, bt_send_data.Length);

            return 0;
        }

        /// <summary>
        /// ���M(�o�C�i��)
        /// </summary>
        /// <param name="nbtSend">���M�f�[�^(�o�C�i��)</param>
        /// <returns></returns>
        public int sendData(byte[] nbtSend)
        {
            //  ���M�I�u�W�F�N�g���Ȃ���΃G���[
            if (m_UdpSend == null)
            {
                return -1;
            }
            //  ����M�f�[�^�^�C�v���o�C�i���łȂ���΃G���[
            if (m_iDataType != DATA_TYPE_BINARY)
            {
                return -2;
            }

            //���M
            m_UdpSend.Send(nbtSend, nbtSend.Length);

            return 0;
        }


        /// <summary>
        /// ��M(������)
        /// </summary>
        /// <param name="nstrReceive">��M�f�[�^(������)</param>
        /// <returns></returns>
        public void ReceiveStringData(string nstrReceive)
        {
            //  ��M�C�x���g���s
            evReceiveUDPStringData?.Invoke(nstrReceive);
        }

        /// <summary>
        /// ��M(�o�C�i��)
        /// </summary>
        /// <param name="nbtReceive">��M�f�[�^(�o�C�i��)</param>
        public void ReceiveBinaryData(byte[] nbtReceive)
        {
            //  ��M�C�x���g���s
            evReceiveUDPBinaryData?.Invoke(nbtReceive);
        }


        /// <summary>
        /// ��M�C�x���g�n���h��
        /// </summary>
        /// <param name="ar"></param>
        private void receiveHandler(IAsyncResult ar)
        {
            IPEndPoint end_point = null;
            byte[] bt_receive;
            string str_receive;

            lock(m_lock)
            {
                //  �������g�̃N���X�I�u�W�F�N�g���擾
                UDPCommunicationBase myself_object = (UDPCommunicationBase)ar.AsyncState;

                //  ��M�I�u�W�F�N�g����Ȃ牽�����Ȃ�
                if (myself_object.m_UdpReceive == null)
                {
                    return;
                }

                try
                {
                    //  �f�[�^��M
                    bt_receive = myself_object.m_UdpReceive.EndReceive(ar, ref end_point);
                }
                //  ��M���s
                catch (System.Net.Sockets.SocketException)
                {
                    //  �ēx��M�󂯕t���J�n
                    myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);
                    return;
                }
                catch (System.Exception)
                {
                    //  �ēx��M�󂯕t���J�n
                    myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);
                    return;
                }

                //  �ēx��M�󂯕t���J�n
                myself_object.m_UdpReceive.BeginReceive(receiveHandler, myself_object);

                //  ����M�f�[�^�^�C�v��������
                if(m_iDataType == DATA_TYPE_STRING)
                {
                    //��M�f�[�^�𕶎��ϊ�
                    str_receive = System.Text.Encoding.UTF8.GetString(bt_receive);
                    //  ��M�������Ƃ��A�v���ɒʒm����
                    m_formParent.Invoke(new Action(() => ReceiveStringData(str_receive)));
                }
                //  ����M�f�[�^�^�C�v���o�C�i��
                else
                {
                    //  ��M�������Ƃ��A�v���ɒʒm����
                    m_formParent.Invoke(new Action(() => ReceiveBinaryData(bt_receive)));
                }
 
            }
        }

    }
}
