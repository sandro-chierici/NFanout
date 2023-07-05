// MIT License
// 
// Copyright (c) 2023 Sandro Chierici <sandro.chierici@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NFanout.Adapters;
using NFanout.Core;
using System.Threading.Tasks.Dataflow;

namespace NFanout.Devices;

public class QueueManager
{
    private readonly NFanoutConfiguration _configuration;
    private readonly ILogger<QueueManager> _logger;
    private Dictionary<string, ActionBlock<MessageDTO>> _queues;
    private ActionBlock<MessageDTO> _input;
    private readonly IWorker _worker;

    public QueueManager(NFanoutConfiguration configuration,
        ILogger<QueueManager> logger,
        IServiceProvider servProvider)
    {
        _configuration = configuration;
        _logger = logger;

        //
        // enqueue messages
        //
        _input = new ActionBlock<MessageDTO>(Enroute, 
            new ExecutionDataflowBlockOptions 
            { 
                EnsureOrdered = true, 
                MaxDegreeOfParallelism = 1
            });

        //
        // Running Queues
        //
        _queues = new Dictionary<string, ActionBlock<MessageDTO>>(StringComparer.InvariantCultureIgnoreCase);

        //
        // if there isn't any implementation for worker we provide default one
        //
        _worker = servProvider!.GetService<IWorker>() ?? servProvider.GetRequiredService<DefaultWorker>();
    }

    /// <summary>
    /// Linking entry
    /// </summary>
    public ITargetBlock<MessageDTO> InputQueue => _input;

    /// <summary>
    /// Add queue by key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private ActionBlock<MessageDTO> AddQueue(string key)
    {
        _logger.LogInformation($"Add queue for key {key}");

        _queues[key] = new ActionBlock<MessageDTO>(
            DoRunMessage,
            new ExecutionDataflowBlockOptions 
            { 
                EnsureOrdered = true, 
                // every single queue elaboration is single thread
                MaxDegreeOfParallelism = 1 
            });

        return _queues[key];
    }

    /// <summary>
    /// Message execution by worker
    /// </summary>
    /// <param name="message"></param>
    private void DoRunMessage(MessageDTO message)
    {
        try
        {
            _worker.DoWork(new WorkDataRecord(message.QueueKey!, message.Payload, message.StartUtc));

            // add overall duration 
            message.DurationMillis = DateTime.UtcNow.Ticks - message.StartUtc;

            _logger.LogInformation($"Queue: {message.QueueKey}, message {message.Payload}, duration millis {message.DurationMillis}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling worker with data: QueueKey {message.QueueKey}. {ex.Message}");
        }
    }

    /// <summary>
    /// Enroute a message in right queue
    /// </summary>
    /// <param name="message"></param>
    public void Enroute(MessageDTO message)
    {
        _queues.TryGetValue(message.QueueKey!, out var queue);

        if (queue != null)
        {
            queue.Post(message);
            return;
        }

        AddQueue(message.QueueKey!).Post(message);
    }
}
