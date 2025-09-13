public class FakeServerFactory
{
    public static IFakeServer CreateServer<TConnState>(object[] handlers) where TConnState : new()
    {
        var serverHandler = new FakeServerHandler<TConnState>(handlers);
        return new FakeServer<TConnState>(serverHandler);
    }
}