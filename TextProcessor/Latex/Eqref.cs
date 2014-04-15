namespace TextProcessor.Latex
{
    public class Eqref
    {
        public TextBlock Block { get; set; }
        public string Value { get; set; }

        public Eqref(int start, string content)
        {
            Block= new TextBlock(start,content);
            Value = Utils.HarvestParams(content, 1, 1).ParamsList[0];
        }

        public override bool Equals(object obj)
        {
            var another = obj as Eqref;
            if (another == null)
                return false;
            return Value == another.Value && Block.Equals(another.Block);
        }

        public static string GenerateMarkup(string name)
        {
            return @"\eqref{" + name + "}";
        }
    }
}