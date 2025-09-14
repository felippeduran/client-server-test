using System.Collections.Generic;

namespace Networking.Runtime.Fake
{
    public interface IFakeServer
    {
        int CreateConnection();
        void RemoveConnection(int connectionId);
        (TRes, Error) ReceiveMessage<TArgs, TRes>(int connectionId, string message, TArgs args);
        Error ReceiveMessage<TArgs>(int connectionId, string message, TArgs args);
    }

    public class FakeServer<TConnState> : IFakeServer where TConnState : new()
    {
        int nextConnectionId = 0;
        readonly IDictionary<int, TConnState> connections = new Dictionary<int, TConnState>();

        readonly IFakeServerHandler<TConnState> handler;

        public FakeServer(IFakeServerHandler<TConnState> handler)
        {
            this.handler = handler;
        }

        public int CreateConnection()
        {
            var connectionId = nextConnectionId++;
            connections.Add(connectionId, new TConnState());
            return connectionId;
        }

        public void RemoveConnection(int connectionId)
        {
            connections.Remove(connectionId);
        }

        public (TRes, Error) ReceiveMessage<TArgs, TRes>(int connectionId, string message, TArgs args)
        {
            if (!connections.TryGetValue(connectionId, out var connState))
            {
                return (default(TRes), new Error { Message = "connection not found" });
            }

            return handler.HandleMessage<TArgs, TRes>(connState, message, args);
        }

        public Error ReceiveMessage<TArgs>(int connectionId, string message, TArgs args)
        {
            if (!connections.TryGetValue(connectionId, out var connState))
            {
                return new Error { Message = "connection not found" };
            }

            return handler.HandleMessage(connState, message, args);
        }
    }
}