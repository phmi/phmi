namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Or : NaryOp
    {
        public Or(params IWhereOp[] parameters) : base("or", parameters)
        {
        }
    }
}
