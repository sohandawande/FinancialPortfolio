using FinancialPortfolio.Business.Common.Logging;
using System.Runtime.CompilerServices;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class LogSourceHelper
    {
        public static LogSource Of(string category, [CallerMemberName] string method = "")
        {
            return LogSource.Of(category, method);
        }

        public static LogSource Service<T>([CallerMemberName] string method = "")
        {
            return LogSource.Of(typeof(T).Name, method);
        }

        public static LogSource Controller<T>([CallerMemberName] string method = "")
        {
            return LogSource.Of(typeof(T).Name, method);
        }

        public static LogSource FromType(Type type, [CallerMemberName] string method = "")
        {
            return LogSource.Of(type.Name, method);
        }

        public static LogSource Current([CallerFilePath] string filePath = "", [CallerMemberName] string method = "")
        {
            return LogSource.Of(filePath, method);
        }

    }
}
