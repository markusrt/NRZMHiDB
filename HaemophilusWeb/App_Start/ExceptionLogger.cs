using System.Web.Mvc;
using NLog;

namespace HaemophilusWeb
{
    public class ExceptionLogger : IExceptionFilter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            Log.Error(string.Format("Unhandled error: {0}", exception.Message), exception);
        }
    }
}