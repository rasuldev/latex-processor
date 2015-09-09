using System;

namespace TextProcessor.Latex
{
    public class Environment
    {
        public TextBlock OpeningBlock { get; set; }
        public TextBlock ClosingBlock { get; set; }
        public string Name { get; set; }


        public Environment(TextBlock openingBlock, TextBlock closingBlock, string name)
        {
            OpeningBlock = openingBlock;
            ClosingBlock = closingBlock;
            Name = name;
        }

        public Environment(int openStart, string openContent, int closeStart, string closeContent, string name)
        {
            OpeningBlock = new TextBlock(openStart,openContent);
            ClosingBlock = new TextBlock(closeStart, closeContent);
            Name = name;
        }

        public Environment(string text, int openStart, int openEnd, int closeStart, int closeEnd, string name)
        {
            OpeningBlock = new TextBlock(text, openStart, openEnd);
            ClosingBlock = new TextBlock(text, closeStart, closeEnd);
            Name = name;
        }

        public string GetContent()
        {
            throw new NotImplementedException();
        }

        public Label GetLabel(string text)
        {
            var pos = text.IndexOf(@"\label", OpeningBlock.EndPos, ClosingBlock.StartPos - OpeningBlock.EndPos);
            if (pos == -1)
                return null;
            var paramsInfo = Utils.HarvestParams(text, pos, 1);
            return new Label(text, pos, paramsInfo.EndPos){Name = paramsInfo.ParamsList[0]};
        }

        public override bool Equals(object obj)
        {
            var another = obj as Environment;
            if (another == null)
                return false;

            return  Name == another.Name &&
                    OpeningBlock.Equals(another.OpeningBlock) &&
                    ClosingBlock.Equals(another.ClosingBlock);
        }
    }
}