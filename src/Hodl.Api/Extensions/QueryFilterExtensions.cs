using System.Linq.Expressions;
using System.Reflection;

namespace Hodl.Api.Extensions;

/*
 * Generic filter for Linq
 * ref: 
 * https://stackoverflow.com/questions/41251331/linq-filter-query-with-multiple-optional-parameters
 * 
 * Example:
 * public class Person
 * {
 *     public string Name { get; }
 *     public int Age { get; }
 * 
 *     public Person( string name, int age )
 *     {
 *         Name = name;
 *         Age = age;
 *     }
 * }
 * 
 * public class PersonFilterParams : IFilterParams<Person>
 * {
 *     [Filter( FilterType.Equals )]
 *     public string Name { get; set; }
 * 
 *     [Filter( nameof( Person.Age ), FilterType.GreaterOrEquals )]
 *     public int? MinAge { get; set; }
 * 
 *     [Filter( nameof( Person.Age ), FilterType.LessOrEquals )]
 *     public int? MaxAge { get; set; }
 * 
 *     [Filter( nameof( NonExistingProp ), FilterType.LessOrEquals )]
 *     public int? NonExistingProp { get; set; }
 * }
 * 
 * // you can skip properies here
 * var filter = new PersonFilterParams
 * {
 *     //Name = "Name 4",
 *     //MinAge = 2,
 *     MaxAge = 20,
 *     NonExistingProp = 20,
 * };
 * 
 * var filteredPersons = persons
 *     .Filter( filter )
 *     .ToList();
 *     
 */

public enum FilterType
{
    Less,
    LessOrEquals,
    Equals,
    Greater,
    GreaterOrEquals,
    Contains,
    StartsWith,
    InCollection,
    NotInCollection
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class FilterAttribute : Attribute
{
    public string PropName { get; }

    public FilterType FilterType { get; }

    public FilterAttribute() : this(null, FilterType.Equals)
    {
    }

    public FilterAttribute(FilterType filterType) : this(null, filterType)
    {
    }

    public FilterAttribute(string propName, FilterType filterType)
    {
        PropName = propName;
        FilterType = filterType;
    }
}

public interface IFilterParams<T>
{
}

public static class QueryFilterExtensions
{
    public static IQueryable<T> Filter<T>(this IQueryable<T> source, IFilterParams<T> filterParams)
    {
        var sourceProps = typeof(T).GetProperties();
        var filterProps = filterParams.GetType().GetProperties();

        foreach (var prop in filterProps)
        {
            var filterAttr = prop.GetCustomAttribute<FilterAttribute>();

            if (filterAttr == null)
                continue;

            object val = prop.GetValue(filterParams);

            if (val == null)
                continue;

            // oops.. little hole..
            if (prop.PropertyType == typeof(string) && (string)val == string.Empty)
                continue;

            string propName = string.IsNullOrEmpty(filterAttr.PropName)
                ? prop.Name
                : filterAttr.PropName;

            if (!sourceProps.Any(x => x.Name == propName))
                continue;

            var filter = CreateFilter<T>(propName, filterAttr.FilterType, val);

            if (filter != null)
                source = source.Where(filter);
        }

        return source;
    }

    private static Expression ToStringExpr(Expression val)
    {
        // If the value is not a string, get it as string
        if (val.Type != typeof(string))
        {
            var method = val.Type.GetMethod("ToString", Array.Empty<Type>());

            return Expression.Call(val, method);
        }

        return val;
    }

    private static Expression ToLowerExpr(Expression val)
    {
        var method = typeof(string).GetMethod("ToLower", Array.Empty<Type>());

        return Expression.Call(ToStringExpr(val), method);
    }

    /// <summary>
    /// Compares the lower case versions of two values represented as string 
    /// and test if the val can be found inside the prop value.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    private static Expression ContainsExpr(Expression prop, Expression val)
    {
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var lowerProp = ToLowerExpr(prop);
        var lowerVal = ToLowerExpr(val);

        return Expression.Call(lowerProp, method, lowerVal);
    }

    /// <summary>
    /// Compares the lower case versions of two values represented as string
    /// and test if the prop value starts with val.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    private static Expression StartsWithExpr(Expression prop, Expression val)
    {
        var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var lowerProp = ToLowerExpr(prop);
        var lowerVal = ToLowerExpr(val);

        return Expression.Call(lowerProp, method, lowerVal);
    }

    /// <summary>
    /// Compares the lower case versions of two values represented as string 
    /// and test if the val can be found inside the prop value.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    private static Expression InCollectionExpr(Expression prop, Expression val)
    {
        var method = val.Type.GetMethod("Contains", new[] { prop.Type });

        return Expression.Call(val, method, prop);
    }

    private static Expression GetExpression(string propName, FilterType filterType, Expression prop, Expression val)
        => filterType switch
        {
            FilterType.LessOrEquals => Expression.LessThanOrEqual(prop, val),
            FilterType.Less => Expression.LessThan(prop, val),
            FilterType.Equals => Expression.Equal(prop, val),
            FilterType.Greater => Expression.GreaterThan(prop, val),
            FilterType.GreaterOrEquals => Expression.GreaterThanOrEqual(prop, val),
            FilterType.Contains => ContainsExpr(prop, val),
            FilterType.StartsWith => StartsWithExpr(prop, val),
            FilterType.InCollection => InCollectionExpr(prop, val),
            FilterType.NotInCollection => Expression.Not(InCollectionExpr(prop, val)),
            _ => throw new Exception($"Unknown FilterType '{filterType}' on property '{propName}'!")
        };

    private static Expression<Func<T, bool>> CreateFilter<T>(string propName, FilterType filterType, object propValue)
    {
        var item = Expression.Parameter(typeof(T), "x");
        var prop = Expression.Property(item, propName);
        var val = Expression.Constant(propValue);
        var expr = GetExpression(propName, filterType, prop, val);

        return Expression.Lambda<Func<T, bool>>(expr, item);
    }
}
