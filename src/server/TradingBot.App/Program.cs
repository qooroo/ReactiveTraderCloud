using System;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Common.Config;
using Adaptive.ReactiveTrader.Messaging;
using TradingBot.Lib;

namespace TradingBot.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var p = new Program();

            p.Run();




            Console.WriteLine("hai");
            Console.ReadLine();
        }

        private static async void Run(IBroker broker)
        {
            var config = ServiceConfiguration.FromArgs(args);

            using (var connectionFactory = BrokerConnectionFactory.Create(config.Broker))
            {
                connectionFactory.GetBrokerStream()
                    .Where(x => x.IsConnected)
                    .Take(1)
                    .Subscribe(x => Run(x.Value));
            }

            var executionService = await broker.SubscribeToTopic<HeartbeatDto>("status")
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

        private static void Buy(dynamic trade)
        {
            
        }

        private static void Sell(dynamic trade)
        {
            
        }
    }
}
