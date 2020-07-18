using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Xml2ParaTranz
{
    [JsonObject("ParaTranzItem")]
    public class ParaTranzItem
    {
        public ParaTranzItem()
        {
            this.key = "";
            this.original = "";
            this.translation = "";
            this.context = "";
        }
        public ParaTranzItem(string key, string original)
        {
            this.key = key;
            this.original = original;
            this.translation = "";
            this.context = "";
        }
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("original")]
        public string original { get; set; }
        [JsonProperty("translation")]
        public string translation { get; set; }
        [JsonProperty("context")]
        public string context { get; set; }
    }

    class Xml2ParaTranz
    {
        public void Convert(string src_path)
        {
            //ParaTranz形式ファイルの出力先パスを組み立てる
            //src_pathのディレクトリを取得
            var src_dir = System.IO.Path.GetDirectoryName(src_path);
            //src_pathの拡張子を除くファイル名を取得
            var src_name = System.IO.Path.GetFileNameWithoutExtension(src_path);
            //出力先ディレクトリパス
            var dst_dir = src_dir + "/ParaTranz/en/";
            //出力先ディレクトリが存在しなければ作成
            if (!Directory.Exists(dst_dir))
            {
                System.IO.Directory.CreateDirectory(dst_dir);
            }
            //出力先ファイルパス
            var dst_path = dst_dir + src_name + ".json";

            //ParaTranz形式アイテムのリストを用意（出力用）
            List<ParaTranzItem> paraTranzItems = new List<ParaTranzItem>();

            //XMLとして不正な文字が含まれてる場合があるのでチェックを切っている
            //切ってても引っかかるやつはひっかかるけどそれはもう諦める
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
            using (XmlReader xmlReader = XmlReader.Create(src_path, xmlReaderSettings))
            {
                xmlReader.MoveToContent();
                XDocument xDocument = XDocument.Load(xmlReader);

                IEnumerable<XElement> de = from el in xDocument.Descendants() select el;

                //元ファイルをトラバース
                int n = 0;
                foreach (XElement el in de)
                {
                    if (el.Elements().Count() == 0)
                    {
                        //ノードのテキスト
                        var text = el.Value;
                        if (text == "")
                        {
                            //空文字列は無視
                            continue;
                        }

                        //改行を\\nに置換する
                        text = System.Text.RegularExpressions.Regex.Replace(text, "\n", "\\n");

                        //ParaTranz形式アイテムリストに追加
                        paraTranzItems.Add(new ParaTranzItem(text, text));
                        n++;
                    }
                }

                // シリアライザ
                if (paraTranzItems.Count() > 0)
                {
                    using (var writer = new StreamWriter(dst_path))
                    {
                        //全部ParaTranz形式アイテムリストに追加したのでファイル出力
                        string json = JsonConvert.SerializeObject(paraTranzItems, Newtonsoft.Json.Formatting.Indented);
                        writer.Write(json);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }

        }
        static void Main(string[] args)
        {
            //引数なしはNG
            if(args.Length == 0)
            {
                return;
            }

            //最初の引数をパスとして取得
            var path = args[0];

            if (File.Exists(path))
            {
                //パスがファイルなら

                //src_pathの拡張子を取得
                var extension = System.IO.Path.GetExtension(path);
                if (extension != ".xml")
                {
                    //拡張子.xml以外はNG
                    return;
                }

                //src_pathの拡張子を除くファイル名を取得
                var src_name = System.IO.Path.GetFileNameWithoutExtension(path);

                //xxx_jp.xmlはスキップ
                if(src_name.EndsWith("_jp"))
                {
                    return;
                }

                // ParaTranz形式に変換して出力
                try
                {
                    Xml2ParaTranz xml2ParaTranz = new Xml2ParaTranz();
                    xml2ParaTranz.Convert(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(path + " convert FAIL! " + e.ToString());
                    return;
                }
            }
            else if (Directory.Exists(path))
            {
                //パスがディレクトリなら

                //ディレクトリ内の.xmlファイルを全部ParaTranz形式に変換して出力
                foreach(string f in System.IO.Directory.GetFiles(path))
                {
                    //src_pathの拡張子を取得
                    var extension = System.IO.Path.GetExtension(f);
                    if (extension != ".xml")
                    {
                        //拡張子.xml以外はNG
                        continue;
                    }

                    //src_pathの拡張子を除くファイル名を取得
                    var src_name = System.IO.Path.GetFileNameWithoutExtension(f);

                    //xxx_jp.xmlはスキップ
                    if (src_name.EndsWith("_jp"))
                    {
                        continue;
                    }

                    // ParaTranz形式に変換して出力
                    try
                    {
                        Xml2ParaTranz xml2ParaTranz = new Xml2ParaTranz();
                        xml2ParaTranz.Convert(f);
                        Console.WriteLine(f + " convert SUCCESS");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(f + " convert FAIL! " + e.ToString());
                    }
                }
            }
            else
            {
                //存在しないパスはNG
                return;
            }

        }
    }
}
