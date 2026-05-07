using HRManagement.Data;
using HRManagement.Services.Emails;

namespace HRManagement.Services.Notifications
{
    public class NotificationBackgroundService: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;
        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory, ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //throw new Exception("Test exception in NotificationBackgroundService"); // Simulate an error for testing purposes

                    using var scope = _scopeFactory.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var adminEmail = scope.ServiceProvider.GetRequiredService<IConfiguration>()["Notifications:AdminEmail"];

                    // resolve processor HERE (scoped)

                    ///////////////////////Expiry Notification Processor///////////////////////
                    var processor = scope.ServiceProvider.GetRequiredService<IExpiryNotificationProcessor>();

                    await processor.ProcessAsync(db, email, adminEmail);


                    ///////////////////////Now Timesheet Reminder///////////////////////
                    ///
                    var today = DateTime.UtcNow.Date.Day;
                    if (today == 25)
                    {
                        var timesheetProcessor = scope.ServiceProvider.GetRequiredService<ITimesheetReminderProcessor>();
                        await timesheetProcessor.ProcessAsync(db, email);
                    }




                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in NotificationBackgroundService: {ex.Message}\n\n\n");
                    _logger.LogError(ex, "Error running expiry notification job");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
