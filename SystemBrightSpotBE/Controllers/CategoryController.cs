using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.CategoryService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ILog _log;
        private readonly ICategoryService _categoryService;
        public CategoryController(
            DataContext context,
            ICategoryService categoryService
        )
        {
            _log = LogManager.GetLogger(typeof(CategoryController));
            _categoryService = categoryService;
        }

        [Authorize]
        [HttpGet()]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetAll([FromQuery] CategoryParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _categoryService.GetAll(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("CATEGORY02")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] AddCategoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var checkNameExist = await _categoryService.CheckNameExistAsync(request);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            try
            {
                await _categoryService.Create(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        [AuthorizePermission("CATEGORY03")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] AddCategoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var category = await _categoryService.FindById(id, request.type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }

            var checkNameExist = await _categoryService.CheckNameExistAsync(request, update:true, category.id);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            try
            {
                await _categoryService.Update(category.id, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/delete")]
        [AuthorizePermission("CATEGORY04")]
        public async Task<ActionResult<BaseResponse>> Delete(long id, [FromQuery] CategoryParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var category = await _categoryService.FindById(id, request.type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }
            if (!category.delete_flag)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ServerResource.Forbidden);
            }

            bool hasPermission = await _categoryService.HasPermission(category.id, request.type);
            if (!hasPermission) {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ApiResource.CategoryHasMember);
            }

            try
            {
                await _categoryService.Delete(category.id, request.type);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }
    }
}
