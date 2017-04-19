namespace CK.CodeGen
{
    public class FieldBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public string Name { get; set; }

        public string Type { get; set; }

        public string InitialValue { get; set; }
    }
}