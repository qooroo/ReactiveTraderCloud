using Adaptive.ReactiveTrader.Contract;
using System;
using System.Reactive.Linq;

namespace TradingBot.App
{
    public static class BuyLoSellHi
    {
        public static IObservable<ExecuteTradeRequestDto> CreateBuyOrder(this IObservable<SpotPriceDto> source, string symbol, decimal triggerPrice, decimal notional)
        {
            return source
                .Where(s => s.Symbol == symbol)
                .Where(s => s.Ask <= triggerPrice)
                .Select(s => new ExecuteTradeRequestDto { CurrencyPair = s.Symbol, SpotRate = s.Ask,Direction = DirectionDto.Buy, Notional = notional , DealtCurrency = "EUR", ValueDate = s.ValueDate.ToString("dd/MM/yy")})
                .Take(1);
        }

        public static IObservable<ExecuteTradeRequestDto> CreateSellOrder(this IObservable<SpotPriceDto> source, string symbol, decimal triggerPrice, decimal notional)
        {
            return source
                .Where(s => s.Symbol == symbol)
                .Where(s => s.Bid >= triggerPrice)
                .Select(s => new ExecuteTradeRequestDto { CurrencyPair = s.Symbol, SpotRate = s.Ask, Direction = DirectionDto.Sell, Notional = notional, DealtCurrency = "EUR", ValueDate = s.ValueDate.ToString("dd/MM/yy") })
                .Take(1);
        }
    }
}