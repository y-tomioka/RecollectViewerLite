using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AIChatViewer
{
    public partial class Form1 : Form
    {
        JsonData m_jsonData = new JsonData();
        List<Class1> m_geminidata = new List<Class1>();
        //private bool m_suppressSuggest;

        public Form1()
        {
            InitializeComponent();

            webView21.WebMessageReceived += webView21_WebMessageReceived;

            AppSettings appSetting = AppSettings.Load();
            m_jsonData.geminiPath = appSetting.GeminiJsonPath;
            this.WindowState = FormWindowState.Maximized;

            displayTreeForGemini();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            splitContainer1.Dock = DockStyle.Fill;
            treeView1.Dock = DockStyle.Fill;
            webView21.Dock = DockStyle.Fill;
            splitContainer1.SplitterDistance = 350;
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

            // ==========================================
            // ★追加：起動時の初期メッセージ（ウェルカム画面）
            // ==========================================
            string treeHtml = @"
                <!DOCTYPE html><html><body style='font-family: Meiryo, sans-serif; display: flex; justify-content: center; align-items: center; height: 90vh; background-color: #f8f9fa; color: #555;'>
                    <div style='text-align: center;'>
                        <h2 style='color: #0d47a1;'>🌳 Tree View</h2>
                        <p>Load your AI JSON file from the Settings menu in the top-left corner.</p>
                        <p>After loading, select a chat from the tree on the left to view its contents.</p>
                    </div>
                </body></html>";

            // WebView2にメッセージを表示
            webView21.NavigateToString(treeHtml);
        }

        private void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // 1. メッセージ全体を受け取る（例: "GPT:file-12345" または "GEMINI:C:\path\to\image.png"）
                string rawMessage = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(rawMessage)) return;

                // ==========================================
                // ▼ 年月一覧から個別のチャットへジャンプする処理
                // ==========================================
                if (rawMessage.StartsWith("OPEN_CHILD:"))
                {
                    if (int.TryParse(rawMessage.Substring(11), out int index))
                    {
                        // 現在選択されている親ノード（年月フォルダ）を取得
                        TreeNode parentNode = treeView1.SelectedNode;
                        if (parentNode != null && parentNode.Nodes.Count > index)
                        {
                            // 指定された子ノードを選択状態にする！
                            // （これを実行するだけで、自動的に AfterSelect が発火してチャット画面に切り替わります）
                            treeView1.SelectedNode = parentNode.Nodes[index];
                        }
                    }
                    return;
                }
                // ==========================================
                // ▼ Geminiの添付ファイルを開く処理（拡張子補完対応）
                // ==========================================
                if (rawMessage.StartsWith("GEMINI:"))
                {
                    string relPath = rawMessage.Substring(7);
                    string filePath = relPath;

                    // JSONファイルの場所を基準に絶対パス化
                    string geminiDir = System.IO.Path.GetDirectoryName(m_jsonData.geminiPath);
                    if (!System.IO.Path.IsPathRooted(filePath))
                    {
                        filePath = System.IO.Path.Combine(geminiDir, relPath);
                    }

                    // 実際のファイルに拡張子がない場合を想定し、拡張子なしのパスをチェック
                    if (!System.IO.File.Exists(filePath))
                    {
                        string noExtPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath));
                        if (System.IO.File.Exists(noExtPath)) filePath = noExtPath;
                    }

                    if (System.IO.File.Exists(filePath))
                    {
                        // JSONには拡張子(.cs等)があるのに、実際のファイルに無い場合、tempで復元する
                        string extInJson = System.IO.Path.GetExtension(relPath);
                        string extOnDisk = System.IO.Path.GetExtension(filePath);

                        if (!string.IsNullOrEmpty(extInJson) && string.IsNullOrEmpty(extOnDisk))
                        {
                            string tempDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                            System.IO.Directory.CreateDirectory(tempDir);
                            string tempFilePath = System.IO.Path.Combine(tempDir, System.IO.Path.GetFileNameWithoutExtension(filePath) + extInJson);
                            System.IO.File.Copy(filePath, tempFilePath, true);
                            filePath = tempFilePath; // 開く対象をtempファイルにすり替える
                        }

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("The specified attachment file was not found.\n" + filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void displayTreeForGemini()
        {
            if (string.IsNullOrEmpty(m_jsonData.geminiPath) || !System.IO.File.Exists(m_jsonData.geminiPath))
            {
                //MessageBox.Show("JSONファイルが見つかりません。パスを確認してください。", "エラー");
                m_geminidata = new List<Class1>();
                return;
            }

            GeminiConverter geminiConverter = new GeminiConverter();
            m_geminidata = geminiConverter.deserializeJson(m_jsonData.geminiPath);
            NodeGenerator nodeGenerator = new NodeGenerator();
            nodeGenerator.makeGeminiNode(treeView1, m_geminidata);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (webView21.CoreWebView2 == null) return;

            TreeNode selectedNode = e.Node;

            // StringBuilderを使って、HTML文字列を効率的に組み立てる
            StringBuilder sb = new StringBuilder();

            // Geminiの場合
            if (e.Node.Tag is TreeNodeData nodeData)
            {
                HTMLGenerator htmlGenerator = new HTMLGenerator();
                htmlGenerator.makeGeminiHtml(sb, nodeData, m_jsonData.geminiPath);
            }

            // 1. 実行ファイルと同じ階層に「temp」フォルダのパスを作成
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempDir = System.IO.Path.Combine(baseDir, "temp");

            // 2. tempフォルダが存在しない場合は作成する
            if (!System.IO.Directory.Exists(tempDir))
            {
                System.IO.Directory.CreateDirectory(tempDir);
            }

            // 3. ファイル名が被ってアクセスエラーになるのを防ぐため、一意のファイル名を生成
            string fileName = $"view_{Guid.NewGuid():N}.html";
            string tempFilePath = System.IO.Path.Combine(tempDir, fileName);

            // 4. StringBuilder (sb) の中身を一時ファイルに書き込む
            System.IO.File.WriteAllText(tempFilePath, sb.ToString());

            // 5. WebView2 に一時ファイルのパスを渡して表示
            webView21.CoreWebView2.Navigate(tempFilePath);
        }

        private void listViewSearch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(m_jsonData);
            if(form2.ShowDialog() == DialogResult.OK)
            {
                treeView1.Nodes.Clear();
                displayTreeForGemini();
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
 
        }

        // ツリーの中から、特定のTag（データオブジェクト）を持っているノードを探し出す再帰メソッド
        private TreeNode FindNodeByTag(TreeNodeCollection nodes, object targetTag)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag == targetTag) return node;

                TreeNode foundInChildren = FindNodeByTag(node.Nodes, targetTag);
                if (foundInChildren != null) return foundInChildren;
            }
            return null;
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RecollectViewer Lite ver:0.1");
        }

        // =========================================================
        // ★追加：WebView2内でリンクがクリックされた時の制御
        // =========================================================
        private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            // もし遷移先のURLが「http://」や「https://」から始まる外部のWebサイトだったら
            if (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://"))
            {
                // 1. WebView2 内部での画面の切り替わりを「キャンセル（無効化）」する
                e.Cancel = true;

                // 2. 代わりに、Windowsの既定のブラウザ（Chrome等）でURLを開く
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = e.Uri,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open the link.\n\nDetails: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void アップデート確認ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUrl("http://www.yasui-kamo.com/labo/recollectviewer/");
        }

        public static void OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // ブラウザ起動失敗時のログ出力やエラーハンドリング
                Debug.WriteLine($"ブラウザの起動に失敗しました: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }
    }
}
