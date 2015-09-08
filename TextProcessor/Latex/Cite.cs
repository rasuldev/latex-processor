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

        public Cite(TextBlock block, List<string> keys)
        {
            Block = block;
            Keys = keys;
        }

        public override bool Equals(object obj)
        {
            var another = obj as Cite;
            if (another == null)
                return false;
            return Keys.SequenceEqual(another.Keys) && Block.Equals(another.Block);
        }

        public static string GenerateMarkup(IEnumerable<string> keys)
        {
            return @"\cite{" + String.Join(",", keys) + "}";
        }
    }
}