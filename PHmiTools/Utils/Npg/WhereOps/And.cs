namespace PHmiTools.Utils.Npg.WhereOps
{
    public class And : NaryOp
    {
        public And(params IWhereOp[] parameters) : base("and", parameters)
        {
        }
    }
}
