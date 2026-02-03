using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Dtos.Skill;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.SkillService
{
    public class SkillService : ISkillService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public SkillService(DataContext context, IConfiguration configuration, IMapper mapper, IAuthService authService)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<List<ExperienceJobDto>> GetTree()
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.experience_jobs
                .Where(job => job.tenant_id == tenantId)
                .Include(e => e.ExperienceFields)
                    .ThenInclude(a => a.ExperienceAreas)
                        .ThenInclude(s => s.SpecificSkills)
                .AsNoTracking()
                .OrderByDescending(job => job.updated_at)
                .Select(job => new ExperienceJobDto
                {
                    id = job.id,
                    name = job.name,
                    delete_flag = job.delete_flag ?? false,
                    children = job.ExperienceFields.Select(field => new ExperienceFieldDto
                    {
                        id = field.id,
                        name = field.name,
                        delete_flag = field.delete_flag ?? false,
                        experience_job_id = field.experience_job_id,
                        children = field.ExperienceAreas.Select(area => new ExperienceAreaDto
                        {
                            id = area.id,
                            name = area.name,
                            delete_flag = area.delete_flag ?? false,
                            experience_field_id = area.experience_field_id,
                            children = area.SpecificSkills.Select(spec => new SpecificSkillDto
                            {
                                id = spec.id,
                                name = spec.name,
                                delete_flag = spec.delete_flag ?? false,
                                experience_area_id = spec.experience_area_id
                            }).ToList()
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<SkillDto>> GetExperienceJob()
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.experience_jobs
               .Where(job => job.tenant_id == tenantId)
               .AsNoTracking()
               .OrderByDescending(job => job.updated_at)
               .Select(job => new SkillDto
               {
                   id = job.id,
                   name = job.name
               })
               .ToListAsync();
        }

        public async Task<List<SkillDto>> GetExperienceFieldByJob(string ids)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var listId = ids.Split(',')
                    .Where(x => long.TryParse(x, out _))
                    .Select(long.Parse)
                    .ToList();

            return await _context.experience_fields
               .Where(field => field.tenant_id == tenantId)
               .Where(field => listId.Contains(field.experience_job_id))
               .AsNoTracking()
               .OrderByDescending(field => field.updated_at)
               .Select(field => new SkillDto
               {
                   id = field.id,
                   name = field.name
               })
               .ToListAsync();
        }

        public async Task<List<SkillDto>> GetExperienceAreaByField(string ids)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var listId = ids.Split(',')
                    .Where(x => long.TryParse(x, out _))
                    .Select(long.Parse)
                    .ToList();

            return await _context.experience_areas
               .Where(area => area.tenant_id == tenantId)
               .Where(area => listId.Contains(area.experience_field_id))
               .AsNoTracking()
               .OrderByDescending(area => area.updated_at)
               .Select(area => new SkillDto
               {
                   id = area.id,
                   name = area.name
               })
               .ToListAsync();
        }

        public async Task<List<SkillDto>> GetSpecificSkillByArea(string ids)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var listId = ids.Split(',')
                    .Where(x => long.TryParse(x, out _))
                    .Select(long.Parse)
                    .ToList();

            return await _context.specific_skills
               .Where(s => s.tenant_id == tenantId)
               .Where(s => listId.Contains(s.experience_area_id))
               .AsNoTracking()
               .OrderByDescending(s => s.updated_at)
               .Select(s => new SkillDto
               {
                   id = s.id,
                   name = s.name
               })
               .ToListAsync();
        }

        public async Task Create(CreateSkillDto request)
        {
            object? newEntity = request.type switch
            {
                SkillTypeEnum.experience_job => new ExperienceJob { name = request.name },
                SkillTypeEnum.experience_field => new ExperienceField { name = request.name, experience_job_id = request.experience_job_id ?? 1, delete_flag = true },
                SkillTypeEnum.experience_area => new ExperienceArea { name = request.name, experience_field_id = request.experience_field_id ?? 1, delete_flag = true },
                SkillTypeEnum.specific_skill => new SpecificSkill { name = request.name, experience_area_id = request.experience_area_id ?? 1, delete_flag = true },
                _ => null
            };

            if (newEntity != null)
            {
                _context.Add(newEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Update(long id, UpdateSkillDto request)
        {
            object? entity = request.type switch
            {
                SkillTypeEnum.experience_job => await _context.experience_jobs.FindAsync(id),
                SkillTypeEnum.experience_field => await _context.experience_fields.FindAsync(id),
                SkillTypeEnum.experience_area => await _context.experience_areas.FindAsync(id),
                SkillTypeEnum.specific_skill => await _context.specific_skills.FindAsync(id),
                _ => null
            };

            if (entity != null)
            {
                var propName = entity.GetType().GetProperty("name");
                if (propName != null && propName.CanWrite)
                {
                    propName.SetValue(entity, request.name);
                }
                var propUpdate = entity.GetType().GetProperty("updated_at");
                if (propUpdate != null && propUpdate.CanWrite)
                {
                    propUpdate.SetValue(entity, DateTime.Now);
                }

                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(long id, SkillTypeEnum? type)
        {
            object? entity = type switch
            {
                SkillTypeEnum.experience_job => await _context.experience_jobs.FirstOrDefaultAsync(c => c.id == id),
                SkillTypeEnum.experience_field => await _context.experience_fields.FirstOrDefaultAsync(c => c.id == id),
                SkillTypeEnum.experience_area => await _context.experience_areas.FirstOrDefaultAsync(c => c.id == id),
                SkillTypeEnum.specific_skill => await _context.specific_skills.FirstOrDefaultAsync(c => c.id == id),
                _ => null
            };

            if (entity != null)
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckNameExistAsync(BaseSkillDto request, bool update = false, long id = 0)
        {
            return request.type switch
            {
                SkillTypeEnum.experience_job => await CheckNameExist(_context.experience_jobs, request.name, update, id),
                SkillTypeEnum.experience_field => await CheckNameExist(_context.experience_fields, request.name, update, id),
                SkillTypeEnum.experience_area => await CheckNameExist(_context.experience_areas, request.name, update, id),
                SkillTypeEnum.specific_skill => await CheckNameExist(_context.specific_skills, request.name, update, id),
                _ => false
            };
        }

        private async Task<bool> CheckNameExist<T>(DbSet<T> dbSet, string name, bool update = false, long id = 0) where T : class
        {
            long tenantId = _authService.GetAccountId("Tenant");

            if (update)
            {
                return await dbSet.AnyAsync(e => EF.Property<string>(e, "name") == name && EF.Property<long>(e, "tenant_id") == tenantId && EF.Property<long>(e, "id") != id);
            }
            return await dbSet.AnyAsync(e => EF.Property<string>(e, "name") == name && EF.Property<long>(e, "tenant_id") == tenantId);
        }

        public async Task<CategoryDto?> FindById(long id, SkillTypeEnum? type)
        {
            return type switch
            {
                SkillTypeEnum.experience_job => _mapper.Map<CategoryDto>(await _context.experience_jobs.FirstOrDefaultAsync(c => c.id == id)),
                SkillTypeEnum.experience_field => _mapper.Map<CategoryDto>(await _context.experience_fields.FirstOrDefaultAsync(c => c.id == id)),
                SkillTypeEnum.experience_area => _mapper.Map<CategoryDto>(await _context.experience_areas.FirstOrDefaultAsync(c => c.id == id)),
                SkillTypeEnum.specific_skill => _mapper.Map<CategoryDto>(await _context.specific_skills.FirstOrDefaultAsync(c => c.id == id)),
                _ => null
            };
        }

        public async Task<bool> HasCheck(long id, SkillTypeEnum? type)
        {
            bool hasCheck = type switch
            {
                SkillTypeEnum.experience_job => (
                    await _context.user_experience_job.AnyAsync(u => u.experience_job_id == id) || await _context.project_experience_job.AnyAsync(u => u.experience_job_id == id)
                ),
                SkillTypeEnum.experience_field => (
                    await _context.user_experience_field.AnyAsync(u => u.experience_field_id == id) || await _context.project_experience_field.AnyAsync(u => u.experience_field_id == id)
                ),
                SkillTypeEnum.experience_area => (
                    await _context.user_experience_area.AnyAsync(u => u.experience_area_id == id) || await _context.project_experience_area.AnyAsync(u => u.experience_area_id == id)
                ),
                SkillTypeEnum.specific_skill => (
                    await _context.user_specific_skill.AnyAsync(u => u.specific_skill_id == id) || await _context.project_specific_skill.AnyAsync(u => u.specific_skill_id == id)
                ),
                _ => false
            };

            return hasCheck;
        }
    }
}
