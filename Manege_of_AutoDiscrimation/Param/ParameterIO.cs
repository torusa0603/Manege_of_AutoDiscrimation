using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Manege_of_AutoDiscrimation.Param
{
    class CParameterIO
    {
        /// <summary>
        /// 設定ファイルの内容を設定用オブジェクトに格納
        /// </summary>
        /// <param name="nstrSettingFilePath">設定ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリが存在しない、-2:設定ファイル作成・書き込みエラー、-3:設定ファイルなし(新規作成)<br />
        /// -4:設定ファイル構文エラー、-5:設定値エラー</returns>
        public static int ReadParameter(string nstrSettingFilePath, ref Parameter ncParameter)
        {

            int i_ret;
            if (!File.Exists(nstrSettingFilePath) || !(Path.GetExtension(nstrSettingFilePath) == ".json"))
            {
                i_ret = CreateSettingFile(nstrSettingFilePath);
                switch (i_ret)
                {
                    case -1:
                        // 設定ファイルの途中パスディレクトリが存在しない
                        return -1;
                    case -2:
                        // 設定ファイル作成・書き込みエラー
                        return -2;
                    default:
                        // 設定ファイルなし(新規作成)
                        return -3;
                }
            }
            i_ret = CJsonIO<Parameter>.Load(nstrSettingFilePath, ref ncParameter);
            switch (i_ret)
            {
                case -1:
                    // ファイルへのアクセス失敗
                    return -4;
                case -2:
                    // 設定ファイル構文エラー
                    return -5;
                case -3:
                    // 異常値が代入された
                    ncParameter = null;
                    return -6;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// 設定ファイルを作成する
        /// </summary>
        /// <param name="nstrSettingFilePath">作成ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリーが存在しない、-2:ファイル作成・書き込みエラー</returns>
        private static int CreateSettingFile(string nstrSettingFilePath)
        {
            // 作成ファイルパスのディレクトリの存在チェック
            string str_setting_file_folder = Path.GetDirectoryName(nstrSettingFilePath);
            if (!Directory.Exists(str_setting_file_folder))
            {
                return -1;
            }

            Encoding encd_encoding = Encoding.GetEncoding("utf-8");

            Parameter c_prm = new Parameter();


            int i_ret = CJsonIO<Parameter>.Save(nstrSettingFilePath, c_prm);
            switch (i_ret)
            {
                case -1:
                    // ファイル作成・書き込みエラー
                    return -2;
                default:
                    break;
            }
                return 0;
        }

        /// <summary>
        /// 設定ファイルを作成する
        /// </summary>
        /// <param name="nstrSettingFilePath">作成ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリーが存在しない、-2:ファイル作成・書き込みエラー</returns>
        public static int SaveParameter(string nstrSettingFilePath, Parameter ncParameter)
        {
            // 作成ファイルパスのディレクトリの存在チェック
            string str_setting_file_folder = Path.GetDirectoryName(nstrSettingFilePath);
            if (!Directory.Exists(str_setting_file_folder))
            {
                return -1;
            }

            Encoding encd_encoding = Encoding.GetEncoding("utf-8");


            int i_ret = CJsonIO<Parameter>.Save(nstrSettingFilePath, ncParameter);
            switch (i_ret)
            {
                case -1:
                    // ファイル作成・書き込みエラー
                    return -2;
                default:
                    break;
            }
            return 0;
        }
    }
}
