using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace AIChatViewer
{
    public class Rootobject
    {
        public List<Class1> Property1 { get; set; }
    }

    public class Class1
    {
        [JsonProperty("header")]
        public string Header { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("products")]
        public string[] Products { get; set; }

        [JsonProperty("activityControls")]
        public string[] ActivityControls { get; set; }

        [JsonProperty("safeHtmlItem")]
        public Safehtmlitem[] SafeHtmlItems { get; set; }

        [JsonProperty("subtitles")]
        public Subtitle[] Subtitles { get; set; }

        [JsonProperty("imageFile")]
        public string ImageFile { get; set; }

        [JsonProperty("attachedFiles")]
        public string[] AttachedFiles { get; set; }
    }

    public class Safehtmlitem
    {
        [JsonProperty("html")]
        public string Html { get; set; }
    }

    public class Subtitle
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class TreeNodeData
    {
        public string date;
        public int count;
        public List<Class1> Items;
        public TreeNodeData()
        {
            date = "";
            count = 0;
            Items = new List<Class1>();
        }
    }

    public class GeminiConverter
    {
        public List<Class1> deserializeJson(string jsonPath)
        {
            string jsonString = File.ReadAllText(jsonPath);
            List<Class1> activities = JsonConvert.DeserializeObject<List<Class1>>(jsonString);
            return activities;
        }

        public List<TreeNodeData> getYearTreeData(List<Class1> data)
        {
            return data
                .Where(item => item.SafeHtmlItems != null)
                .GroupBy(item => item.Time.ToLocalTime().Year)
                .OrderByDescending(g => g.Key)
                .Select(g => new TreeNodeData
                {
                    // 例: "2026"
                    date = $"{g.Key}",
                    count = g.Count(),
                    Items = g.ToList()
                }).ToList();
        }

        public List<TreeNodeData> getMonthTreeData(DateTime date, List<Class1> data)
        {
            return data
                .Where(item => item.SafeHtmlItems != null
                            && item.Time.ToLocalTime().Year == date.Year)
                .GroupBy(item => item.Time.ToLocalTime().Month)
                .OrderByDescending(g => g.Key)
                .Select(g => new TreeNodeData
                {
                    // 引数で渡された「年」を使って "2026-04" のようにする
                    date = $"{date.Year}-{g.Key:D2}",
                    count = g.Count(),
                    Items = g.ToList()
                }).ToList();
        }

        public List<TreeNodeData> getDayTreeData(DateTime date, List<Class1> data)
        {
            return data
                .Where(item => item.SafeHtmlItems != null
                            && item.Time.ToLocalTime().Year == date.Year
                            && item.Time.ToLocalTime().Month == date.Month)
                .GroupBy(item => item.Time.ToLocalTime().Day)
                .OrderByDescending(g => g.Key)
                .Select(g => new TreeNodeData
                {
                    // 引数で渡された「年」「月」を使って "2026-04-06" のようにする
                    date = $"{date.Year}-{date.Month:D2}-{g.Key:D2}",
                    count = g.Count(),
                    Items = g.ToList()
                }).ToList();
        }
    }
}
