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

namespace TradingBot.App
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
            RegisterCall("placeOrder", PlaceOrder);
            StartHeartBeat();
        }

        private Task PlaceOrder(IRequestContext ctx, IMessage msg)
        {
            Console.WriteLine($"Received Order from {ctx.UserSession.Username}");

            var payload = JsonConvert.DeserializeObject<ExecuteTradeRequestDto>(Encoding.UTF8.GetString(msg.Payload));

            _broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateOrder(payload)
                .Subscribe(ExecuteTrade);

            return Task.FromResult("Order placed");
        }

        private void ExecuteTrade(ExecuteTradeRequestDto trade)
        {
            Console.WriteLine($"Executing order to {trade.Direction} {trade.CurrencyPair} at {trade.SpotRate}, notional {trade.Notional}");

            _realmProxy.RpcCatalog.Invoke
                (new DummyCallback(),
                    new CallOptions(),
                    $"{_executionService}.executeTrade",
                    new object[] {new MessageDto {Payload = trade, ReplyTo = "", Username = "bot"}});
        }
    }
}