using System;

namespace TextProcessor.Latex
{
    public class Environment
    {
        public TextBlock OpeningBlock { get; set; }
        public TextBlock ClosingBlock { get; set; }
        public string Name { get; set; }

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