namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Lt : UnaryOp
    {
        public Lt(string column, object value)
            : base("<", column, value)
        {
        }
    }
}