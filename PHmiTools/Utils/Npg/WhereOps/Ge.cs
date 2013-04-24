namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Ge : UnaryOp
    {
        public Ge(string column, object value)
            : base(">=", column, value)
        {
        }
    }
}
