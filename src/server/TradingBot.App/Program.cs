using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Common;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Messaging;
using TradingBot.Lib;

namespace TradingBot.App
{
    public class Program
    {
        private string _executionService;

        public static void Main()
        {
            var p = new Program();

            p.Run().Wait();

            Console.ReadLine();
        }

        private async Task Run()
        {
            Console.WriteLine("Running...");

            var config = ServiceConfiguration.FromArgs(new string[] {"config.docker.json"});

            IConnected<IBroker> connectedBroker;
            using (var connectionFactory = BrokerConnectionFactory.Create(config.Broker))
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

            Console.WriteLine($"Found execution service {_executionService}");

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateBuyOrder("EURUSD", 1.09393m, 1000000)
                .Subscribe(x => Buy(x));

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateSellOrder("EURUSD", 1.09493m, 1000000)
                .Subscribe(x => Sell(x));
        }

        private void Buy(dynamic trade)
        {
            Console.WriteLine($"Buying {trade}");
        }

        private void Sell(dynamic trade)
        {
            Console.WriteLine($"Selling {trade}");
        }
    }
}
