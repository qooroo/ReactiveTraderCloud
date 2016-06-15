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

            Console.WriteLine("hai");
            Console.ReadLine();
        }

        private async Task Run()
        {
            var config = ServiceConfiguration.FromArgs(new string[] {"prod.config.json"});

            IConnected<IBroker> connectedBroker;
            using (var connectionFactory = BrokerConnectionFactory.Create(config.Broker))
            {
                connectedBroker = await connectionFactory.GetBrokerStream()
                    .Where(x => x.IsConnected)
                    .Take(1);
            }

            var broker = connectedBroker.Value;

            _executionService = await broker.SubscribeToTopic<HeartbeatDto>("status")
                .Where(x => x.Type == "execution")
                .Select(x => x.Instance)
                .Take(1);

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateBuyOrder("EURUSD", 1.4m, 1000000)
                .Subscribe(x => Buy(x));

            broker.SubscribeToTopic<SpotPriceDto>("prices")
                .CreateSellOrder("EURUSD", 1.5m, 1000000)
                .Subscribe(x => Sell(x));
        }

        private void Buy(dynamic trade)
        {
            
        }

        private void Sell(dynamic trade)
        {
            
        }
    }
}
