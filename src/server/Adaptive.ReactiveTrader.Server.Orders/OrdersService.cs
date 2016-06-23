using System;
using System.Text;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Messaging.Abstraction;
using Adaptive.ReactiveTrader.Messaging.WAMP;
using Newtonsoft.Json;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;

namespace OrdersService
{
    public class OrdersService : ServiceHostBase
    {
        private readonly IWampRealmProxy _realmProxy;
        private readonly string _executionService;
        private readonly IBroker _broker;

        public OrdersService(IBroker broker, string type, IWampRealmProxy realmProxy, string executionService) : base(broker, type)
        {
            _broker = broker;
            _realmProxy = realmProxy;
            _executionService = executionService;
        }

        public void Initialize()
        {
            // Register a handler for calls to placeOrder
            RegisterCall("placeOrder", PlaceOrder);
            StartHeartBeat();
            Console.WriteLine($"Service {InstanceID} up and running");
        }

        private Task PlaceOrder(IRequestContext ctx, IMessage msg)
        {
            Console.WriteLine($"Received Order from {ctx.UserSession.Username}:");

            var trade = JsonConvert.DeserializeObject<ExecuteTradeRequestDto>(Encoding.UTF8.GetString(msg.Payload));

            Console.WriteLine($"{trade.Direction} {trade.CurrencyPair} at {trade.SpotRate}, notional {trade.Notional}");

            _broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateOrder(trade)
                .Subscribe(ExecuteTrade);

            return Task.FromResult("Order placed");
        }

        private void ExecuteTrade(ExecuteTradeRequestDto trade)
        {
            Console.WriteLine($"Executing order to {trade.Direction} {trade.CurrencyPair} at {trade.SpotRate}, notional {trade.Notional}");

            _realmProxy.RpcCatalog.Invoke
                (new ConsoleCallback(),
                    new CallOptions(),
                    $"{_executionService}.executeTrade",
                    new object[] { new MessageDto { Payload = trade, ReplyTo = "", Username = "Orders Service" } });
        }
    }
}