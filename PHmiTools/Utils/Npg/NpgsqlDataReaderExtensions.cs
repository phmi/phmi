using System;
using Npgsql;

namespace PHmiTools.Utils.Npg
{
    public static class NpgsqlDataReaderExtensions
    {
        public static int? GetNullableInt32(this NpgsqlDataReader reader, int ordinal)
        {
            return reader[ordinal] as int?;
        }

        public static long? GetNullableInt64(this NpgsqlDataReader reader, int ordinal)
        {
            return reader[ordinal] as long?;
        }

        public static DateTime GetDateTimeFormTicks(this NpgsqlDataReader reader, int ordinal)
        {
            return new DateTime(reader.GetInt64(ordinal));
        }

        public static long? ToNullableTicks(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.Ticks;
            return null;
        }

        public static DateTime? GetNullableDateTimeFormTicks(this NpgsqlDataReader reader, int ordinal)
        {
            var ticks = GetNullableInt64(reader, ordinal);
            return ticks.HasValue ? new DateTime(ticks.Value) : (DateTime?)null;
        }

        public static byte[] GetByteArray(this NpgsqlDataReader reader, int ordinal)
        {
            return reader[ordinal] as byte[];
        }

        public static string GetNullableString(this NpgsqlDataReader reader, int ordinal)
        {
            return reader[ordinal] as string;
        }

        public static double? GetNullableDouble(this NpgsqlDataReader reader, int ordinal)
        {
            return reader[ordinal] as double?;
        }
    }
}
