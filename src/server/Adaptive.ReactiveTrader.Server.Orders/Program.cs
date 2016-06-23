using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WampSharp.V2;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Messaging;
using WampSharp.V2.Client;

namespace OrdersService
{
    public class Program
    {
        private string _executionService;
        private IServiceConfiguration _config;
        private IWampRealmProxy _realmProxy;
        private IBroker _broker;

        public static void Main(string[] args)
        {
            new Program().Run(args);

            Console.ReadLine();
        }

        private async void Run(string[] args)
        {
            _config = ServiceConfiguration.FromArgs(args);

            await ConnectToBroker();

            var serviceHost = new OrdersService(_broker, "orders", _realmProxy, _executionService);

            serviceHost.Initialize();
        }

        // Note: This is a overly simplified version of the broker connection code. The other services
        // use a fuller version which reacts to broker/eventstore disconnections and reconnections
        private async Task ConnectToBroker()
        {
            Console.WriteLine("Initializing connection...");

            // 1. Find a broker that is connected
            IConnected<IBroker> connectedBroker;
            using (var connectionFactory = BrokerConnectionFactory.Create(_config.Broker))
            {
                connectionFactory.Start();
                connectedBroker = await connectionFactory.GetBrokerStream()
                    .Where(x => x.IsConnected)
                    .Take(1);
            }

            Console.WriteLine("Connected to Broker");

            _broker = connectedBroker.Value;

            // 2. Find an instance of an execution service
            _executionService = await _broker.SubscribeToTopic<HeartbeatDto>("status")
                .Where(x => x.Type == "execution")
                .Select(x => x.Instance)
                .Take(1);

            Console.WriteLine($"Found execution service {_executionService}");

            // 3. Create a proxy to publish messages to
            var factory = new DefaultWampChannelFactory();
            var channel = factory.CreateJsonChannel($"ws://{_config.Broker.Host}:{_config.Broker.Port}/ws", _config.Broker.Realm);

            await channel.Open();

            Console.WriteLine($"Opened channel to {_executionService}");

            _realmProxy = channel.RealmProxy;
        }
    }
}