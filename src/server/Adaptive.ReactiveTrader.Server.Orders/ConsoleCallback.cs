using System;
using System.Collections.Generic;
using WampSharp.Core.Serialization;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.Rpc;

namespace OrdersService
{
    internal sealed class ConsoleCallback : IWampRawRpcOperationClientCallback
    {
        public void Result<TMessage>(IWampFormatter<TMessage> formatter, ResultDetails details)
        {
            Console.WriteLine("OK");
        }

        public void Result<TMessage>(IWampFormatter<TMessage> formatter, ResultDetails details, TMessage[] arguments)
        {
            Console.WriteLine("OK");
        }

        public void Result<TMessage>(IWampFormatter<TMessage> formatter,
            ResultDetails details,
            TMessage[] arguments,
            IDictionary<string, TMessage> argumentsKeywords)
        {
            Console.WriteLine("OK");
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error)
        {
            Console.WriteLine("Not OK");
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error, TMessage[] arguments)
        {
            Console.WriteLine("Not OK");
        }

        public void Error<TMessage>(IWampFormatter<TMessage> formatter, TMessage details, string error, TMessage[] arguments,
            TMessage argumentsKeywords)
        {
            Console.WriteLine("Not OK");
        }
    }
}