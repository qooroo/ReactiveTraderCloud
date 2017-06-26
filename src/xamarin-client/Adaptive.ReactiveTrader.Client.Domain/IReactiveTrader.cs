using System;
using Adaptive.ReactiveTrader.Client.Domain.Instrumentation;
using Adaptive.ReactiveTrader.Client.Domain.Repositories;
using Adaptive.ReactiveTrader.Shared.Logging;
using Adaptive.ReactiveTrader.Client.Domain.ServiceClients;
using System.Threading.Tasks;

namespace Adaptive.ReactiveTrader.Client.Domain
{
    public interface IReactiveTrader
    {
        IReferenceDataRepository ReferenceData { get; }
        ITradeRepository TradeRepository { get; }
        IObservable<ConnectionInfo> ConnectionStatusStream { get; }
        IPriceLatencyRecorder PriceLatencyRecorder { get; }
        IPricingServiceClient PricingServiceClient { get; }
        Task InitializeAsync(string username, string[] servers, ILoggerFactory loggerFactory = null, string authToken = null);
    }
}