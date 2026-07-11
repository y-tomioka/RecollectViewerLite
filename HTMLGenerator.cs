using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIChatViewer
{
    public class HTMLGenerator
    {
        private static string m_cssContent = "";
        private static string m_jsContent = "";

        private static readonly string[] GeminiTitlePrefixes = new[]
        {
            "送信したメッセージ:",
            "Sent message:",
            "Prompt sent:",
            "Message sent:",
            "フィードバックを送信しました:",
            "Feedback sent:",
        };
        public HTMLGenerator()
        {
            // 実行ファイル（.exe）があるフォルダのパスを取得
            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            // CSSとJSの中身をテキストとして丸ごと読み込む
            m_cssContent = System.IO.File.ReadAllText(System.IO.Path.Combine(appDir, "atom-one-dark.min.css"));
            m_jsContent = System.IO.File.ReadAllText(System.IO.Path.Combine(appDir, "highlight.min.js"));
        }
        public void makeGeminiHtml(StringBuilder sb, TreeNodeData nodeData, string path)
        {
            // ―― HTMLのヘッダー部分 ――
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");

            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            // 1. デザインテーマを直接埋め込み（<style>タグを使用）
            sb.AppendLine("<style>");
            sb.AppendLine(m_cssContent);
            sb.AppendLine("</style>");
            // 2. ライブラリ本体を直接埋め込み（<script>タグを使用）
            sb.AppendLine("<script>");
            sb.AppendLine(m_jsContent);
            sb.AppendLine("</script>");
            // 3. 実行コマンド（画面が読み込まれたら自動で色付けする）
            sb.AppendLine(@"<script>
            // 自動判定の精度を上げるため、推測する言語の候補を主要言語に絞る
            hljs.configure({
                languages: [
                    'csharp', 'cpp', 'c', 'java', 'php', 'go', 'ruby', 'kotlin',
                    'javascript', 'typescript', 'python',
                    'json', 'xml', 'css', 'sql', 'bash'
                ]
            });
            hljs.highlightAll();
        </script>");
            sb.AppendLine(@"<style>
            /* コードブロック全体の装飾 */
            pre {
                position: relative; /* コピーボタンの配置基準にするため必須 */
                background-color: #282c34 !important; /* ダーク風の背景色 */
                color: #abb2bf !important; /* 文字色 */
                padding: 40px 15px 15px 15px !important; /* 上部にボタン用の余白を取る */
                border-radius: 6px;
                overflow-x: auto;

                /* ▼▼ ここから追加 ▼▼ */
                font-family: Consolas, ""Courier New"", monospace !important;
                font-size: 0.95em; /* 少しだけ文字を大きくして視認性アップ */
                line-height: 1.5;  /* 行間を少し開けて詰まり感をなくす */
            }
            /* さらに、preの中のcodeタグにも念のため適用 */
            pre code {
                font-family: Consolas, ""Courier New"", monospace !important;
            }
            /* コピーボタンのデザイン */
            .copy-btn {
                position: absolute;
                top: 8px;
                right: 8px;
                padding: 4px 8px;
                background-color: #444;
                color: #fff;
                border: 1px solid #666;
                border-radius: 4px;
                cursor: pointer;
                font-size: 12px;
                transition: 0.2s;
            }
            .copy-btn:hover {
                background-color: #666;
            }
            /* ▼▼ 追加：言語名ラベルのデザイン ▼▼ */
            .lang-label {
                position: absolute;
                top: 0;
                left: 0;
                padding: 2px 8px;
                background-color: #444; /* コピーボタンと同じ色 */
                color: #abb2bf;
                font-size: 11px;
                font-weight: bold;
                border-bottom-right-radius: 6px;
                text-transform: uppercase; /* 強制的に大文字にする (例: csharp -> CSHARP) */
            }
            .answer-content p { line-height: 1.6; }
            .answer-content code:not(pre code) { background-color: #e9ecef; padding: 2px 4px; border-radius: 3px; color: #d63384; font-family: Consolas, monospace; }
        </style>");

                sb.AppendLine(@"<script>
                document.addEventListener('DOMContentLoaded', () => {
                // まずhighlight.jsを適用
                hljs.configure({
                    languages: ['csharp', 'cpp', 'c', 'java', 'php', 'go', 'ruby', 'kotlin', 'javascript', 'typescript', 'python', 'json', 'xml', 'css', 'sql', 'bash']
                });
                hljs.highlightAll();

                const preTags = document.querySelectorAll('pre');
                preTags.forEach((pre) => {
                    const codeBlock = pre.querySelector('code');

                    // ==========================================
                    // 既存：コピーボタンの処理
                    // ==========================================
                    const btn = document.createElement('button');
                    btn.className = 'copy-btn';
                    btn.innerText = 'Copy';
                    btn.addEventListener('click', () => {
                        const textToCopy = codeBlock ? codeBlock.innerText : pre.innerText;
                        navigator.clipboard.writeText(textToCopy).then(() => {
                            btn.innerText = 'Copied!';
                            setTimeout(() => { btn.innerText = 'Copy'; }, 2000);
                        });
                    });
                    pre.appendChild(btn);
                });
            });
        </script>");
            sb.AppendLine("</head>");

            sb.AppendLine("<body style='font-family: Segoe UI, system-ui, sans-serif; padding: 20px; background-color: #f0f2f5;'>");
            sb.AppendLine($"<h2 style='color: #333;'>{nodeData.date} - Conversations ({nodeData.count} items)</h2>");
            sb.AppendLine("<hr style='border: 1px solid #ccc; margin-bottom: 20px;'>");

            // ―― 相談内容をループして一覧化 ――
            foreach (var item in nodeData.Items)
            {
                // 時間とタイトルの取得（タイトルが空なら「タイトルなし」とする）
                string timeStr = item.Time.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
                string title = string.IsNullOrEmpty(item.Title) ? "(No title)" : StripGeminiTitlePrefix(item.Title);
                title = title.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace(" ", "&nbsp;");

                // ★ここからが「折りたたみUI」の魔法（detailsタグ）
                sb.AppendLine("<details style='margin-bottom: 15px; background-color: #f8f9fa; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>");

                // クリックできるタイトル部分（summaryタグ）
                sb.AppendLine($"  <summary style='cursor: pointer; font-size: 1.1em; font-weight: bold; color: #333333;'>");
                sb.AppendLine($"    <font color=lightblue;>[{timeStr}]</font> {title}");
                sb.AppendLine($"  </summary>");

                // 展開された時に表示される中身（実際のAIの回答HTML）
                //sb.AppendLine("  <div style='margin-top: 15px; padding-top: 15px; border-top: 1px dashed #eee; background-color: white;'>");
                sb.AppendLine("  <div class='answer-content' style='margin-top: 15px; padding-top: 15px; border-top: 1px dashed #eee; background-color: white;'>");
                if (item.SafeHtmlItems != null && item.SafeHtmlItems.Length > 0)
                {
                    AppendAttachmentLinks(sb, item, path);

                    //sb.AppendLine(item.SafeHtmlItems[0].Html);
                    string rawHtml = item.SafeHtmlItems[0].Html;
                    string cleanHtml = CleanAiMetadata(rawHtml);
                    string cleanText = System.Text.RegularExpressions.Regex.Replace(cleanHtml ?? "", "<[^>]*>", "").Trim();
                    System.Diagnostics.Debug.WriteLine($"[makeGeminiHtml] rawLen={rawHtml?.Length ?? -1} cleanLen={cleanHtml?.Length ?? -1} textLen={cleanText.Length}");
                    if (string.IsNullOrWhiteSpace(cleanText))
                    {
                        sb.AppendLine("<p style='color: #999; font-style: italic;'>No text response</p>");
                    }
                    else
                    {
                        sb.AppendLine(cleanHtml);
                    }
                }
                else
                {
                    sb.AppendLine("<p style='color: #999; font-style: italic;'>No text response</p>");
                }
                sb.AppendLine("  </div>");

                sb.AppendLine("</details>");
            }
            // 3. （念のため）ハイライトを実行する命令
            sb.AppendLine("<script>hljs.highlightAll();</script>");
            // ―― HTMLのフッター部分 ――
            sb.AppendLine("</body></html>");
        }

        // =========================================================
        // ★追加：Geminiの内部タグ（□entity□ や □cite□）を綺麗に掃除するメソッド
        // =========================================================
        public static string CleanAiMetadata(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // 1. citeタグ（Web検索の引用元）を跡形もなく削除
            // 例: \uE200cite\uE202turn0search2... \uE201
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\uE200cite\uE202[^\uE201]*\uE201?", "");

            // 2. entityタグ（用語解説リンク）を、表示テキストだけ残して置換
            // 例: \uE200entity\uE202["disease", "統合失調症", 0]\uE201 -> 「統合失調症」だけを抽出
            string entityPattern = @"\uE200entity\uE202\[[^,]+,\s*(?:&quot;|"")(.*?)(?:&quot;|"")[^\]]*\]\uE201?";
            text = System.Text.RegularExpressions.Regex.Replace(text, entityPattern, "$1");

            // 3. 念のため残った制御文字（秀丸で ■ や ☆ や ↲ に見えていた文字）を完全消去
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[\uE200-\uE202]", "");

            // 4. ChatGPTの添付ファイル引用指示を削除
            //    例: "Make sure to include fileciteturn1file0 in your response to cite this file."
            text = System.Text.RegularExpressions.Regex.Replace(text, @"Make sure to include filecite\w+ in your response to cite this file\.?\s*", "");

            // 5. テキスト中に残った filecite トークン自体も削除
            //    例: "fileciteturn1file0" や "【fileciteturn0file2】"
            text = System.Text.RegularExpressions.Regex.Replace(text, @"【?fileciteturn\d+file\d+】?", "");

            return text;
        }

        // =========================================================
        // ★追加：Word/Office由来の内部情報（XMLメタデータ等）を切り捨てるメソッド
        //   Word文書を貼り付けた際に紛れ込む mso-* スタイルや
        //   schemas-microsoft-com 名前空間、条件付きコメント等を除去する。
        // =========================================================
        public static string StripOfficeMetadata(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline;

            // 1. Wordの条件付きコメント（<!--[if gte mso ...]> ... <![endif]-->）を削除
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<!--\[if[^>]*?\]>.*?<!\[endif\]-->", "", options);

            // 2. XML宣言や <xml>...</xml> ブロック（Wordのドキュメントプロパティ等）を削除
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<\?xml[^>]*?\?>", "", options);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<xml\b.*?</xml>", "", options);

            // 3. Office名前空間付きタグ（<o:p>, <w:WordDocument>, <m:...> 等）を削除
            text = System.Text.RegularExpressions.Regex.Replace(text, @"</?(?:o|w|m|v|x|st1):[^>]*>", "", options);

            // 4. mso-* スタイルや schemas-microsoft-com 名前空間宣言を含む属性を削除
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*(?:style|xmlns(?::[\w-]+)?)\s*=\s*""[^""]*(?:mso-|schemas-microsoft-com)[^""]*""", "", options);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*(?:style|xmlns(?::[\w-]+)?)\s*=\s*'[^']*(?:mso-|schemas-microsoft-com)[^']*'", "", options);

            // 5. 行単位で残った mso-* / panose-1 等の宣言を削除
            text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*(?:mso-|panose-1)[^\r\n]*$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

            return text;
        }

        
        // ====================================================
        // ▼ 左側のリスト検索用の「厳格なブロック判定」メソッド ▼
        // ====================================================

        // Gemini用：1つのブロック（相談＋回答）にすべてのキーワードが含まれているか？
        public static bool IsStrictMatchGemini(Class1 session, string[] keywords)
        {
            if (keywords == null || keywords.Length == 0) return true;
            if (session.SafeHtmlItems == null) return false;

            foreach (var item in session.SafeHtmlItems)
            {
                string blockText = session.Title + "\n" + (item.Html ?? "");
                // 1つのブロックの中に、すべてのキーワードが含まれているか（AND条件）
                if (keywords.All(kw => blockText.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true; // 1つでも完全一致するブロックがあれば合格！
                }
            }
            return false;
        }

        private static string StripGeminiTitlePrefix(string title)
        {
            foreach (string prefix in GeminiTitlePrefixes)
            {
                if (title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return title.Substring(prefix.Length).TrimStart();
                }
            }
            return title;
        }

        private static string GetAttachmentDisplayName(Class1 item, string fileName)
        {
            Subtitle matched = item.Subtitles?.FirstOrDefault(s =>
                !string.IsNullOrEmpty(s.Url) &&
                string.Equals(s.Url, fileName, StringComparison.OrdinalIgnoreCase));

            if (matched != null && !string.IsNullOrWhiteSpace(matched.Name))
            {
                string name = matched.Name.Trim();
                if (name.StartsWith("-"))
                {
                    name = name.TrimStart('-').Trim();
                }
                return name;
            }

            return System.IO.Path.GetFileName(fileName);
        }

        private static void AppendAttachmentLinks(StringBuilder sb, Class1 item, string jsonPath)
        {
            if (item.AttachedFiles == null || item.AttachedFiles.Length == 0)
            {
                return;
            }

            string jsonDir = System.IO.Path.GetDirectoryName(jsonPath);
            foreach (string fileName in item.AttachedFiles)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                string fullPath = System.IO.Path.Combine(jsonDir, fileName);
                string escapedUrl = fullPath.Replace("\\", "\\\\");
                string displayName = WebUtility.HtmlEncode(GetAttachmentDisplayName(item, fileName));

                sb.AppendLine("<div style='margin-top: 8px;'>");
                sb.AppendLine($"  <a href='#' onclick=\"window.chrome.webview.postMessage('GEMINI:{escapedUrl}'); return false;\" style='color: #0056b3; text-decoration: underline; cursor: pointer; font-size: 0.9em;'>");
                sb.AppendLine($"    📎 {displayName}");
                sb.AppendLine("  </a>");
                sb.AppendLine("</div>");
            }
        }

    }
}
