using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.Common;

namespace TSO_E_Cityserver.Database
{
    public static class Extensions
    {
        public static IEnumerable<T> Select<T>(
            this DbDataReader reader, Func<DbDataReader, T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }
    }
}
