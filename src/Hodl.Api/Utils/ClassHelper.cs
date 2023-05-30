using System.Reflection;

namespace Hodl.Utils;

public static class ClassHelper
{
    public static IEnumerable<Type> GetLastDescendants(Type type)
    {
        if (!type.IsClass)
            throw new Exception(type + " is not a class");

        var alltypes = Assembly.GetExecutingAssembly().GetTypes();
        var subTypes = alltypes.Where(x => x.IsSubclassOf(type)).ToArray();

        return subTypes.Where(x => subTypes.All(y => y.BaseType != x));
    }
}
