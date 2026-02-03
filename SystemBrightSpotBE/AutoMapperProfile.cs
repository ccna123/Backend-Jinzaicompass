
using AutoMapper;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Dtos.Company;
using SystemBrightSpotBE.Dtos.MonitoringSystem;
using SystemBrightSpotBE.Dtos.Plan;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Dtos.Setting;
using SystemBrightSpotBE.Dtos.Tenant;
using SystemBrightSpotBE.Dtos.User;
using SystemBrightSpotBE.Dtos.UserProject;
using SystemBrightSpotBE.Dtos.UserSkill;
using SystemBrightSpotBE.Dtos.UserStatusHistory;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            var baseType = typeof(BaseCategoryModel);
            var allCategoryModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));

            foreach (var model in allCategoryModels)
            {
                CreateMap(model, typeof(CategoryDto));
                CreateMap(typeof(AddCategoryDto), model);
            }
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.gender_name, opt => opt.MapFrom(src => src.Gender != null ? src.Gender.name : ""))
                .ForMember(dest => dest.role_name, opt => opt.MapFrom(src => src.Role != null? src.Role.name : ""))
                .ForMember(dest => dest.department_name, opt => opt.MapFrom(src => src.Department != null ? src.Department.name : ""))
                .ForMember(dest => dest.division_name, opt => opt.MapFrom(src => src.Division != null ? src.Division.name : ""))
                .ForMember(dest => dest.group_name, opt => opt.MapFrom(src => src.Group != null ? src.Group.name : ""))
                .ForMember(dest => dest.position_name, opt => opt.MapFrom(src => src.Position != null ? src.Position.name : ""))
                .ForMember(dest => dest.employment_type_name, opt => opt.MapFrom(src => src.EmploymentType != null ? src.EmploymentType.name : ""))
                .ForMember(dest => dest.employment_status_name, opt => opt.MapFrom(src => src.EmploymentStatus != null ? src.EmploymentStatus.name : "" ))
                .ForMember(dest => dest.status_history, opt => opt.MapFrom(src => src.UserStatusHistory));
            CreateMap<User, UserListDto>();
            CreateMap<AddUserDto, User>()
               .ForMember(dest => dest.date_of_birth, opt => opt.MapFrom(src => DateOnlyHelper.Parse(src.date_of_birth)));
            CreateMap<UpdateUserDto, User>()
               .ForMember(dest => dest.date_of_birth, opt => opt.MapFrom(src => DateOnlyHelper.Parse(src.date_of_birth)));
            CreateMap<UpdateUserGeneralDto, User>();
            CreateMap<CreateUserCertificationDto, UserCertification>();
            CreateMap<CreateUserCompanyAwardDto, UserCompanyAward>();
            CreateMap<UserStatusHistory, StatusHistoryDto>();
            CreateMap<Role, CategoryDto>();
            CreateMap<Gender, CategoryDto>();
            CreateMap<Position, CategoryDto>();
            CreateMap<EmploymentType, CategoryDto>();
            CreateMap<EmploymentStatus, CategoryDto>();
            CreateMap<Certification, CategoryDto>();
            CreateMap<CompanyAward, CategoryDto>();
            CreateMap<ParticipationProcess, CategoryDto>();
            CreateMap<ParticipationPosition, CategoryDto>();
            CreateMap<ReportType, CategoryDto>();
            CreateMap<Department, CategoryDto>();
            CreateMap<Division, CategoryDto>();
            CreateMap<Group, CategoryDto>();
            CreateMap<ExperienceJob, CategoryDto>();
            CreateMap<ExperienceField, CategoryDto>();
            CreateMap<ExperienceArea, CategoryDto>();
            CreateMap<SpecificSkill, CategoryDto>();
            CreateMap<CreateUserProjectDto, Project>();
            CreateMap<UpdateUserProjectDto, Project>();
            CreateMap<Project, UserProjectDto>();
            CreateMap<Company, CompanyDto>();
            CreateMap<CreateCompanyDto, Company>();
            CreateMap<UpdateCompanyDto, Company>();
            CreateMap<CreateReportDto, Report>();
            CreateMap<UpdateReportDto, Report>();
            CreateMap<CreatePlanDto, Plan>();
            CreateMap<Plan, PlanDto>();
            CreateMap<UserPlanActivity, UserPlanActivityDto>();
            CreateMap<UserPlanConditionActivity, UserPlanConditionActivityDto>();
            CreateMap<Setting, SettingDto>();
            CreateMap<UpdateSettingDto, Setting>();
            CreateMap<Tenant, TenantDto>();
            CreateMap<CreateTenantDto, Tenant>();
            CreateMap<UpdateTenantDto, Tenant>();
            CreateMap<MonitoringSystem, MonitoringSystemDto>();
            CreateMap<CreateMonitoringSystemDto, MonitoringSystem>();
        }
    }
}