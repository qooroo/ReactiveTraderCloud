using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WampSharp.Core.Serialization;
using WampSharp.V2;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.Rpc;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Messaging.WAMP;
using WampSharp.V2.Client;

namespace TradingBot.App
{
    public class Program
    {
        private string _executionService;
        private IServiceConfiguration _config;
        private IWampRealmProxy _executionRealmProxy;

        public static void Main()
        {
            var p = new Program();

            p.Run().Wait();

            Console.ReadLine();
        }

        private async Task Run()
        {
            Console.WriteLine("Running...");

            _config = ServiceConfiguration.FromArgs(new string[] {"config.dev.json"});

            IConnected<IBroker> connectedBroker;
            using (var connectionFactory = BrokerConnectionFactory.Create(_config.Broker))
            {
                connectionFactory.Start();
                connectedBroker = await connectionFactory.GetBrokerStream()
                    .Where(x => x.IsConnected)
                    .Take(1);
            }

            Console.WriteLine("Connected to Broker");

            var broker = connectedBroker.Value;

            _executionService = await broker.SubscribeToTopic<HeartbeatDto>("status")
                .Where(x => x.Type == "execution")
                .Select(x => x.Instance)
                .Take(1);

            var factory = new DefaultWampChannelFactory();
            var channel = factory.CreateJsonChannel($"ws://{ _config.Broker.Host}:{_config.Broker.Port}/ws", _config.Broker.Realm);

            await channel.Open();

            _executionRealmProxy = channel.RealmProxy;

            Console.WriteLine($"Opened channel to {_executionService}");

            Console.WriteLine($"Found execution service {_executionService}");

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateBuyOrder("EURUSD", 1.09433m, 1000000)
                .Subscribe(x => ExecuteTrade(x));

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateSellOrder("EURUSD", 1.09453m, 1000000)
                .Subscribe(x => ExecuteTrade(x));
        }

        private void ExecuteTrade(ExecuteTradeRequestDto trade)
        {
            Console.WriteLine($"Executing trade {trade}");
            _executionRealmProxy.RpcCatalog.Invoke
                (new MyCallback(),
                 new CallOptions(),
                 $"{_executionService}.executeTrade",
                 new object[] { new MessageDto {Payload = trade, ReplyTo = "", Username = "bot"}});
        }
    }

    public class MyCallback : IWampRawRpcOperationClientCallback
    {
        public void Result<TMessage>(IWampFormatter<TMessage> formatter, ResultDetails details)
        {
            Console.WriteLine("Got sth 2");
        }

        public void Result<TMessage>(IWampFormatter<TMessage> formatter, ResultDetails details, TMessage[] arguments)
        {
            Console.WriteLine("Got sth 1");
        }

        public void Result<TMessage>(IWampFormatter<TMessage> formatter,
                                     ResultDetails details,
                                     TMessage[] arguments,
                                     IDictionary<string, TMessage> argumentsKeywords)
        {

            Console.WriteLine("Got sth");
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error)
        {
            throw new NotImplementedException();
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error, TMessage[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error, TMessage[] arguments,
                                    TMessage argumentsKeywords)
        {
            throw new NotImplementedException();
        }
    }
}

