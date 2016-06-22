using Adaptive.ReactiveTrader.Contract;
using System;
using System.Reactive.Linq;

namespace TradingBot.App
{
    public static class OrderCreator
    {
        public static IObservable<ExecuteTradeRequestDto> CreateOrder(this IObservable<SpotPriceDto> source, ExecuteTradeRequestDto request)
        {
            return (from s in source
                where s.Symbol == request.CurrencyPair
                where request.Direction == DirectionDto.Buy ? s.Ask <= request.SpotRate : s.Bid >= request.SpotRate
                let spotRate = request.Direction == DirectionDto.Buy ? s.Ask : s.Bid
                select
                    new ExecuteTradeRequestDto
                    {
                        CurrencyPair = s.Symbol,
                        SpotRate = spotRate,
                        Direction = request.Direction,
                        Notional = request.Notional,
                        DealtCurrency = request.DealtCurrency,
                        ValueDate = s.ValueDate.ToString("dd/MM/yy")
                    })
                .Take(1);
        }
    }
}