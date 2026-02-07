using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Organization;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.OrganizationService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : BaseController
    {
        private readonly ILog _log;
        private readonly IOrganizationService _organizationService;
        
        public OrganizationController(
            IOrganizationService organizationService
        )
        {
            _log = LogManager.GetLogger(typeof(OrganizationController));
            _organizationService = organizationService;
        }

        [Authorize]
        [HttpGet("tree")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetTree()
        {
            try
            {
                var data = await _organizationService.GetTree();
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
        [HttpGet("tree/department")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetTreeDepartment([FromQuery] DepartmentParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            if (request.department_id != null && request.department_id > 0)
            {
                var department = await _organizationService.FindById((long)request.department_id, OrganizationTypeEnum.department);
                if (department is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            try
            {
                var treeDepartment = await _organizationService.GetTreeDepartment(request.department_id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: treeDepartment);
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
        [HttpGet("department")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetDepartment()
        {
            try
            {
                var data = await _organizationService.GetDepartment();
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
        [HttpGet("division")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetDivisionByDepartment([FromQuery] OrganizationParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _organizationService.GetDivisionByDepartment(request.id);
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
        [HttpGet("group")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetGroupByDivision([FromQuery] OrganizationParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _organizationService.GetGroupByDivision(request.id);
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
        [HttpGet("group-department")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetGroupByDepartment([FromQuery] OrganizationParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _organizationService.GetGroupByDepartment(request.id);
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
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateOrganizationDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var checkNameExist = await _organizationService.CheckNameExistAsync(request);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            if ((request.type == OrganizationTypeEnum.division || request.type == OrganizationTypeEnum.group) && request.department_id == null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: CategoryResource.DepartmentRequired);
            }

            if (request.department_id != null)
            {
                var department = await _organizationService.FindById(request.department_id.Value, OrganizationTypeEnum.department);
                if (department is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            if (request.division_id != null)
            {
                var division = await _organizationService.FindById(request.division_id.Value, OrganizationTypeEnum.division);
                if (division is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            try
            {
                await _organizationService.Create(request);
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
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdateOrganizationDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var category = await _organizationService.FindById(id, request.type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }
            if (!category.delete_flag)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ServerResource.Forbidden);
            }

            var checkNameExist = await _organizationService.CheckNameExistAsync(request, update: true, category.id);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            try
            {
                await _organizationService.Update(category.id, request);
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
        public async Task<ActionResult<BaseResponse>> Delete(long id, [FromQuery] OrganizationTypeEnum type)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var category = await _organizationService.FindById(id, type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }
            if (!category.delete_flag)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ServerResource.Forbidden);
            }

            bool hasCheck = await _organizationService.HasCheckUser(category.id, type);
            if (hasCheck)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ApiResource.OrganizationHasMember);
            }

            try
            {
                await _organizationService.Delete(category.id, type);
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
