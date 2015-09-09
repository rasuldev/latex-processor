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
        }

        public override bool Equals(object obj)
        {
            var another = obj as Cite;
            if (another == null)
                return false;
            return Keys.SequenceEqual(another.Keys) && Block.Equals(another.Block);
        }

        public override string ToString()
        {
            return GenerateMarkup(Keys);
        }

        public static string GenerateMarkup(IEnumerable<string> keys)
        {
            return @"\cite{" + String.Join(", ", keys) + "}";
        }
    }
}