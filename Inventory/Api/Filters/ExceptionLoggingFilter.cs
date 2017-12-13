using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Api.Filters
{
    public class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public ExceptionLoggingFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ExceptionLoggingFilter));
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.ActionDescriptor.ToString());
        }
    }
}