namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Eq : UnaryOp
    {
        public Eq(string column, object value)
            : base("=", column, value)
        {
        }
    }
}
