using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Company;
using SystemBrightSpotBE.Filters;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.CompanyService
{
    public class CompanyService : ICompanyService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public CompanyService(
            DataContext context,
            IMapper mapper,
            IAuthService authService
        ) {
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<List<CompanyDto>> GetAll()
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var companies = await _context.companies
                .Where(c => c.tenant_id == tenantId)
                .OrderByDescending(c => c.updated_at)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<CompanyDto>>(companies);
        }

        public async Task<PagedResponse<List<CompanyDto>>> GetPaginate(CompanyParamDto request)
        {
            var paginationFilter = new PaginationFilter(request.page, request.size);
            var sortFilter = new SortFilter(request.order, request.column);
            long tenantId = _authService.GetAccountId("Tenant");

            var dataQuery = _context.companies.Where(c => c.tenant_id == tenantId).AsQueryable();

            var totalRecords = await dataQuery.CountAsync();
            var data = dataQuery
               .Select(c => new CompanyDto
               {
                   id = c.id,
                   name = c.name,
                   phone = c.phone,
                   address = c.address,
                   updated_at = c.updated_at ?? DateTime.MinValue,
               }).AsQueryable();

            //Sort
            if (!string.IsNullOrEmpty(sortFilter.SortColumn))
            {
                if (sortFilter.SortBy == "desc")
                {
                    data = data.OrderByDescending(g => EF.Property<string>(g, sortFilter.SortColumn));
                }
                else
                {
                    data = data.OrderBy(g => EF.Property<string>(g, sortFilter.SortColumn));
                }
            }

            data = data.OrderByDescending(g => g.updated_at);

            data = data.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize).Take(paginationFilter.PageSize);

            return PaginationHelper.CreatePagedResponse(await data.ToListAsync(), paginationFilter, totalRecords, null);
        }

        public async Task Create(CreateCompanyDto request)
        {
            var company = _mapper.Map<Company>(request);
            company.updated_at = DateTime.Now;

            _context.companies.Add(company);
            await _context.SaveChangesAsync();
        }

        public async Task Update(long id, UpdateCompanyDto request)
        {
            var company = await _context.companies.FindAsync(id);
            if (company == null)
            {
                throw new Exception("Company not found");
            }

            _mapper.Map(request, company);
            company.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task Delete(long id)
        {
            var company = await _context.companies.FindAsync(id);
            if (company == null)
            {
                throw new Exception("Company not found");
            }

            _context.companies.Remove(company);
            await _context.SaveChangesAsync();
        }

        public async Task<CompanyDto> FindById(long id)
        {
            var company = await _context.companies
                .AsNoTracking()
                .FirstOrDefaultAsync(comp => comp.id == id);

            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<bool> CheckNameExist(string value, bool update, long id)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            bool result = false;
            if (update)
            {
                result = await _context.companies.AnyAsync(u => u.name == value && u.tenant_id == tenantId && u.id != id);
            }
            else
            {
                result = await _context.companies.AnyAsync(u => u.name == value && u.tenant_id == tenantId);
            }

            return result;
        }
    }
}
