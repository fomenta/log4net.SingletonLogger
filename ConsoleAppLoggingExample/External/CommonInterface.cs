namespace ConsoleApp.External
{
    public static class CommonInterface
    {
        public static void RegisterOnce()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Registering....");
        }
    }
}
