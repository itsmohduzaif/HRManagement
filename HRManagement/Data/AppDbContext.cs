using HRManagement.Entities;
using HRManagement.Models;
using HRManagement.Models.Leaves;
using HRManagement.Models.Settings;
using HRManagement.Models.Timesheet;
using HRManagement.SeedConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<ThemeSettings> ThemeSettings { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }


        // New Timesheet related DbSets
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new LeaveTypeConfiguration());
            builder.ApplyConfiguration(new EmployeeConfiguration());

            builder.ApplyConfiguration(new GeneralSettingsConfiguration());
            builder.ApplyConfiguration(new ThemeSettingsConfiguration());
            builder.ApplyConfiguration(new EmailSettingsConfiguration());
            builder.ApplyConfiguration(new EmailTemplateConfiguration());

            //builder.ApplyConfiguration(new UserConfiguration());



            // Configure the LeaveDaysUsed property in LeaveRequest entity for the precision and scale, because otherwise it gives warning in console.
            builder.Entity<LeaveRequest>()
                .Property(lr => lr.LeaveDaysUsed)
                .HasColumnType("decimal(18,2)");  // Precision of 18 and scale of 2

            builder.Entity<TimesheetEntry>()
                .Property(te => te.Hours)
                .HasColumnType("decimal(18,2)");  // Did same with this because it also gives warning in console about precision and scale for decimal type.


        }

    }

}
