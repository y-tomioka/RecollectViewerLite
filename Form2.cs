using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AIChatViewer
{
    public partial class Form2 : Form
    {
        JsonData m_jsonData;

        public Form2(JsonData jsonData)
        {
            InitializeComponent();
            m_jsonData = jsonData;
            textBox1.Text = m_jsonData.geminiPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_jsonData.geminiPath = textBox1.Text;
            AppSettings appSettings = new AppSettings();
            appSettings.GeminiJsonPath = textBox1.Text;
            appSettings.Save();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 1. JSONファイルのみを表示・選択できるようにフィルターを設定
                openFileDialog.Filter = "JSONファイル (*.json)|*.json";
                openFileDialog.FilterIndex = 1; // デフォルトで「JSONファイル」を選択状態にする
                openFileDialog.Title = "Geminiの履歴データ(JSON)を選択してください";
                openFileDialog.RestoreDirectory = true; // 前回開いたディレクトリを記憶する

                // 2. ダイアログを表示し、ユーザーが[OK]を押したか判定
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 3. 選択されたファイルのフルパスを変数に格納
                    textBox1.Text = openFileDialog.FileName;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {
        }
    }
}
