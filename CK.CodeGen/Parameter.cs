namespace CK.CodeGen
{
    public class Parameter
    {
        public List<string> Attributes { get; } = new List<string>();

        public string Name { get; set; }

        public string Type { get; set; }

        public string DefaultValue { get; set; }


    }
}