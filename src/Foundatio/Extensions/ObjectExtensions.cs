namespace Foundatio.Utility;

public static class ObjectExtensions
{
    public static T DeepClone<T>(this T original)
    {
        return original;// DeepClonerGenerator.CloneObject(original);
    }
}
