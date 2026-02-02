using System.Linq.Expressions;

namespace AP.Common.Utilities.Helpers;

public interface IOrderBy
{
    dynamic Expression { get; }
}

public class OrderBy<T, TResult>(Expression<Func<T, TResult>> expression) : IOrderBy
{
    private readonly Expression<Func<T, TResult>> expression = expression;

    public dynamic Expression => expression;
}