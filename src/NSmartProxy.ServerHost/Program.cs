using System;

namespace NSmartProxy.ServerHost
{
    public class Program
    {
        static void Main()
        {
            var host = new ServerHost();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                host.Stop();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, __) => host.Stop();

            host.Start();
        }
    }
}
