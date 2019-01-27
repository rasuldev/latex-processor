using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace TextProcessor.Latex
{
    public class Cite
    {
        public TextBlock Block { get; set; }
        public List<string> Keys { get; set; }
        public string OptionalParam { get; set; }

        public Cite(TextBlock block, IEnumerable<string> keys)
        {
            Block = block;
            Keys = keys.ToList();
        }

        public Cite(int start, string content)
        {
            Block = new TextBlock(start, content);
            var keysRaw = Utils.HarvestParams(content, 1, 1).ParamsList[0];
            Keys = keysRaw.Split(',').Select(k => k.Trim()).ToList();
            // check for optional param
            if (content.Contains("["))
            {
                OptionalParam = content.Split(new[] {'[', ']'})[1];
            }
        }

        public override bool Equals(object obj)
        {
            var another = obj as Cite;
            if (another == null)
                return false;
            return Keys.SequenceEqual(another.Keys) && Block.Equals(another.Block) && another.OptionalParam == OptionalParam;
        }

        public override string ToString()
        {
            return GenerateMarkup(Keys, OptionalParam);
        }

        public static string GenerateMarkup(IEnumerable<string> keys, string optionalParam = null)
        {
            var optWithBrackets = string.IsNullOrEmpty(optionalParam) ? "" : $"[{optionalParam}]";
            return $@"\cite{optWithBrackets}{{" + String.Join(", ", keys) + "}";
        }
    }
}