// using System;

// public class AuthenticatedMiddleware<TArgs, TRes>
// {
//     readonly Func<ConnectionState, string, TArgs, (TRes, Error)> Handler;

//     public AuthenticatedMiddleware(Func<ConnectionState, string, TArgs, (TRes, Error)> handler)
//     {
//         Handler = handler;
//     }

//     public (TRes, Error) Handle(ConnectionState connState, string message, TArgs args)
//     {
//         if (connState.AccountId == null)
//         {
//             return (default, new Error { Message = "connection not authenticated" });
//         }

//         return Handler(connState, message, args);
//     }
// }

// public class CommandMiddleware<TArgs, TRes>
// {
//     readonly IAccountStorage accountStorage;
//     readonly Func<PlayerState, LevelConfig[], TArgs, (TRes, Error)> Handler;

//     public (TRes, Error) Handle(ConnectionState connState, string message, TArgs args)
//     {
//         var (state, error) = accountStorage.GetPlayerState(connState.AccountId);
//         if (error != null)
//         {
//             return (default, error);
//         }

//         var levelConfig = ConfigsProvider.GetHardcodedConfigs();

//         return Handler(state, levelConfig, args);
//     }
// }

// public class B
// {
//     public static void A()
//     {
//         var handlers = new Handlers();
//         var authenticationHandlers = new AuthenticationHandlers();

//         ConnectionState connectionState = default;
//         Handlers.BeginLevelArgs args = default;

//         // var decorated = new AuthenticatedMiddleware<BeginLevelArgs, BeginLevelRes>((ConnectionState connState, string message, BeginLevelArgs args) =>
//         // {
//         //     return handlers.BeginLevel(connState, args);
//         // });

//         new Func<ConnectionState, TArgs, > []
//         (connState, args) =>
//         {
//             return authenticationHandlers.Authenticate(connState, args);
//         };
//     }
// }

// public class IdentityDecorator
// {
//     public Func<ConnectionState, TArgs, (TRes, Error)> Decorate<TArgs, TRes>(Func<ConnectionState, TArgs, (TRes, Error)> func)
//     {
//         return func;
//     }
// }

// public static class AuthDecorator
// {
//     public static Func<ConnectionState, TArgs, (TRes, Error)> Decorate<TArgs, TRes>(Func<ConnectionState, TArgs, (TRes, Error)> func)
//     {
//         return (ConnectionState connState, TArgs args) =>
//         {
//             if (connState.AccountId == null)
//             {
//                 return (default, new Error { Message = "connection not authenticated" });
//             }

//             return func(connState, args);
//         };
//     }
// }

// public class CommandsDecorator
// {
//     readonly IAccountStorage accountStorage;

//     public CommandsDecorator(IAccountStorage accountStorage)
//     {
//         this.accountStorage = accountStorage;
//     }

//     public Func<ConnectionState, TArgs, (TRes, Error)> Decorate<TArgs, TRes>(Func<ConnectionState, TArgs, (TRes, Error)> func)
//     {
//         return (ConnectionState connState, TArgs args) =>
//         {
//             var (state, error) = accountStorage.GetPlayerState(connState.AccountId);
//             if (error != null)
//             {
//                 return (default, error);
//             }

//             var levelConfig = ConfigsProvider.GetHardcodedConfigs();

//             return func(connState, args);
//         };
//     }
// }