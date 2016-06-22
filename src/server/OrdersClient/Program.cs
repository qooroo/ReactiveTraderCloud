using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WampSharp.V2;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Messaging.WAMP;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;

namespace OrdersClient
{
    public class Program
    {
        private IServiceConfiguration _config;
        private IBroker _broker;
        private string _ordersService;
        private IWampRealmProxy _realmProxy;

        public static void Main(string[] args)
        {
            Console.WriteLine("ORDERS CLIENT");

            new Program().Run(args).Wait();

            Console.ReadLine();
        }

        private async Task Run(string[] args)
        {
            _config = ServiceConfiguration.FromArgs(args);

            await InitializeConnection();

            while (true)
            {
                try
                {
                    var order = Console.ReadLine().Split(' ');

                    var dto = new ExecuteTradeRequestDto
                    {
                        Direction = (DirectionDto)Enum.Parse(typeof(DirectionDto), order[0]),
                        DealtCurrency = order[1],
                        CurrencyPair = order[2],
                        Notional = decimal.Parse(order[3]),
                        SpotRate = decimal.Parse(order[4])
                    };

                    _realmProxy.RpcCatalog.Invoke(
                        new DummyCallback(),
                        new CallOptions(),
                        $"{_ordersService}.placeOrder",
                        new object[] {new MessageDto {Payload = dto, ReplyTo = "", Username = "ordersClient"}});
                }
                catch (Exception)
                {
                    Console.WriteLine("please enter a valid order");
                }
            }
        }

        private async Task InitializeConnection()
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

            _ordersService = await _broker.SubscribeToTopic<HeartbeatDto>("status")
                .Where(x => x.Type == "orders")
                .Select(x => x.Instance)
                .Take(1);

            Console.WriteLine($"Found orders service {_ordersService}");

            var factory = new DefaultWampChannelFactory();
            var channel = factory.CreateJsonChannel($"ws://{_config.Broker.Host}:{_config.Broker.Port}/ws", _config.Broker.Realm);

            await channel.Open();

            Console.WriteLine($"Opened channel to {_ordersService}");

            _realmProxy = channel.RealmProxy;
        }
    }
}
