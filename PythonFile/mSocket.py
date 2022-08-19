import socket
import threading
from enum import Enum
from abc import ABCMeta, abstractmethod, ABC
import mEvent
import mLogHandle

class Publisher(object):
    
    # Set event object in class declaration.
    evt = mEvent.Event('Socket')
    handle_connect = False
    log = mLogHandle.Log("Socket")
    
    def Action(self,earg):
        # Do some actions and fire event.
        self.evt(earg)
        

class Socket(threading.Thread):
    class Mode(Enum):
        tpServer = 0
        tpClient = 1

    __meMode = Mode.tpServer
    __bind_port=50000
    __buffersize = 16


    # コンストラクタ
    def __init__(self,eMode,Adress):
        self.pub = Publisher()
        self.__meMode = eMode
        if self.__meMode == self.Mode.tpServer:
            self.__Socket = SocketAsServer(Adress,self.__bind_port,self.__buffersize,self.pub)
        else:
            self.__Socket = SocketAsClient(Adress,self.__bind_port,self.__buffersize,self.pub)
        super(Socket, self).__init__()
    
    # 通信開始メソッド
    def run(self):
        self.__Socket.Connect()

    def Send(self, Command):
        self.__Socket.Send(Command)

class AbstractSocket(metaclass=ABCMeta):
    @abstractmethod
    def __init__(self,Address,BindPort,BufferSize,Publisher):
        pass
    
    @abstractmethod
    def Connect(self):
        self.__opend = False
    
    @abstractmethod
    def Send(self, n_strCommand):
        pass

class SocketAsServer(AbstractSocket):
    def __init__(self,Address,BindPort,BufferSize,nPublisher):
        self.__adress = Address
        self.__bind_port= BindPort
        self.__buffersize=BufferSize
        self.__publisher=nPublisher

    def Connect(self):
        super().Connect
        #ソケットを作成
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as self.__mServer:
        #IPアドレスとポート設定
            if self.__adress == "host":
                host= socket.gethostname()
                self.__adress = socket.gethostbyname(host)
            self.__mServer.bind((self.__adress, self.__bind_port))
            self.__publisher.log.Write(f"HOST_{self.__adress}")
            #self.__mServer.connect((socket.gethostbyname(host), self.__bind_port))
            self.__mServer.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.__mServer.listen(1)
            # 通信を開始する
            while True:
                # 接続完了
                self.__clientsocket, address = self.__mServer.accept()
                print(f"Connecting_{address}")
                # 通信状態フラグを上げる
                self.__opend = True
                while True:
                    try:
                        # 受信コマンドを待つ
                        rec = self.__clientsocket.recv(self.__buffersize)
                        recmsg = rec.decode('utf-8')
                        print(recmsg)
                        # 無事受信したと返す
                        send_msg = f"/OK\n"
                        self.Send(send_msg)
                        if self.__publisher.handle_connect == True:
                            self.__publisher.Action(recmsg)
                    except:
                        import traceback
                        traceback.print_exc()
                        break
                    # endコマンドが来たら終了し、接続待ち状態になる
                    if recmsg == "end":
                        break
                # 通信状態フラグを下げる
                self.__opend = False
                self.__clientsocket.close()

    def Send(self, n_strCommand):
        if self.__opend:
            send_msg_binary = n_strCommand.encode('utf-8')
            self.__clientsocket.send(send_msg_binary)
            print(send_msg_binary)
            return 0
        else:
            return -1

class SocketAsClient(AbstractSocket):
    __adress = ""

    def __init__(self,Address,BindPort,BufferSize,nPublisher):
        self.__adress = Address
        self.__bind_port= BindPort
        self.__buffersize=BufferSize
        self.__publisher=nPublisher

    def Connect(self):
        super().Connect
        #ソケットを作成
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as self.__mClient:
        #IPアドレスとポート設定
            if self.__adress == "host":
                host= socket.gethostname()
                self.__adress = socket.gethostbyname(host)
            self.__mClient.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.__mClient.connect((self.__adress, self.__bind_port))
            self.__publisher.log.Write(f"CLIENT_{self.__adress}")
            # 通信を開始する
            while True:
                # 通信状態フラグを上げる
                self.__opend = True
                while True:
                    try:
                        # 受信コマンドを待つ
                        rec = self.__mClient.recv(self.__buffersize)
                        recmsg = rec.decode('utf-8')
                        if self.__publisher.handle_connect == True:
                            self.__publisher.Action(recmsg)
                        print(recmsg)
                    except:
                        recmsg = "end"
                        break
                    # endコマンドが来たら終了し、接続待ち状態になる
                    if recmsg == "end":
                        break
                # 通信状態フラグを下げる
                self.__opend = False
                self.__mClient.close()
                if recmsg == "end":
                    break
    
    def Send(self, n_strCommand):
        # 通信を可能な時のみ行う
        if self.__opend:
            # コマンド送信
            sendmsg = n_strCommand.encode('utf-8')
            self.__mClient.send(sendmsg)
            print(sendmsg)
            if n_strCommand == "end":
                self.__mClient.close()
            return 0
        else:
            # そもそも通信不可
            return -1

if __name__ == '__main__':
    typeConnect = input("Server? Client? : ")
    if  typeConnect == "S":
        c_socket = Socket(Socket.Mode.tpServer, "")
    elif typeConnect == "C" :
        str_adress = input("Adress : ")
        c_socket = Socket(Socket.Mode.tpClient, str_adress)
    else:
        quit
        
    c_socket.setDaemon(True)
    c_socket.start()

    while True:
        key = input("key :")
        if key == "quit":
            c_socket.Send("end")
            break
        else:
            c_socket.Send(key)
        if (typeConnect == "C") & (key == "end"):
            break