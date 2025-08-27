using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;

public interface IHandler<TConnState>
{
    (TRes, Error) Handle<TArgs, TRes>(TConnState connState, string message,TArgs args);
}

// public class FakeServerHandler2<TConnState> : IFakeServerHandler<TConnState>
// {
//     readonly Dictionary<string, (MethodInfo, object)> handlersByEndpoint = new();

//     public FakeServerHandler2(object[] handlers)
//     {
//         foreach (var handler in handlers)
//         {
//             var handlerType = handler.GetType();
//             var methods = handlerType.GetMethods().Where(m => m.GetCustomAttribute<EndpointHandlerAttribute>() != null);
//             foreach (var method in methods)
//             {
//                 if (method.GetParameters().Length != 2)
//                 {
//                     throw new Exception($"Method {method.Name} must have exactly two parameters");
//                 }

//                 var connStateType = method.GetParameters()[0].ParameterType;
//                 if (!connStateType.IsAssignableFrom(typeof(TConnState)))
//                 {
//                     throw new Exception($"Method {method.Name} must have a parameter of type {typeof(TConnState)}");
//                 }

//                 var attribute = method.GetCustomAttribute<EndpointHandlerAttribute>();
//                 var methodName = attribute.ResolveMethodName(method);

//                 handlersByEndpoint[$"{handlerType.Name}/{methodName}"] = (method, handler);
//             }
//         }
//     }

//     public (TResult, Error) HandleMessage<TArgs, TResult>(TConnState connState, string message, TArgs args)
//     {
//         if (!handlersByEndpoint.TryGetValue(message, out var handlerData))
//         {
//             return (default, new Error { Message = "handler not found" }); ;
//         }

//         var validResType = handlerData.Item1.ReturnParameter.ParameterType.IsAssignableFrom(typeof((TResult, Error)));
//         var validArgType = handlerData.Item1.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TArgs));
//         if (!validResType || !validArgType)
//         {
//             return (default, new Error { Message = "invalid type" });
//         }

//         var result = handlerData.Item1.Invoke(handlerData.Item2, new object[] { connState, args });

//         Debug.Log($"Received message: {message}");
//         return ((TResult, Error))result;
//     }
// }


public class FakeServerHandler<TConnState> : IFakeServerHandler<TConnState>
{
    readonly Dictionary<string, (MethodInfo, object)> handlersByEndpoint = new();

    public FakeServerHandler(object[] handlers)
    {
        foreach (var handler in handlers)
        {
            var handlerType = handler.GetType();
            var methods = handlerType.GetMethods().Where(m => m.GetCustomAttribute<EndpointHandlerAttribute>() != null);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length != 2)
                {
                    throw new Exception($"Method {method.Name} must have exactly two parameters");
                }

                var connStateType = method.GetParameters()[0].ParameterType;
                if (!connStateType.IsAssignableFrom(typeof(TConnState)))
                {
                    throw new Exception($"Method {method.Name} must have a parameter of type {typeof(TConnState)}");
                }

                var attribute = method.GetCustomAttribute<EndpointHandlerAttribute>();
                var methodName = attribute.ResolveMethodName(method);

                handlersByEndpoint[$"{handlerType.Name}/{methodName}"] = (method, handler);
            }
        }
    }

    public (TResult, Error) HandleMessage<TArgs, TResult>(TConnState connState, string message, TArgs args)
    {
        if (!handlersByEndpoint.TryGetValue(message, out var handlerData))
        {
            return (default, new Error { Message = "handler not found" });
        }

        var validConnStateType = handlerData.Item1.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TConnState));
        if (!validConnStateType)
        {
            return (default, new Error { Message = "invalid connection state type" });
        }

        var validArgType = handlerData.Item1.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(TArgs));
        if (!validArgType)
        {
            return (default, new Error { Message = "invalid args type" });
        }

        return ((TResult, Error))handlerData.Item1.Invoke(handlerData.Item2, new object[] { connState, args });
    }

    public Error HandleMessage<TArgs>(TConnState connState, string message, TArgs args)
    {
        if (!handlersByEndpoint.TryGetValue(message, out var handlerData))
        {
            return new Error { Message = "handler not found" };
        }

        var validArgType = handlerData.Item1.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TArgs));
        if (!validArgType)
        {
            return new Error { Message = "invalid type" };
        }

        return (Error)handlerData.Item1.Invoke(handlerData.Item2, new object[] { connState, args });
    }
}