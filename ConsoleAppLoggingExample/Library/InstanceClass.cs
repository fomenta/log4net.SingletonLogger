using ConsoleApp.Library.Utilities;

namespace ConsoleApp.Library
{
    public class InstanceClass
    {
        public InstanceClass()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Constructor");
        }

        public void Start()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Starting");
            RegisterStatic();
            StaticClass.RegisterOnce();
        }

        public static void RegisterStatic()
        {
            if (LogUtility.IsDebugEnabled) LogUtility.Debug(() => "Registering...");
        }
    }
}
