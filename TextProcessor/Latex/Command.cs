using System;
using System.Linq;

namespace TextProcessor.Latex
{
    public class Command
    {
        public TextBlock Block { get; set; }
        public string[] Params { get; set; }

        public Command(int start, string content, int paramsCount)
        {
            Block= new TextBlock(start,content);
            Params = Utils.HarvestParams(content, 1, paramsCount).ParamsList.ToArray();
        }

        public override bool Equals(object obj)
        {
            var another = obj as Command;
            if (another == null)
                return false;
            
            return Params.SequenceEqual(another.Params) && Block.Equals(another.Block);
        }

        public static string GenerateMarkup(string commandName, string paramValue)
        {
            return $"\\{commandName}{{{paramValue}}}";
        } 
    }
}