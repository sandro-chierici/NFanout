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
using NFanout.Adapters;
using NFanout.Core;
using NFanout.Devices;
using NFanout.Devices.RoutingAlgos;

namespace NFanout.Configuration;

public static class DependencyInjectionExtensions
{
    public static void AddNFanout(
        this IServiceCollection serviceCollection,
        Action<NFanoutConfiguration>? configure = null)
    {
        // build default configuration and call for customizations
        var configuration = new NFanoutConfiguration();
        configure?.Invoke(configuration);

        // spread configuration
        serviceCollection.AddSingleton(configuration);

        // register RoutingAlgos provided
        serviceCollection.AddSingleton<DefaultKeyRouting>();
        serviceCollection.AddSingleton<RoundRobin>();
        serviceCollection.AddSingleton<LessWorkload>();

        // Default Worker
        serviceCollection.AddSingleton<DefaultWorker>();

        // Queues
        serviceCollection.AddSingleton<QueueManager>();

        // Pipeline Manager. Register as contravariant interface
        serviceCollection.AddSingleton<IDataInput<object>, PipelineManager>();
    }
}
