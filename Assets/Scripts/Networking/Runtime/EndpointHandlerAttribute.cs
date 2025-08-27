using System;
using System.Reflection;

public class EndpointHandlerAttribute : Attribute
{
    public string MethodName;

    public EndpointHandlerAttribute(string methodName = null)
    {
        this.MethodName = methodName;
    }

    public string ResolveMethodName(MethodInfo method)
    {
        return string.IsNullOrEmpty(MethodName) ? method.Name : MethodName;
    }
}