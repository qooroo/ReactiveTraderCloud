using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WampSharp.V2;
using WampSharp.V2.Core.Contracts;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Messaging.Abstraction;
using Adaptive.ReactiveTrader.Messaging.WAMP;
using Newtonsoft.Json;
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

            await InitializeConnection();

            var serviceHost = new SimpleOrdersService(_broker, "simpleOrders", _executionRealmProxy, _executionService);
            serviceHost.Initialize();

            //_broker.SubscribeToTopic<SpotPriceDto>("prices")
            //    .CreateBuyOrder("EURUSD", 1.09433m, 1000000)
            //    .Subscribe(ExecuteTrade);

            //_broker.SubscribeToTopic<SpotPriceDto>("prices")
            //    .CreateSellOrder("EURUSD", 1.09453m, 1000000)
            //    .Subscribe(ExecuteTrade);
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

    public class SimpleOrdersService : ServiceHostBase
    {
        private readonly IWampRealmProxy _realmProxy;
        private readonly string _executionService;
        private readonly IBroker _broker;

        public SimpleOrdersService(IBroker broker, string type, IWampRealmProxy realmProxy, string executionService) : base(broker, type)
        {
            _broker = broker;
            _realmProxy = realmProxy;
            _executionService = executionService;
        }

        public void Initialize()
        {
            RegisterCall("placeOrder", PlaceOrder);
        }

        private Task PlaceOrder(IRequestContext ctx, IMessage msg)
        {
            Console.WriteLine("Received Order from {username}", ctx.UserSession.Username);

            var payload = JsonConvert.DeserializeObject<ExecuteTradeRequestDto>(Encoding.UTF8.GetString(msg.Payload));

            if (payload.Direction == DirectionDto.Buy)
            {
                _broker.SubscribeToTopic<SpotPriceDto>("prices")
                    .CreateOrder(payload)
                    .Subscribe(ExecuteTrade);
            }

            return Task.FromResult("Order placed");
        }

        private void ExecuteTrade(ExecuteTradeRequestDto trade)
        {
            Console.WriteLine($"Executing order to {trade.Direction} {trade.CurrencyPair} at {trade.SpotRate}, notional {trade.Notional}");

            _realmProxy.RpcCatalog.Invoke
                (new DummyCallback(),
                 new CallOptions(),
                 $"{_executionService}.executeTrade",
                 new object[] { new MessageDto { Payload = trade, ReplyTo = "", Username = "bot" } });
        }
    }
}