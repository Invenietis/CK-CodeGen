namespace CK.CodeGen
{
    public class ConstructorBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public string Initializer { get; set; }

        public StringBuilder Body { get; } = new StringBuilder();

    }
}