using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Company;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.CompanyService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : BaseController
    {
        private readonly ILog _log;
        private readonly ICompanyService _companyService;
        public CompanyController(
            ICompanyService companyService
        )
        {
            _log = LogManager.GetLogger(typeof(CompanyController));
            _companyService = companyService;
        }

        [Authorize]
        [HttpGet()]
        [AuthorizePermission("COMPANY01")]
        public async Task<ActionResult<BaseResponse>> GetAll()
        {
            try
            {
                var companies = await _companyService.GetAll();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: companies);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("search")]
        [AuthorizePermission("COMPANY01")]
        public async Task<ActionResult<BaseResponse>> GetPaginate([FromQuery] CompanyParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var companies = await _companyService.GetPaginate(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: companies);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("COMPANY02")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateCompanyDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            bool checkNameExist = await _companyService.CheckNameExist(request.name);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CompanyNameExist);
            }

            try
            {
                await _companyService.Create(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        [AuthorizePermission("COMPANY03")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdateCompanyDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var company = await _companyService.FindById(id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }

            bool checkNameExist = await _companyService.CheckNameExist(request.name, update: true, id);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CompanyNameExist);
            }

            try
            {
                await _companyService.Update(id, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/delete")]
        [AuthorizePermission("COMPANY04")]
        public async Task<ActionResult<BaseResponse>> Delete(long id)
        {
            var company = await _companyService.FindById(id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }

            try
            {
                await _companyService.Delete(id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/detail")]
        [AuthorizePermission("COMPANY05")]
        public async Task<ActionResult<BaseResponse>> Detail(long id)
        {
            var company = await _companyService.FindById(id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }
            
            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: company);
        }
    }
}
