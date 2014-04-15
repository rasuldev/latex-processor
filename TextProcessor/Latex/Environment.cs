using System;

namespace TextProcessor.Latex
{
    public class Environment
    {
        public TextBlock OpeningBlock { get; set; }
        public TextBlock ClosingBlock { get; set; }
        public string Name { get; set; }

        public string GetContent()
        {
            throw new NotImplementedException();
        }
    }
}