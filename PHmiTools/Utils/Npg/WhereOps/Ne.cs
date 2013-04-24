namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Ne : UnaryOp
    {
        public Ne(string column, object value) : base("<>", column, value)
        {
        }
    }
}
