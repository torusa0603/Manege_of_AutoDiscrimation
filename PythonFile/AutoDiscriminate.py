import mWatershed
import sys 
import re
import mEvent
import mSocket
import threading

# 呼び出しイベント
# 引数説明(イベントの実像??、イベント発生源からの受け渡しオブジェクト)
def event_handle(sender, earg):
    # 検査開始コマンド受信時の処理
    if earg == "/Start\n":
        # 検査実行
        mWatershed.main(False, result_folder_path, False, True)
        # 検査終了をコマンドとして送信
        c_socket.Send("/DiscrimateEnd\n")
    # プログラム終了コマンド受信時処理
    elif earg == "/Stop\n":
        lock.release()


if __name__ == '__main__':
    # コマンドラインからの入力された文字列"python AutoDiscriminate.py [result_folder_path]"から結果フォルダーパスを取得する
    result_folder_path = str(sys.argv[1])
    # サーバーとしてソケットクラスをインスタンス
    c_socket = mSocket.Socket(mSocket.Socket.Mode.tpServer, "127.0.0.1")
    # 別スレッドにてソケットを開く
    c_socket.setDaemon(True)
    c_socket.start()

    # コマンド受け取り時のイベントを設定
    c_socket.pub.evt += event_handle
    # イベント接続フラグをオンにする
    c_socket.pub.handle_connect = True
    # プログラムが終了しないように待たせておく
    lock = threading.Lock()
    lock.acquire()
    # ここでロックされる、eventhandle内からロック解除する
    lock.acquire()