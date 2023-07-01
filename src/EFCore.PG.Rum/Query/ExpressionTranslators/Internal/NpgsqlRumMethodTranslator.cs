using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlRumMethodTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<MethodInfo, string> Functions = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlRumLinqExtensions.Score), new[] { typeof(NpgsqlTsVector), typeof(NpgsqlTsQuery) })] = "rum_ts_score"
        };

        static readonly Dictionary<MethodInfo, string> DoubleReturningOperators = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlRumLinqExtensions.Distance), new[] { typeof(NpgsqlTsVector), typeof(NpgsqlTsQuery) })] = "<=>"
        };

        static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
            => typeof(NpgsqlRumLinqExtensions).GetRuntimeMethod(name, parameters);

        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _doubleMapping;

        public NpgsqlRumMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _doubleMapping = typeMappingSource.FindMapping(typeof(double));
        }


        /// <inheritdoc />
#pragma warning disable EF1001
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (Functions.TryGetValue(method, out var function))
                return _sqlExpressionFactory.Function(function, "", arguments, false, new []{false, false}, method.ReturnType);

            if (DoubleReturningOperators.TryGetValue(method, out var floatOperator))
                return new PostgresUnknownBinaryExpression(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    floatOperator,
                    _doubleMapping.ClrType,
                    _doubleMapping);

            return null;
        }
#pragma warning restore EF1001
    }
}
