using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AIChatViewer
{
    public class AppSettings
    {
        public string GeminiJsonPath { get; set; } = "";

        // 設定ファイルの保存先（実行ファイルと同じフォルダの settings.xml）
        private static string SettingsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting\\settings.xml");

        // 設定をXMLファイルに保存する
        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StreamWriter sw = new StreamWriter(SettingsPath, false, System.Text.Encoding.UTF8))
            {
                serializer.Serialize(sw, this);
            }
        }

        // XMLファイルから設定を読み込む
        public static AppSettings Load()
        {
            if (!File.Exists(SettingsPath)) return new AppSettings();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (StreamReader sr = new StreamReader(SettingsPath, System.Text.Encoding.UTF8))
                {
                    return (AppSettings)serializer.Deserialize(sr);
                }
            }
            catch
            {
                // 読み込み失敗時は初期値を返す
                return new AppSettings();
            }
        }
    }
}
