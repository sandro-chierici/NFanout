using NFanout.Core;
using NFanout.Devices.RoutingAlgos;

namespace NFanout.Test
{
    internal class RoutingTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRoundRobinCorrect()
        {
            var algo = new RoundRobin(new NFanoutConfiguration
            {
                FixedQueues = 4,
                Routing = RoutingType.ROUND_ROBIN
            });


            Assert.True(algo.GetQueueKey(0) == "0");
            Assert.True(algo.GetQueueKey(0) == "1");
            Assert.True(algo.GetQueueKey(0) == "2");
            Assert.True(algo.GetQueueKey(0) == "3");

            Assert.True(algo.GetQueueKey(0) == "0");

        }

        [Test]
        public void TestRoundRobinIncorrect()
        {
            var algo = new RoundRobin(new NFanoutConfiguration
            {
                FixedQueues = 0, // incorrect < 1
                Routing = RoutingType.ROUND_ROBIN
            });


            Assert.True(algo.GetQueueKey(0) == "0");
            Assert.True(algo.GetQueueKey(0) == "0");
            Assert.True(algo.GetQueueKey(0) == "0");
        }
    }
}
