using System.Diagnostics.CodeAnalysis;
using UnisonMQ.Client;

namespace UnisonMQ.ClientTests;

[TestFixture]
[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
public class CommunicationManagerTests
{
    private CommunicationManager _communicationManager = null!;

    [SetUp]
    public void InitManager()
    {
        _communicationManager = new();
    }
    
    [Test]
    public void IsWaitedSimpleTest()
    {
        var communicationManager = _communicationManager;
        var isWaited = false;

        var waitingResponseTask = new Task(() =>
        {
            var response = new Response()
            {
                Keyword = "TEST"
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            isWaited = communicationManager.Expect(cts.Token, response) == 0;
        });
        
        var receivedResponseTask = new Task(() =>
        {
            communicationManager.Received(new Response()
            {
                Keyword = "TEST"
            });
        });
        
        waitingResponseTask.Start();
        receivedResponseTask.Start();
        
        waitingResponseTask.Wait();
        receivedResponseTask.Wait();
        
        Assert.IsTrue(isWaited);
    }

    [Test]
    public void DoesNotReceivedTest()
    {
        var communicationManager = _communicationManager;
        var isWaited = false;

        var waitingResponseTask = new Task(() =>
        {
            var response = new Response()
            {
                Keyword = "TEST"
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            isWaited = communicationManager.Expect(cts.Token, response) == 0;
        });
        
        var receivedResponseTask = new Task(() =>
        {
            communicationManager.Received(new Response()
            {
                Keyword = "TEST2"
            });
        });
        
        waitingResponseTask.Start();
        receivedResponseTask.Start();
        
        waitingResponseTask.Wait();
        receivedResponseTask.Wait();
        
        Assert.IsFalse(isWaited);
    }

    [Test]
    public void ReceivedWithDelayedAndInappropriateMessageTest()
    {
        var communicationManager = _communicationManager;
        var isWaited = false;

        var waitingResponseTask = new Task(() =>
        {
            var response = new Response()
            {
                Keyword = "TEST"
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            isWaited = communicationManager.Expect(cts.Token, response) == 0;
        });
        
        var receivedResponseTask = new Task(() =>
        {
            communicationManager.Received(new Response()
            {
                Keyword = "TEST2"
            });

            Task.Delay(3000).Wait();
            
            communicationManager.Received(new Response()
            {
                Keyword = "TEST"
            });
        });
        
        waitingResponseTask.Start();
        receivedResponseTask.Start();
        
        waitingResponseTask.Wait();
        receivedResponseTask.Wait();
        
        Assert.IsTrue(isWaited);
    }

    [Test]
    public void Expect2MessagesReceived4Test()
    {
        var communicationManager = _communicationManager;
        var isWaited1 = false;
        var isWaited2 = false;
        
        var waitingResponseTask1 = new Task(() =>
        {
            var response1 = new Response()
            {
                Keyword = "TEST1"
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            isWaited1 = communicationManager.Expect(cts.Token, response1) == 0;
        });
        
        var waitingResponseTask2 = new Task(() =>
        {
            var response2 = new Response()
            {
                Keyword = "TEST2"
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            isWaited2 = communicationManager.Expect(cts.Token, response2) == 0;
        });
        
        var receivedResponseTask = new Task(() =>
        {
            communicationManager.Received(new Response()
            {
                Keyword = "TEST2"
            });

            Task.Delay(2000).Wait();
            
            communicationManager.Received(new Response()
            {
                Keyword = "TEST"
            });
            
            communicationManager.Received(new Response()
            {
                Keyword = string.Empty
            });
            
            communicationManager.Received(new Response()
            {
                Keyword = "TEST1"
            });
        });

        var tasks = new Task[] { waitingResponseTask1, waitingResponseTask2, receivedResponseTask };
        
        Array.ForEach(tasks, t => t.Start());
        Task.WaitAll(tasks);
        
        Assert.IsTrue(isWaited1);
        Assert.IsTrue(isWaited2);
    }

    [Test]
    public void Expect3SameAnswersTest()
    {
        var communicationManager = _communicationManager;
        var isWaited1 = false;
        var isWaited2 = false;
        var isWaited3 = false;
        
        CancellationTokenSource cts = new CancellationTokenSource();
        
        var response = new Response()
        {
            Keyword = "TEST"
        };

        var tasks = new Task[]
        {
            new Task(() =>
            {
                isWaited1 = communicationManager.Expect(cts.Token, response) == 0;
            }),
            new Task(() =>
            {
                isWaited2 = communicationManager.Expect(cts.Token, response) == 0;
            }),
            new Task(() =>
            {
                isWaited3 = communicationManager.Expect(cts.Token, response) == 0;
            }),
            new Task(() =>
            {
                communicationManager.Received(new Response()
                {
                    Keyword = "TEST2"
                });

                Task.Delay(1000).Wait();
            
                communicationManager.Received(new Response()
                {
                    Keyword = "TEST"
                });
            
                communicationManager.Received(new Response()
                {
                    Keyword = string.Empty
                });
            
                communicationManager.Received(new Response()
                {
                    Keyword = "TEST1"
                });
                
                communicationManager.Received(new Response()
                {
                    Keyword = "TEST"
                });
                
                Task.Delay(1000).Wait();
                
                communicationManager.Received(new Response()
                {
                    Keyword = string.Empty
                });
                
                communicationManager.Received(new Response()
                {
                    Keyword = "TEST"
                });
                
                communicationManager.Received(new Response()
                {
                    Keyword = null!
                });
            })
        };
        
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        
        Array.ForEach(tasks, t => t.Start());
        Task.WaitAll(tasks);
        
        Assert.IsTrue(isWaited1);
        Assert.IsTrue(isWaited2);
        Assert.IsTrue(isWaited3);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Expect3DifferentResponsesTest(int responseIndex)
    {
        var communicationManager = _communicationManager;
        var responses = new Response[] { Response.Ok, Response.Error, Response.Pong };
        var actualResponseIndex = -1;

        var waitingResponseTask = new Task(() =>
        {
            actualResponseIndex = communicationManager.ExpectDuring(TimeSpan.FromSeconds(5), responses);
        });
        
        var receivedResponseTask = new Task(() =>
        {
            communicationManager.Received(new Response()
            {
                Keyword = "Fake"
            });
            communicationManager.Received(responses[responseIndex]);
        });
        
        var tasks = new Task[] { waitingResponseTask, receivedResponseTask };
        
        Array.ForEach(tasks, t => t.Start());
        Task.WaitAll(tasks);
        
        Assert.That(actualResponseIndex, Is.EqualTo(responseIndex));
    }
}