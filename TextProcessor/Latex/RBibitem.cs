using System.Collections.Generic;
using System.Text;

namespace TextProcessor.Latex
{
    public class RBibitem
    {
        public TextBlock Block { get; set; }
        public string Key { get; set; }
        
        /// <summary>
        /// Contains RBibitem props like \by, \paper
        /// </summary>
        public Dictionary<string, string> Properties { get; }

        public bool IsBook => Properties?.ContainsKey("book") == true;
        //private static string[] _knownProps = {"by","paper","inbook","vol","issue", ""};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block">Should contain all properties</param>
        /// <param name="key"></param>
        /// <param name="props"></param>
        public RBibitem(TextBlock block, string key, Dictionary<string, string> props)
        {
            Block = block;
            Key = key;
            Properties = props;
        }

        public override string ToString()
        {
            return GenerateMarkup(Key, Properties);
        }

        public static string GenerateMarkup(string key, Dictionary<string, string> props)
        {
            var sb = new StringBuilder($"\\RBibitem{{{key}}}");
            foreach (var prop in props)
            {
                sb.AppendLine($"\r\n\\{prop.Key} {prop.Value}");
            }

            return sb.ToString();
        }
    }
}