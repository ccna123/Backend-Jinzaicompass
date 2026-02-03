using log4net;

namespace SystemBrightSpotBE.Services.Hosted
{
    public class LogInitializerHostedService : IHostedService
    {
        private readonly LogInitializer _logInitializer;
        private ILog _log;

        public LogInitializerHostedService(LogInitializer logInitializer)
        {
            _logInitializer = logInitializer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log = _logInitializer.InitLog();
            _log.Info("🔵 Application starting...");

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _log.Error("❌ Unhandled exception occurred", args.ExceptionObject as Exception);
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                _log.Warn("🟡 Application is shutting down via CancelKeyPress (SIGINT or Ctrl+C).");
                e.Cancel = false;
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _log.Warn("🟠 Application is shutting down via ProcessExit.");
            };

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.Warn("🔴 Application stopping gracefully...");
            return Task.CompletedTask;
        }
    }
}