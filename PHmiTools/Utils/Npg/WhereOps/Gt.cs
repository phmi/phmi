namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Gt : UnaryOp
    {
        public Gt(string column, object value)
            : base(">", column, value)
        {
        }
    }
}
