using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WampSharp.V2;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Messaging;
using WampSharp.V2.Client;

namespace TradingBot.App
{
    public class Program
    {
        private string _executionService;
        private IServiceConfiguration _config;
        private IWampRealmProxy _executionRealmProxy;
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

            var serviceHost = new OrdersService(_broker, "orders", _executionRealmProxy, _executionService);

            serviceHost.Initialize();
        }

        private async Task ConnectToBroker()
        {
            Console.WriteLine("Initializing connection...");

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

            _executionService = await _broker.SubscribeToTopic<HeartbeatDto>("status")
                .Where(x => x.Type == "execution")
                .Select(x => x.Instance)
                .Take(1);

            Console.WriteLine($"Found execution service {_executionService}");

            var factory = new DefaultWampChannelFactory();
            var channel = factory.CreateJsonChannel($"ws://{_config.Broker.Host}:{_config.Broker.Port}/ws", _config.Broker.Realm);

            await channel.Open();

            Console.WriteLine($"Opened channel to {_executionService}");

            _executionRealmProxy = channel.RealmProxy;
        }
    }
}