namespace PHmiClient.Tags
{
    public interface INumericTag : ITag<double?>
    {
        double? MinValue { get; }
        double? MaxValue { get; }
        string ValueString { get; }
    }
}
