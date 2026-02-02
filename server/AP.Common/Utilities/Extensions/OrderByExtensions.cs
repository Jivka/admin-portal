using AP.Common.Utilities.Helpers;

namespace AP.Common.Utilities.Extensions;

public static class OrderByExtensions
{
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.OrderBy(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.OrderByDescending(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.ThenBy(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.ThenByDescending(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, Dictionary<string, IOrderBy> orders, Dictionary<string, string>? sort)
    {
        var first = sort?.FirstOrDefault();

        if (first != null && first?.Key != null && orders.ContainsKey(first?.Key!))
        {
            var orderedSource = first?.Value == "desc" ? source.OrderByDescending(orders[first?.Key!]) : source.OrderBy(orders[first?.Key!]);
            sort?.Skip(1).ForEach(next => ApplyThenBy(orderedSource, orders, next));

            return orderedSource;
        }

        return source.OrderByDescending(orders["default"]);
    }

    public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, Dictionary<string, List<IOrderBy>> orders, Dictionary<string, string>? sort)
    {
        var first = sort?.FirstOrDefault();

        if (first != null && first?.Key != null && orders.ContainsKey(first?.Key!))
        {
            var firstOrder = orders[first?.Key!];
            var orderedSource = first?.Value == "desc" ? source.OrderByDescending(firstOrder?.FirstOrDefault() !) : source.OrderBy(firstOrder?.FirstOrDefault() !);

            foreach (var order in firstOrder?.Skip(1) !)
            {
                orderedSource = first?.Value == "desc" ? orderedSource.ThenByDescending(order) : orderedSource.ThenBy(order);
            }

            foreach (var next in sort?.Skip(1) !)
            {
                ApplyThenBy(orderedSource, orders, next);
            }

            return orderedSource;
        }

        return source.OrderByDescending(orders["default"].FirstOrDefault() !);
    }

    private static void ApplyThenBy<T>(IOrderedQueryable<T> orderedSource, Dictionary<string, IOrderBy> orders, KeyValuePair<string, string>? next)
    {
        if (next != null && next?.Key != null && orders.ContainsKey(next?.Key!))
        {
            _ = next?.Value == "desc" ? orderedSource.ThenByDescending(orders[next?.Key!]) : orderedSource.ThenBy(orders[next?.Key!]);
        }
    }

    private static void ApplyThenBy<T>(IOrderedQueryable<T> orderedSource, Dictionary<string, List<IOrderBy>> orders, KeyValuePair<string, string>? next)
    {
        if (next != null && next?.Key != null && orders.ContainsKey(next?.Key!))
        {
            foreach (var order in orders[next?.Key!])
            {
                _ = next?.Value == "desc" ? orderedSource.ThenByDescending(order) : orderedSource.ThenBy(order);
            }
        }
    }
}