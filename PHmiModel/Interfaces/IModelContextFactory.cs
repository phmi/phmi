
namespace PHmiModel.Interfaces
{
    public interface IModelContextFactory
    {
        IModelContext Create(string connectionString, bool startTrackingChanges);
    }
}
