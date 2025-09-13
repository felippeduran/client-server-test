using NUnit.Framework;
using System;
using System.Collections.Generic;

public class FakeServerHandlerTests
{
    class StubHandler<TConnState, TArgs, TRes>
    {
        public Func<TConnState, TArgs, (TRes, Error)> Handler;

        [EndpointHandler]
        public (TRes, Error) TestMethod(TConnState connState, TArgs arg)
        {
            return Handler(connState, arg);
        }
    }

    class MockHandler<TArgs, TRes>
    {
        public TArgs ExpectedArgs;
        public TRes ReturnResult;
        public Error ReturnError;

        [EndpointHandler]
        public (TRes, Error) TestMethod(TArgs arg)
        {
            Assert.AreEqual(ExpectedArgs, arg);
            return (ReturnResult, ReturnError);
        }
    }

    class TestHandler : StubHandler<object, int, (int, string)> { }

    [Test]
    public void TestHandleMessage_OnSuccess_ShouldReturnResult()
    {
        var args = 5;
        var connState = new Dictionary<string, int>();
        var expectedResult = (10, "test");
        var mockHandler = new TestHandler
        {
            Handler = (connState, arg) => (expectedResult, null),
        };

        var handler = new FakeServerHandler<object>(new object[] { mockHandler });
        var (result, error) = handler.HandleMessage<int, (int, string)>(connState, "TestHandler/TestMethod", args);

        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That(error, Is.EqualTo(null));
    }

    [Test]
    public void TestHandleMessage_OnFailure_ShouldReturnError()
    {
        var args = 5;
        var connState = new Dictionary<string, int>();
        var expectedError = new Error { Message = "test" };
        var mockHandler = new TestHandler
        {
            Handler = (connState, arg) => (default((int, string)), expectedError),
        };

        var handler = new FakeServerHandler<object>(new object[] { mockHandler });
        var (result, error) = handler.HandleMessage<int, (int, string)>(connState, "TestHandler/TestMethod", args);

        Assert.That(error, Is.EqualTo(expectedError));
        Assert.That(result, Is.EqualTo(default((int, string))));
    }

    [Test]
    public void TestHandleMessage_OnInvalidType_ShouldReturnError()
    {
        var args = "1";
        var expectedError = new Error { Message = "invalid args type" };
        var connState = new Dictionary<string, int>();
        var mockHandler = new TestHandler
        {
            Handler = (connState, arg) => (default, null),
        };

        var handler = new FakeServerHandler<object>(new object[] { mockHandler });
        var (result, error) = handler.HandleMessage<string, int>(connState, "TestHandler/TestMethod", args);
        Assert.That(error, Is.EqualTo(expectedError));
        Assert.That(result, Is.EqualTo(default(int)));
    }

    [Test]
    public void TestInvalidEndpoint()
    {
        var args = 1;
        var expectedError = new Error { Message = "handler not found" };
        var connState = new Dictionary<string, int>();
        var mockHandler = new TestHandler
        {
            Handler = (connState, arg) => (default, null),
        };

        var handler = new FakeServerHandler<object>(new object[] { mockHandler });
        var (result, error) = handler.HandleMessage<int, int>(connState, "TestMethod2", args);
        Assert.That(error, Is.EqualTo(expectedError));
        Assert.That(result, Is.EqualTo(default(int)));
    }
}