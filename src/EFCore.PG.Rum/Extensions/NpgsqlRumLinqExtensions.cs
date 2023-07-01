using System;

using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlRumLinqExtensions
    {
        /// <summary>
        /// tsvector &lt;=&gt; tsquery operator
        /// rum_ts_distance(tsvector,tsquery)
        /// </summary>
        public static float Distance(this NpgsqlTsVector vector, NpgsqlTsQuery query)
        {
            throw new NotSupportedException ();
        }

        /// <summary>
        /// rum_ts_score(tsvector,tsquery)
        /// </summary>
        public static float Score(this NpgsqlTsVector vector, NpgsqlTsQuery query)
        {
            throw new NotSupportedException ();
        }
    }
}
