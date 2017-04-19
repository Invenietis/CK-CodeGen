namespace CK.CodeGen
{
    public class GenericConstraint
    {
        public string GenericParameterName { get; set; }

        public List<string> Constraints { get; } = new List<string>();

    }
}