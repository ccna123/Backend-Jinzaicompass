using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Dtos.Organization;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.OrganizationService
{
    public class OrganizationService : IOrganizationService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public OrganizationService(
            DataContext context, 
            IConfiguration configuration, 
            IMapper mapper,
            IAuthService authService
        ) {
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }
        public async Task<List<DepartmentDto>> GetTree()
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var departments = await _context.departments
                .Where(d => d.tenant_id == tenantId)
                .Include(d => d.Divisions)
                    .ThenInclude(div => div.Groups)
                .Include(d => d.Groups)
                .AsNoTracking()
                .ToListAsync();

            var result = departments.Select(d => new DepartmentDto
            {
                id = d.id,
                name = d.name,
                delete_flag = d.delete_flag ?? false,

                children = d.Divisions.Select(div => new DivisionDto
                {
                    id = div.id,
                    name = div.name,
                    delete_flag = div.delete_flag ?? false,
                    department_id = div.department_id,

                    children = div.Groups.Select(g => new GroupDto
                    {
                        id = g.id,
                        name = g.name,
                        delete_flag = g.delete_flag ?? false,
                        department_id = g.department_id,
                        division_id = g.division_id
                    }).Cast<OrganizationDto>().ToList()
                })
                .Cast<OrganizationDto>()
                .Concat(
                    d.Groups
                        .Where(g => g.division_id == null)
                        .Select(g => new GroupDto
                        {
                            id = g.id,
                            name = g.name,
                            delete_flag = g.delete_flag ?? false,
                            department_id = g.department_id,
                            division_id = g.division_id
                        }).Cast<OrganizationDto>()
                )
                .ToList()
            }).ToList();

            return result;
        }

        public async Task<DepartmentDto?> GetTreeDepartment(long? departmentId)
        {
            long depId = (long)_authService.GetAccountId("Department");

            if (departmentId == null)
            {
                departmentId = depId;
            }

            var department = await _context.departments
                .Include(d => d.Divisions)
                    .ThenInclude(div => div.Groups)
                .Include(d => d.Groups)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.id == departmentId);

            if (department == null)
            {
                return null;
            }

            var result = new DepartmentDto
            {
                id = department.id,
                name = department.name,
                delete_flag = department.delete_flag ?? false,
            };

            var divisions = department.Divisions.Select(div => new DivisionDto
            {
                id = div.id,
                name = div.name,
                delete_flag = div.delete_flag ?? false,
                department_id = div.department_id,
                children = div.Groups.Select(g => new GroupDto
                {
                    id = g.id,
                    name = g.name,
                    delete_flag = g.delete_flag ?? false,
                    department_id = g.department_id,
                    division_id = g.division_id
                }).Cast<OrganizationDto>().ToList()
            }).Cast<OrganizationDto>().ToList();

            var groups = department.Groups
                .Where(g => g.division_id == null)
                .Select(g => new GroupDto
                {
                    id = g.id,
                    name = g.name,
                    delete_flag = g.delete_flag ?? false,
                    department_id = g.department_id,
                    division_id = g.division_id
                }).Cast<OrganizationDto>().ToList();

           
            result.children = divisions.Concat(groups).ToList();

            return result;
        }

        public async Task<List<OrganizationDto>> GetDepartment()
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.departments
               .Where(d => d.tenant_id == tenantId)
               .AsNoTracking()
               .OrderByDescending(d => d.updated_at)
               .Select(d => new OrganizationDto
               {
                   id = d.id,
                   name = d.name
               })
               .ToListAsync();
        }

        public async Task<List<OrganizationDto>> GetDivisionByDepartment(long id)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.divisions
                .Where(d => d.department_id == id)
                .Where(d => d.tenant_id == tenantId)
                .AsNoTracking()
                .OrderByDescending(d => d.updated_at)
                .Select(d => new OrganizationDto
                {
                    id = d.id,
                    name = d.name
                })
                .ToListAsync();
        }

        public async Task<List<OrganizationDto>> GetGroupByDivision(long id)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.groups
                .Where(d => d.division_id == id)
                .Where(d => d.tenant_id == tenantId)
                .AsNoTracking()
                .OrderByDescending(d => d.updated_at)
                .Select(d => new OrganizationDto
                {
                    id = d.id,
                    name = d.name
                })
                .ToListAsync();
        }

        public async Task<List<OrganizationDto>> GetGroupByDepartment(long id)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            return await _context.groups
                .Where(d => d.department_id == id)
                .Where(d => d.tenant_id == tenantId)
                .AsNoTracking()
                .OrderByDescending(d => d.updated_at)
                .Select(d => new OrganizationDto
                {
                    id = d.id,
                    name = d.name
                })
                .ToListAsync();
        }

        public async Task Create(CreateOrganizationDto request)
        {
            object? newEntity = request.type switch
            {
                OrganizationTypeEnum.department => new Department { name = request.name },
                OrganizationTypeEnum.division => new Division { name = request.name, department_id = request.department_id ?? 1, delete_flag = true },
                OrganizationTypeEnum.group => new Group { name = request.name, department_id = request.department_id ?? 1, division_id = request.division_id, delete_flag = true },
                _ => null
            };

            if (newEntity != null)
            {
                _context.Add(newEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Update(long id, UpdateOrganizationDto request)
        {
            object? entity = request.type switch
            {
                OrganizationTypeEnum.department => await _context.departments.FindAsync(id),
                OrganizationTypeEnum.division => await _context.divisions.FindAsync(id),
                OrganizationTypeEnum.group => await _context.groups.FindAsync(id),
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

        public async Task Delete(long id, OrganizationTypeEnum? type)
        {
            switch (type)
            {
                case OrganizationTypeEnum.department:
                    var department = await _context.departments
                        .Include(d => d.Divisions)
                        .ThenInclude(div => div.Groups)
                        .FirstOrDefaultAsync(d => d.id == id);

                    if (department != null)
                    {
                        // Update user if used department id
                        var users = await _context.users
                           .Where(u => u.department_id == department.id)
                           .ToListAsync();

                        foreach (var user in users)
                        {
                            user.department_id = null;
                            user.division_id = null;
                            user.group_id = null;
                        }
                        // Delete group > division > department
                        foreach (var div in department.Divisions.ToList())
                        {
                            _context.groups.RemoveRange(div.Groups);
                        }

                        _context.divisions.RemoveRange(department.Divisions);

                        _context.departments.Remove(department);

                        await _context.SaveChangesAsync();
                    }
                    break;

                case OrganizationTypeEnum.division:
                    var division = await _context.divisions
                        .Include(div => div.Groups)
                        .FirstOrDefaultAsync(div => div.id == id);

                    if (division != null)
                    {
                        // Update user if used division id
                        var users = await _context.users
                            .Where(u => u.division_id == division.id)
                            .ToListAsync();

                        foreach (var user in users)
                        {
                            user.division_id = null;
                            user.group_id = null;
                        }
                        // Delete group > division
                        _context.groups.RemoveRange(division.Groups);

                        _context.divisions.Remove(division);

                        await _context.SaveChangesAsync();
                    }
                    break;

                case OrganizationTypeEnum.group:
                    var group = await _context.groups.FirstOrDefaultAsync(g => g.id == id);

                    if (group != null)
                    {
                        // Update user if used group id
                        var users = await _context.users
                            .Where(u => u.group_id == group.id)
                            .ToListAsync();

                        foreach (var user in users)
                        {
                            user.group_id = null;
                        }
                        // Delete group
                        _context.groups.Remove(group);
                        await _context.SaveChangesAsync();
                    }
                    break;
        }
        }

        public async Task<bool> CheckNameExistAsync(BaseOrganizationDto request, bool update = false, long id = 0)
        {
            return request.type switch
            {
                OrganizationTypeEnum.department => await CheckNameExist(_context.departments, request.name, update, id),
                OrganizationTypeEnum.division => await CheckNameExist(_context.divisions, request.name, update, id),
                OrganizationTypeEnum.group => await CheckNameExist(_context.groups, request.name, update, id),
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

        public async Task<CategoryDto?> FindById(long id, OrganizationTypeEnum? type)
        {
            return type switch
            {
                OrganizationTypeEnum.department => _mapper.Map<CategoryDto>(await _context.departments.FirstOrDefaultAsync(c => c.id == id)),
                OrganizationTypeEnum.division => _mapper.Map<CategoryDto>(await _context.divisions.FirstOrDefaultAsync(c => c.id == id)),
                OrganizationTypeEnum.group => _mapper.Map<CategoryDto>(await _context.groups.FirstOrDefaultAsync(c => c.id == id)),
                _ => null
            };
        }

        public async Task<bool> HasCheckUser(long id, OrganizationTypeEnum? type)
        {
            bool hasCheck = type switch
            {
                OrganizationTypeEnum.department => await _context.users.AnyAsync(u => u.department_id == id && u.deleted_at == null),
                OrganizationTypeEnum.division => await _context.users.AnyAsync(u => u.division_id == id && u.deleted_at == null),
                OrganizationTypeEnum.group => await _context.users.AnyAsync(u => u.group_id == id && u.deleted_at == null),
                _ => false
            };

            return hasCheck;
        }
    }
}
