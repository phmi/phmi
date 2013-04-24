namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Le : UnaryOp
    {
        public Le(string column, object value)
            : base("<=", column, value)
        {
        }
    }
}
