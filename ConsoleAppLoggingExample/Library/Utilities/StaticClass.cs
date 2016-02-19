using ConsoleApp.Business;

namespace ConsoleApp.Library.Utilities
{
    public static class StaticClass
    {
        public static void RegisterOnce()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Registering....");
            using (var c = new Client()) { c.Start(); }

        }
    }
}
