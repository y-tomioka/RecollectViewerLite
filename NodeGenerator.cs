using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIChatViewer
{
    public class NodeGenerator
    {
        public void makeGeminiNode(TreeView treeView1, List<Class1> geminidata)
        {
            GeminiConverter geminiConverter = new GeminiConverter();

            // 描画を一時停止（ノード追加中のチラつき防止と速度向上のための定石です）
            treeView1.BeginUpdate();
            //treeView1.Nodes.Clear();

            // 1. 最上位のルートノード「Gemini」を作成
            TreeNode rootNode = new TreeNode("Gemini");
            treeView1.Nodes.Add(rootNode);

            // 2. 年の階層を作成
            var yearList = geminiConverter.getYearTreeData(geminidata);
            foreach (var yearItem in yearList)
            {
                // ノードのテキストは "2026年 (30件)" のようにする
                TreeNode yearNode = new TreeNode($"{yearItem.date} ({yearItem.count}件)");
                // 後で検索やWebView連携で使えるように、裏側(Tag)に元データを丸ごと持たせておく
                yearNode.Tag = yearItem;
                rootNode.Nodes.Add(yearNode);

                // dateプロパティ("2026年")から数字だけ取り出してDateTimeを作る（次の月検索のため）
                int year = int.Parse(yearItem.date.Replace("年", ""));
                DateTime dummyYearDate = new DateTime(year, 1, 1);

                // 3. 月の階層を作成
                var monthList = geminiConverter.getMonthTreeData(dummyYearDate, geminidata);
                foreach (var monthItem in monthList)
                {
                    TreeNode monthNode = new TreeNode($"{monthItem.date} ({monthItem.count}件)");
                    monthNode.Tag = monthItem;
                    yearNode.Nodes.Add(monthNode);

                    // dateプロパティ("2026年4月")から月の数字だけ取り出す
                    string monthStr = monthItem.date.Replace($"{year}年", "").Replace("月", "");
                    int month = int.Parse(monthStr);
                    DateTime dummyMonthDate = new DateTime(year, month, 1);

                    // 4. 日の階層を作成
                    var dayList = geminiConverter.getDayTreeData(dummyMonthDate, geminidata);
                    foreach (var dayItem in dayList)
                    {
                        TreeNode dayNode = new TreeNode($"{dayItem.date} ({dayItem.count}件)");
                        dayNode.Tag = dayItem;
                        monthNode.Nodes.Add(dayNode);
                    }
                }
            }

            // アプリ起動時に「Gemini」ノードだけは開いた状態にしておく
            rootNode.Expand();

            // 描画を再開
            treeView1.EndUpdate();
        }
    }
}
