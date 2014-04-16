namespace TextProcessor.Latex
{
    public class Ref
    {
        public TextBlock Block { get; set; }
        public string Value { get; set; }

        public Ref(int start, string content)
        {
            Block= new TextBlock(start,content);
            Value = Utils.HarvestParams(content, 1, 1).ParamsList[0];
        }

        public override bool Equals(object obj)
        {
            var another = obj as Ref;
            if (another == null)
                return false;
            return Value == another.Value && Block.Equals(another.Block);
        }

        public static string GenerateMarkup(string name)
        {
            return Command.GenerateMarkup("ref", name);
        } 
    }
}