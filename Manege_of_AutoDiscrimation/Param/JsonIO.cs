using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Reflection;

namespace Manege_of_AutoDiscrimation.Param
{

    public class CJsonIO<T> where T : class
    {

        /// <summary>
        /// Json形式ファイルからパラメータ値を抜き出し、インスタンスに代入する
        /// </summary>
        /// <param name="nstrFilePath">読み込むファイルのパス</param>
        /// <param name="ncParameter">読み込まれたパラメータを代入するインスタンス</param>
        /// <returns>0:正常終了、-1:ファイルからの読み込み失敗、-2:json構文エラー、-3:異常値が代入された</returns>
        public static int Load(string nstrFilePath, ref T ncParameter)
        {
            string str_jsonfile_sentence;
            int i_ret;
            try
            {
                // ファイルから文字列を丸ごと抜き出す
                str_jsonfile_sentence = File.ReadAllText(nstrFilePath);
            }
            catch
            {
                // ファイルからの読み込み失敗
                return -1;
            }

            try
            {
                // コメントアウトの箇所を削除した文字列をデシリアライズする
                ncParameter = JsonConvert.DeserializeObject<T>(str_jsonfile_sentence);
            }
            catch
            {
                // json構文エラー
                return -2;
            }
            return 0;
        }

        /// <summary>
        /// インスタンスをシリアライズし、Json形式ファイル書き出す
        /// </summary>
        /// <param name="nstrFilePath">書き出すファイルのパス</param>
        /// <param name="ncParameter">書き出すパラメータを保有するインスタンス</param>
        /// <returns>0:正常終了、-1:ファイル作成・書き込みエラー</returns>
        public static int Save(string nstrFilePath, T ncParameter)
        {
            Encoding encd_encoding = Encoding.GetEncoding("utf-8");
            // パラメータをシリアライズする
            string str_json_contents = JsonConvert.SerializeObject(ncParameter, Formatting.Indented);
            //// パラメータ文字列にコメントを追加する(現在、未実装)
            //AddDescriptionOfParameter(ref str_json_contents);

            try
            {
                // jsonファイルを作成する
                using (FileStream fs = File.Create(nstrFilePath)) { }
                // jsonファイルにパラメータ文字列を書き込む
                using (StreamWriter writer = new StreamWriter(nstrFilePath, false, encd_encoding))
                {
                    writer.WriteLine(str_json_contents);
                }
                return 0;
            }
            catch
            {
                // ファイル作成・書き込みエラー
                return -1;
            }
        }
    }
}

