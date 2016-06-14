using System;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Contract;

namespace TradingBot.Lib
{
    public static class BuyLoSellHi
    {
        public static IObservable<dynamic> CreateBuyOrder(this IObservable<SpotPriceDto> source, string symbol, decimal triggerPrice, decimal notional)
        {
            return source
                .Where(s => s.Symbol == symbol)
                .Where(s => s.Ask <= triggerPrice)
                .Select(s => new {s.Symbol, s.Ask, BuySell.Buy, notional})
                .Take(1);
        }

        public static IObservable<dynamic> CreateSellOrder(this IObservable<SpotPriceDto> source, string symbol, decimal triggerPrice, decimal notional)
        {
            return source
                .Where(s => s.Symbol == symbol)
                .Where(s => s.Bid >= triggerPrice)
                .Select(s => new {s.Symbol, s.Ask, BuySell.Sell, notional})
                .Take(1);
        }
    }
}
