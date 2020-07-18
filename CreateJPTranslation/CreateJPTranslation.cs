using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CreateJPTranslation
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

    class CreateJPTranslation
    {
        public void CreateTranslation(string src_path, string translation_path)
        {
            //出力先パスを組み立てる
            //src_pathのディレクトリを取得
            var src_dir = System.IO.Path.GetDirectoryName(src_path);
            //src_pathの拡張子を除くファイル名を取得
            var src_name = System.IO.Path.GetFileNameWithoutExtension(src_path);
            //src_pathの拡張子を取得
            var src_extension = System.IO.Path.GetExtension(src_path);
            //出力先ディレクトリパス
            var dst_dir = src_dir;
            //出力先ファイルパス
            var dst_path = dst_dir + "/" + src_name + "_jp" + src_extension;

            //翻訳したテキスト
            string dst_text = "";
            //元ファイルを読み込む
            using (var sr = new StreamReader(src_path))
            {
                dst_text = sr.ReadToEnd();
            }
            //ParaTranzファイルを読み込む
            using (var sr = new StreamReader(translation_path, System.Text.Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                List<ParaTranzItem> paraTranzItems = JsonConvert.DeserializeObject<List<ParaTranzItem>>(json);
                foreach (ParaTranzItem item in paraTranzItems)
                {
                    var original = item.original;
                    var translation = item.translation;
                    //改行を\\nに置換する
                    original = original.Replace("\\n", "\r\n");
                    translation = translation.Replace("\\n", "\r\n");

                    //ParaTranzファイルのアイテムを使って元ファイルを置換する
                    dst_text = dst_text.Replace(original, translation);
                }
            }
            //置換したファイルを出力
            using (var writer = new StreamWriter(dst_path))
            {
                writer.Write(dst_text);
                writer.Flush();
                writer.Close();
            }
        }

        public void CreateTranslation(string src_path)
        {
            //src_pathの拡張子を取得
            var extension = System.IO.Path.GetExtension(src_path);
            if (extension != ".xml")
            {
                //拡張子.xml以外はNG
                return;
            }

            //src_pathの拡張子を除くファイル名を取得
            var src_name = System.IO.Path.GetFileNameWithoutExtension(src_path);

            //xxx_jp.xmlはスキップ
            if (src_name.EndsWith("_jp"))
            {
                return;
            }

            //src_pathのディレクトリを取得
            var src_dir = System.IO.Path.GetDirectoryName(src_path);
            //src_pathの拡張子を取得
            var src_extension = System.IO.Path.GetExtension(src_path);
            //ParaTranzファイルパス
            var translation_path = src_dir + "/ParaTranz/jp/" + src_name + ".json";

            if (!File.Exists(translation_path))
            {
                //ParaTranzファイルなしならスキップ
                return;
            }

            //出力先ディレクトリパス
            var dst_dir = src_dir;
            //出力先ファイルパス
            var dst_path = dst_dir + src_name + "_jp" + src_extension;
            // ParaTranz形式に変換して出力
            try
            {
                CreateTranslation(src_path, translation_path);
                Console.WriteLine(src_path + " convert SUCCESS");
            }
            catch (Exception e)
            {
                Console.WriteLine(src_path + " convert FAIL! " + e.ToString());
            }
        }
        static void Main(string[] args)
        {
            //引数なしはNG
            if (args.Length == 0)
            {
                return;
            }
            //最初の引数をパスとして取得
            var path = args[0];

            if (File.Exists(path))
            {
                //パスがファイルなら
                var f = path;
                CreateJPTranslation createJPTranslation = new CreateJPTranslation();
                createJPTranslation.CreateTranslation(f);
            }
            else if (Directory.Exists(path))
            {
                //パスがディレクトリなら

                //ディレクトリ内の.xmlファイル全部から翻訳ファイル作成
                foreach (string f in System.IO.Directory.GetFiles(path))
                {
                    CreateJPTranslation createJPTranslation = new CreateJPTranslation();
                    createJPTranslation.CreateTranslation(f);

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
