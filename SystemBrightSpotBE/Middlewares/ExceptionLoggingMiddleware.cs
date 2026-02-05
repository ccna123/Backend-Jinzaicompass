using log4net;

namespace SystemBrightSpotBE.Middlewares
{
    public class ExceptionLoggingMiddleware
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExceptionLoggingMiddleware));
        private readonly RequestDelegate _next;

        public ExceptionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Unhandled request exception {context.Request.Method} {context.Request.Path}", ex);
                Console.Error.WriteLine($"❌ Unhandled request exception {context.Request.Method} {context.Request.Path}: {ex}");
                throw;
            }
        }
    }
}
