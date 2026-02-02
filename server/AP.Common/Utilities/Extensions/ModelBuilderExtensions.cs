using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace AP.Common.Utilities.Extensions;

public static class Json
{
    public static string Value(
        string expression,
        string path)
        => throw new InvalidOperationException($"{nameof(Value)} cannot be called client side");
}

public static class ModelBuilderExtensions
{
    public static ModelBuilder UseCustomDbFunctions(this ModelBuilder builder)
    {
        // JSON_VALUE as a db function to retrieve a property value from a JSON column
        var jsonvalueMethodInfo = typeof(Json)
            .GetRuntimeMethod(
                nameof(Json.Value),
                [typeof(string), typeof(string)]);
        builder
            .HasDbFunction(jsonvalueMethodInfo!)
            .HasTranslation(args =>
                new SqlFunctionExpression("JSON_VALUE", args, true, new List<bool> { true }, typeof(string), null));

        return builder;
    }
}