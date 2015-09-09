namespace TextProcessor.Latex
{
    public class Bibitem
    {
        public TextBlock Block { get; set; }
        public string Key { get; set; }
        public string FullTitle { get; set; }

        public Bibitem(TextBlock block, string key, string fullTitle)
        {
            Block = block;
            Key = key;
            FullTitle = fullTitle;
        }

        public override string ToString()
        {
            return GenerateMarkup(Key, FullTitle);
        }

        public static string GenerateMarkup(string key, string fullTitle)
        {
            return $"\\bibitem{{{key}}}\r\n{fullTitle}";
        }
    }
}