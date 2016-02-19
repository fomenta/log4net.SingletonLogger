using ConsoleApp.External;
using System;

namespace ConsoleApp.Business
{
    public class Client : IDisposable
    {
        public Client()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Constructor");
        }

        public void Start()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Starting");
            RegisterStatic();
            CommonInterface.RegisterOnce();
        }

        public static void RegisterStatic()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Registering...");
        }

        public void Dispose()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Fatal(() => "Disposing...");
        }
    }
}
