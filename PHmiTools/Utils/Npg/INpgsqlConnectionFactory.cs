using Npgsql;

namespace PHmiTools.Utils.Npg
{
    public interface INpgsqlConnectionFactory
    {
        NpgsqlConnection Create();
    }
}
