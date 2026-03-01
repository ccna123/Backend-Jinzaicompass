using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAll([FromQuery] CategoryParamDto request);
        Task<CategoryDto?> FindById(long id, CategoryTypeEnum? type);
        Task<bool> CheckNameExistAsync(AddCategoryDto request, bool update = false, long id = 0);
        Task<bool> HasPermission(long id, CategoryTypeEnum? type);
        Task Create(AddCategoryDto request);
        Task Update(long id, AddCategoryDto request);
        Task Delete(long id, CategoryTypeEnum? type);
    }
}
