from ast import Not
import os
from datetime import datetime as dt

class Log():
    # 書き込むファイルのパス
    m_strFilePath = ""

    # 書き込むファイルを作成
    # 引数説明(自己インスタンス、書き込みファイル名)
    def __init__(self, n_strFileName):
        str_logfolder = ".\log"
        # ログフォルダーの存在確認
        if not os.path.exists(str_logfolder):
            # 無ければ作成
            os.mkdir(str_logfolder)
        # 与えられたファイル名の形式がテキストであるかの確認
        if n_strFileName.find('.txt') == -1:
            n_strFileName += ".txt"
        # 書きこむファイルパスを作成
        self.m_strFilePath = f"{str_logfolder}\{n_strFileName}"

    # 書き込む関数
    # 引数説明(自己インスタンス、書き込み内容)
    def Write(self, n_strComments):
        # 追加で書き込んでいくモードでファイルを開く
        f = open(self.m_strFilePath, 'a')
        # ファイルに書き込む
        f.writelines(f"{dt.now()} {n_strComments}\n")
        # ファイルを閉じる
        f.close()