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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NFanout.Adapters;
using NFanout.Core;
using NFanout.Devices.RoutingAlgos;
using System.Threading.Tasks.Dataflow;

namespace NFanout.Devices;

public class PipelineManager : IDataInput<object>
{
    private readonly IRoutingAlgo _routing;
    private readonly ILogger<PipelineManager> _logger;
    private readonly QueueManager _queueManager;

    private TransformBlock<object, MessageDTO?> _inputStage;
    private TransformBlock<MessageDTO, MessageDTO?> _routingStage;

    public PipelineManager(
        NFanoutConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<PipelineManager> logger,
        IHostApplicationLifetime lf,
        QueueManager queueManager)
    {
        _logger = logger;
        _queueManager = queueManager;

        // looking for spcific routing algo idf configured by clients otherwise default        
        //_routing = specificRoutingAlgo ?? provider.GetRequiredService<DefaultRoutingAlgo>();

        //
        // Get the right routing
        //
        _routing = config.Routing switch
        {
            RoutingType.ROUND_ROBIN => serviceProvider.GetRequiredService<RoundRobin>(),
            RoutingType.LESS_WORKLOAD => serviceProvider.GetRequiredService<LessWorkload>(),
            // if registered use custom alogorithm otherwise default key extractor
            RoutingType.BY_KEY => serviceProvider.GetService<IRoutingAlgo>() ?? serviceProvider.GetRequiredService<DefaultKeyRouting>(),
            _ => serviceProvider.GetRequiredService<DefaultKeyRouting>()
        };

        // ------------------------
        // building the pipeline 
        // ------------------------

        // input stage
        _inputStage = new TransformBlock<object, MessageDTO?>(
            // here is conversion to encapsulated message and set to unique progressive key (timestamp)
            DoWrap,
            new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

        _routingStage = new TransformBlock<MessageDTO, MessageDTO?>(
            DoRouting,
            new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            });

        // null messages will be dropped
        _inputStage.LinkTo(
            DataflowBlock.NullTarget<MessageDTO?>(),
            (dto) => dto == null);

        // null messages will be dropped
        _routingStage.LinkTo(
            DataflowBlock.NullTarget<MessageDTO?>(),
            (dto) => dto == null);

        // link pipeline
        _inputStage.LinkTo(_routingStage!,
            new DataflowLinkOptions { PropagateCompletion = true });

        _routingStage.LinkTo(queueManager.InputQueue!,
            new DataflowLinkOptions { PropagateCompletion = true });

        //
        // Register for program gracefully termination
        //
        lf.ApplicationStopping.Register(() =>
        {
            _inputStage.Complete();
            _logger.LogInformation("Program termination!. Waiting dequeue...");
        });
    }

    /// <summary>
    /// Messages input port into pipeline
    /// </summary>
    /// <param name="value"></param>
    public void Push(object value) => _inputStage.Post(value);

    /// <summary>
    /// Complete messages
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private MessageDTO? DoWrap(object value)
    {
        if (value == null)
            return null;

        return new MessageDTO(
            value,
            DateTime.UtcNow.Ticks);
    }

    /// <summary>
    /// Routing messages
    /// </summary>
    /// <param name="dTO"></param>
    private MessageDTO? DoRouting(MessageDTO dto)
    {
        try
        {
            dto.QueueKey = _routing.GetQueueKey(dto);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Key extraction error: {ex.Message}");
            return null;
        }
    }
}

