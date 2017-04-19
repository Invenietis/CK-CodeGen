namespace CK.CodeGen
{
    public abstract class PropertyBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Type { get; set; }

        public string Name { get; set; }


    }
}