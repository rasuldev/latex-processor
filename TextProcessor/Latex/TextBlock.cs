namespace TextProcessor.Latex
{
    public class TextBlock
    {
        public int StartPos { get; set; }

        public int EndPos => StartPos + Content.Length - 1;

        public int Length => EndPos - StartPos + 1;
        public string Content { get; set; }


        public TextBlock()
        {

        }

        public TextBlock(string text, int start, int end)
        {
            StartPos = start;
            Content = text.Substring(start, end - start + 1);
        }

        public TextBlock(int start, string content)
        {
            StartPos = start;
            Content = content;
            // TODO: check content.Length == Length
        }

        public override bool Equals(object obj)
        {
            var another = obj as TextBlock;
            if (another == null)
                return false;
            return StartPos == another.StartPos &&
                   Content == another.Content;

        }

        public override string ToString()
        {
            return Content;
        }
    }
}