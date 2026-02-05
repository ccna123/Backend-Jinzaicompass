using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Providers;

namespace SystemBrightSpotBE.Data
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        private readonly ITenantProvider _tenantProvider;

        public DataContext(IConfiguration configuration, ITenantProvider tenantProvider)
        {
            Configuration = configuration;
            _tenantProvider = tenantProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var connectionString = Configuration.GetConnectionString("WebApiDatabase");
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseNpgsql(connectionString);
                }
            }
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<User> users { get; set; }
        public DbSet<UserStatusHistory> user_status_history { get; set; }
        public DbSet<UserExperienceJob> user_experience_job { get; set; }
        public DbSet<UserExperienceField> user_experience_field { get; set; }
        public DbSet<UserExperienceArea> user_experience_area { get; set; }
        public DbSet<UserSpecificSkill> user_specific_skill { get; set; }
        public DbSet<UserCertification> user_certification { get; set; }
        public DbSet<UserCompanyAward> user_company_award { get; set; }
        public DbSet<Project> projects { get; set; }
        public DbSet<ProjectParticipationProcess> project_participation_process { get; set; }
        public DbSet<ProjectParticipationPosition> project_participation_position { get; set; }
        public DbSet<ProjectExperienceJob> project_experience_job { get; set; }
        public DbSet<ProjectExperienceField> project_experience_field { get; set; }
        public DbSet<ProjectExperienceArea> project_experience_area { get; set; }
        public DbSet<ProjectSpecificSkill> project_specific_skill { get; set; }
        public DbSet<ParticipationProcess> participation_processes { get; set; }
        public DbSet<ParticipationPosition> participation_positions { get; set; }
        public DbSet<Gender> genders { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<EmploymentStatus> employment_status { get; set; }
        public DbSet<EmploymentType> employment_types { get; set; }
        public DbSet<Position> positions { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Division> divisions { get; set; }
        public DbSet<Group> groups { get; set; }
        public DbSet<ExperienceJob> experience_jobs { get; set; }
        public DbSet<ExperienceField> experience_fields { get; set; }
        public DbSet<ExperienceArea> experience_areas { get; set; }
        public DbSet<SpecificSkill> specific_skills { get; set; }
        public DbSet<Certification> certifications { get; set; }
        public DbSet<CompanyAward> company_awards { get; set; }
        public DbSet<Company> companies { get; set; }
        public DbSet<Report> reports { get; set; }
        public DbSet<ReportDepartment> report_departments { get; set; }
        public DbSet<ReportDivision> report_divisions { get; set; }
        public DbSet<ReportGroup> report_groups { get; set; }
        public DbSet<ReportUser> report_users { get; set; }
        public DbSet<ReportType> report_types { get; set; }
        public DbSet<Plan> plans { get; set; }
        public DbSet<PlanCondition> plan_conditions { get; set; }
        public DbSet<UserPlan> user_plan { get; set; }
        public DbSet<UserPlanCondition> user_plan_condition { get; set; }
        public DbSet<UserPlanActivity> user_plan_activity { get; set; }
        public DbSet<UserPlanConditionActivity> user_plan_condition_activity { get; set; }
        public DbSet<Setting> settings { get; set; }
        public DbSet<Notification> notifications { get; set; }
        public DbSet<Tenant> tenants { get; set; }
        public DbSet<MonitoringSystem> monitoring_systems { get; set; }

        public override int SaveChanges()
        {
            AddTenantId();
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            AddTenantId();
            AddTimestamps();
            return await base.SaveChangesAsync();
        }

        public void AddTenantId()
        {
            var tenantId = _tenantProvider.GetTenantId();

            if (tenantId == null)
            {
                return;
            }

            var entries = ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added) && e.Entity is BaseEntity)
                .ToList();

            foreach (var entry in entries)
            {
                ((BaseEntity)entry.Entity).tenant_id = tenantId.Value;
            }
        }

        public void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Entity is BaseEntity)
                .ToList();

            foreach (var entity in entities)
            {
                var now = DateTime.Now;

                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).created_at = now;
                }
                ((BaseEntity)entity.Entity).updated_at = now;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var fk in entityType.GetForeignKeys())
                {
                    if (fk.Properties.Any(p => p.Name == "tenant_id") && fk.PrincipalEntityType.ClrType == typeof(Tenant))
                    {
                        fk.DeleteBehavior = DeleteBehavior.Cascade;
                    }
                }
            }

            var baseCategoryType = typeof(BaseCategoryModel);
            modelBuilder.Model.GetEntityTypes().Where(t => baseCategoryType.IsAssignableFrom(t.ClrType) && t.ClrType != baseCategoryType);
        }

        [DbFunction("regexp_replace", IsBuiltIn = true)]
        public static string RegexReplace(string input, string pattern, string replacement, string flags) => throw new NotSupportedException();
    }
}
