
namespace PHmiResources
{
    public class RegexPatterns
    {
        public const string VariableName = @"^[а-яА-Яa-zA-Z_][\w]*$";
        public const string NameSpace = @"^[а-яА-Яa-zA-Z_][\w.]*$";
        public const string Server = @"^[\w_.-]+:[\d]+$";
    }
}
