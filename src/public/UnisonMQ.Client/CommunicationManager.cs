using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UnisonMQ.Client;

internal sealed class CommunicationManager
{
    private readonly List<Response> _expectedResponses = new();
    private readonly object _syncRoot = new();

    public int Expect(CancellationToken cancellationToken, params Response[] responses)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            lock (_syncRoot)
            {
                var expectedResponse = _expectedResponses.FirstOrDefault(
                    r => responses.Any(expectedR => expectedR.Keyword == r.Keyword));
                
                if (expectedResponse != default)
                {
                    var removed = _expectedResponses.Remove(expectedResponse);
                    
                    return removed ?
                        Array.IndexOf(responses, expectedResponse) : 
                        -1;
                }
            }
        }

        return -1;
    }

    public int ExpectDuring(TimeSpan timeout, params Response[] responses)
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(timeout);
        
        return Expect(cts.Token, responses);
    }

    public void Received(Response response)
    {
        lock (_syncRoot)
        {
            _expectedResponses.Add(response);
        }
    }
}