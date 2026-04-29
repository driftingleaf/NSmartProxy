using System;

namespace NSmartProxy
{
    public class Program
    {
        static void Main(string[] args)
        {
            var host = new NSmartProxyClient();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                host.Stop();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, __) => host.Stop();

            host.Start(args);
        }
    }
}
