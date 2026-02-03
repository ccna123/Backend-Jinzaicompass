using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public CategoryService(DataContext context, IMapper mapper, IAuthService authService)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<List<CategoryDto>> GetAll([FromQuery] CategoryParamDto request)
        {
            return request.type switch
            {
                CategoryTypeEnum.role => await GetCategoryList(_context.roles),
                CategoryTypeEnum.gender => await GetCategoryList(_context.genders),
                CategoryTypeEnum.position => await GetCategoryList(_context.positions, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.employment_type => await GetCategoryList(_context.employment_types, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.employment_status => await GetCategoryList(_context.employment_status),
                CategoryTypeEnum.certification => await GetCategoryList(_context.certifications, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.company_award => await GetCategoryList(_context.company_awards, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.participation_process => await GetCategoryList(_context.participation_processes, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.participation_position => await GetCategoryList(_context.participation_positions, "updated_at", hasDeleteFlag: true),
                CategoryTypeEnum.report_type => await GetCategoryList(_context.report_types, "updated_at", hasDeleteFlag: true),
                _ => throw new ArgumentOutOfRangeException(nameof(request.type), "Unsupported category type")
            };
        }

        private async Task<List<CategoryDto>> GetCategoryList<T>(DbSet<T> dbSet, string? orderByProperty = null, bool hasDeleteFlag = false) where T : class
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var query = dbSet.AsNoTracking().AsQueryable();

            var entityType = _context.Model.FindEntityType(typeof(T));
            bool hasTenantId = entityType?.FindProperty("tenant_id") != null;

            if (hasTenantId)
            {
                query = query.Where(e => EF.Property<long>(e, "tenant_id") == tenantId);
            }

            if (!string.IsNullOrWhiteSpace(orderByProperty))
            {
                query = query.OrderByDescending(e => EF.Property<object>(e, orderByProperty));
            }

            return await query
                .Select(e => new CategoryDto
                {
                    id = EF.Property<long>(e, "id"),
                    name = EF.Property<string>(e, "name"),
                    delete_flag = hasDeleteFlag ? (EF.Property<bool?>(e, "delete_flag") ?? false) : false
                })
                .ToListAsync();
        }

        public async Task Create(AddCategoryDto request)
        {
            object? newEntity = request.type switch
            {
                CategoryTypeEnum.role => new Role { name = request.name },
                CategoryTypeEnum.gender => new Gender { name = request.name },
                CategoryTypeEnum.position => new Position { name = request.name, delete_flag = true },
                CategoryTypeEnum.employment_type => new EmploymentType { name = request.name, delete_flag = true },
                CategoryTypeEnum.employment_status => new EmploymentStatus { name = request.name },
                CategoryTypeEnum.certification => new Certification { name = request.name, delete_flag = true },
                CategoryTypeEnum.company_award => new CompanyAward { name = request.name, delete_flag = true },
                CategoryTypeEnum.participation_process => new ParticipationProcess { name = request.name, delete_flag = true },
                CategoryTypeEnum.participation_position => new ParticipationPosition { name = request.name, delete_flag = true },
                CategoryTypeEnum.report_type => new ReportType { name = request.name, delete_flag = true },
                _ => null
            };

            if (newEntity != null)
            {
                await _context.AddAsync(newEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Update(long id, AddCategoryDto request)
        {
            object? entity = request.type switch
            {
                CategoryTypeEnum.role => await _context.roles.FindAsync(id),
                CategoryTypeEnum.gender => await _context.genders.FindAsync(id),
                CategoryTypeEnum.position => await _context.positions.FindAsync(id),
                CategoryTypeEnum.employment_type => await _context.employment_types.FindAsync(id),
                CategoryTypeEnum.employment_status => await _context.employment_status.FindAsync(id),
                CategoryTypeEnum.certification => await _context.certifications.FindAsync(id),
                CategoryTypeEnum.company_award => await _context.company_awards.FindAsync(id),
                CategoryTypeEnum.participation_process => await _context.participation_processes.FindAsync(id),
                CategoryTypeEnum.participation_position => await _context.participation_positions.FindAsync(id),
                CategoryTypeEnum.report_type => await _context.report_types.FindAsync(id),
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

        public async Task<bool> CheckNameExistAsync(AddCategoryDto request, bool update = false, long id = 0)
        {
            return request.type switch
            {
                CategoryTypeEnum.role => await CheckNameExist(_context.roles, request.name, update, id),
                CategoryTypeEnum.gender => await CheckNameExist(_context.genders, request.name, update, id),
                CategoryTypeEnum.position => await CheckNameExist(_context.positions, request.name, update, id),
                CategoryTypeEnum.employment_type => await CheckNameExist(_context.employment_types, request.name, update, id),
                CategoryTypeEnum.employment_status => await CheckNameExist(_context.employment_status, request.name, update, id),
                CategoryTypeEnum.certification => await CheckNameExist(_context.certifications, request.name, update, id),
                CategoryTypeEnum.company_award => await CheckNameExist(_context.company_awards, request.name, update, id),
                CategoryTypeEnum.participation_process => await CheckNameExist(_context.participation_processes, request.name, update, id),
                CategoryTypeEnum.participation_position => await CheckNameExist(_context.participation_positions, request.name, update, id),
                CategoryTypeEnum.report_type => await CheckNameExist(_context.report_types, request.name, update, id),
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

        public async Task<CategoryDto?> FindById(long id, CategoryTypeEnum? type)
        {
            return type switch
            {
                CategoryTypeEnum.role => _mapper.Map<CategoryDto>(await _context.roles.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.gender => _mapper.Map<CategoryDto>(await _context.genders.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.position => _mapper.Map<CategoryDto>(await _context.positions.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.employment_type => _mapper.Map<CategoryDto>(await _context.employment_types.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.employment_status => _mapper.Map<CategoryDto>(await _context.employment_status.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.certification => _mapper.Map<CategoryDto>(await _context.certifications.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.company_award => _mapper.Map<CategoryDto>(await _context.company_awards.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.participation_process => _mapper.Map<CategoryDto>(await _context.participation_processes.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.participation_position => _mapper.Map<CategoryDto>(await _context.participation_positions.FirstOrDefaultAsync(c => c.id == id)),
                CategoryTypeEnum.report_type => _mapper.Map<CategoryDto>(await _context.report_types.FirstOrDefaultAsync(c => c.id == id)),
                _ => null
            };
        }

        public async Task Delete(long id, CategoryTypeEnum? type)
        {
            switch (type)
            {
                case CategoryTypeEnum.position:
                    {
                        var users = await _context.users.Where(u => u.position_id == id).ToListAsync();

                        foreach (var user in users)
                        {
                            user.position_id = null;
                        }

                        var entity = await _context.positions.FirstOrDefaultAsync(c => c.id == id);
                        if (entity != null)
                        {
                            _context.Remove(entity);
                        }
                        break;
                    }

                case CategoryTypeEnum.employment_type:
                    {
                        var users = await _context.users.Where(u => u.employment_type_id == id).ToListAsync();

                        foreach (var user in users)
                        {
                            user.employment_type_id = null;
                        }

                        var entity = await _context.employment_types.FirstOrDefaultAsync(c => c.id == id);
                        if (entity != null)
                        {
                            _context.Remove(entity);
                        }
                        break;
                    }

                case CategoryTypeEnum.employment_status:
                    {
                        var users = await _context.users.Where(u => u.employment_status_id == id).ToListAsync();

                        foreach (var user in users)
                        {
                            user.employment_status_id = null;
                        }

                        var entity = await _context.employment_status.FirstOrDefaultAsync(c => c.id == id);
                        if (entity != null)
                        {
                            _context.Remove(entity);
                        }
                        break;
                    }

                default:
                    {
                        object? entity = type switch
                        {
                            CategoryTypeEnum.role => await _context.roles.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.gender => await _context.genders.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.certification => await _context.certifications.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.company_award => await _context.company_awards.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.participation_process => await _context.participation_processes.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.participation_position => await _context.participation_positions.FirstOrDefaultAsync(c => c.id == id),
                            CategoryTypeEnum.report_type => await _context.report_types.FirstOrDefaultAsync(c => c.id == id),
                            _ => null
                        };

                        if (entity != null)
                        {
                            _context.Remove(entity);
                        }
                        break;
                    }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasPermission(long id, CategoryTypeEnum? type)
        {
            switch (type)
            {
                case CategoryTypeEnum.position:
                    {
                        var inUsed = await _context.users.AnyAsync(u => u.position_id == id && u.deleted_at != null);
                        return !inUsed;
                    }
                case CategoryTypeEnum.employment_type:
                    {
                        var inUsed = await _context.users.AnyAsync(u => u.employment_type_id == id && u.deleted_at != null);
                        return !inUsed;
                    }
                case CategoryTypeEnum.employment_status:
                    {
                        var inUsed = await _context.users.AnyAsync(u => u.employment_status_id == id && u.deleted_at != null);
                        return !inUsed;
                    }
                case CategoryTypeEnum.certification:
                    {
                        var inUsed = await _context.user_certification.AnyAsync(c => c.certification_id == id);
                        return !inUsed;
                    }
                case CategoryTypeEnum.company_award:
                    {
                        var inUsed = await _context.user_company_award.AnyAsync(a => a.company_award_id == id);
                        return !inUsed;
                    }
                case CategoryTypeEnum.participation_process:
                    {
                        var inUsed = await _context.project_participation_process.AnyAsync(p => p.participation_process_id == id);
                        return !inUsed;
                    }
                case CategoryTypeEnum.participation_position:
                    {
                        var inUsed = await _context.project_participation_position.AnyAsync(p => p.participation_position_id == id);
                        return !inUsed;
                    }
                case CategoryTypeEnum.report_type:
                    {
                        var inUsed = await _context.reports.AnyAsync(r => r.report_type_id == id);
                        return !inUsed;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
