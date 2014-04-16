namespace TextProcessor.Latex
{
    public class Label
    {
        public TextBlock Block { get; set; }
        public string Name { get; set; }
        public string EnvironmentName { get; set; }

        public Label(int start, string content)
        {
            Block = new TextBlock(start, content);
        }

        public Label(string text, int start, int end)
        {
            Block = new TextBlock(text, start, end);
        }

        public static string GenerateMarkup(string name)
        {
            return @"\label{" + name + "}";
        }

        public override bool Equals(object obj)
        {
            var another = obj as Label;
            if (another == null)
                return false;
            return this.Name == another.Name &&
                   this.EnvironmentName == another.EnvironmentName &&
                   this.Block.Equals(another.Block);
        }
    }
}