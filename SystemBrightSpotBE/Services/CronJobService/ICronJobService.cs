using NCrontab;

namespace SystemBrightSpotBE.Services.CronJobService
{
    public abstract class ICronJobService(string cronExpression, TimeZoneInfo timeZoneInfo) : IHostedService, IDisposable
    {
        private System.Timers.Timer? _timer;
        private readonly string _cronExpression = cronExpression;
        private readonly TimeZoneInfo _timeZoneInfo = timeZoneInfo;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ScheduleJob();
            return Task.CompletedTask;
        }

        private void ScheduleJob()
        {
            var schedule = CrontabSchedule.Parse(_cronExpression);

            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo);
            var next = schedule.GetNextOccurrence(now);

            var delay = next - now;

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer?.Stop();
                await DoWork(null);
                ScheduleJob();
            };

            _timer.Start();
        }

        public virtual Task DoWork(object state)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
