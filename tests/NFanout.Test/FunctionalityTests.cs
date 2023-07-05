using Microsoft.Extensions.DependencyInjection;
using NFanout.Core;
using NFanout.Devices;
using NFanout.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace NFanout.Test
{
    public class FunctionalityTests
    {
        private ServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging.AddConsole();

            var services = builder.Services;

            // 
            // setup DI
            //
            services.AddNFanout((cfg) =>
            {
                cfg.Routing = RoutingType.ROUND_ROBIN;
                cfg.FixedQueues = 4;
            });

            // declare value type
            services.AddSingleton<PipelineManager>();

            serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void TestRun()
        {
            var inputService = serviceProvider.GetService<PipelineManager>();
            inputService!.Push(1);
            inputService!.Push(2);
            inputService!.Push(3);
            inputService!.Push(4);
            inputService!.Push(5);
            inputService!.Push(6);
            inputService!.Push(7);
            inputService!.Push(8);
        }
    }
}