using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management.Data;

namespace Task_Management.Services
{
    public class DeadlineNotificationJob
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public DeadlineNotificationJob(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task CheckAndNotifyUpcomingDeadlines()
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.AddDays(1);

            var upcomingTasks = await _context.Tasks
                .Where(t => t.DueDate.HasValue &&
                            t.DueDate.Value > now &&
                            t.DueDate.Value <= tomorrow &&
                            t.Status != "Completed")
                .ToListAsync();

            foreach (var task in upcomingTasks)
            {
                var user = await _userManager.FindByIdAsync(task.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var message = $"Reminder: Your task '{task.Title}' is due on {task.DueDate.Value.ToShortDateString()}. Current status: {task.Status}.";
                    await _emailService.SendEmailAsync(user.Email, "Upcoming Task Deadline", message);
                }
            }
        }
    }
}
