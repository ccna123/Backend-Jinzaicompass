
using log4net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Services.UserService;
using SystemBrightSpotBE.Services.PermissionService;
using SystemBrightSpotBE.Services.CategoryService;
using SystemBrightSpotBE.Services.CompanyService;
using SystemBrightSpotBE.Services.OrganizationService;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Dtos.User;
using SystemBrightSpotBE.Dtos.UserStatusHistory;
using SystemBrightSpotBE.Dtos.UserSkill;
using SystemBrightSpotBE.Dtos.UserManager;
using SystemBrightSpotBE.PDFDocument;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SystemBrightSpotBE.Dtos.Organization;

namespace SystemBrightSpotBE.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly ILog _log;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly ICompanyService _companyService;
        private readonly IOrganizationService _organizationService;

        public UserController(
            IUserService userService,
            IPermissionService permissionService,
            ICategoryService categoryService,
            ICompanyService companyService,
            IOrganizationService organizationService
        )
        {
            _log = LogManager.GetLogger(typeof(UserController));
            _userService = userService;
            _permissionService = permissionService;
            _categoryService = categoryService;
            _companyService = companyService;
            _organizationService = organizationService;
        }

        [Authorize]
        [HttpPost]
        [AuthorizePermission("USER01")]
        public async Task<ActionResult<BaseResponse>> GetAll([FromBody] ListUserParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var users = await _userService.GetAll(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: users);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("USER02")]
        public async Task<ActionResult<BaseResponse>> Create([FromForm] AddUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var checkEmailExists = await _userService.CheckExistAsync("mail", request.email);
            if (checkEmailExists)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.EmailExist);
            }

            var checkCodeExists = await _userService.CheckExistAsync("code", request.code);
            if (checkCodeExists)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CodeExist);
            }

            var status = _permissionService.checkGrantPermission(request.role_id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            if (!string.IsNullOrWhiteSpace(request.status_history_json))
            {
                request.status_history = JsonConvert.DeserializeObject<List<StatusHistoryDto>>(request.status_history_json) ?? new();
            }

            try
            {
                await _userService.Create(request);
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
        [AuthorizePermission("USER04")]
        public async Task<ActionResult<BaseResponse>> GetDetail(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                return JJsonResponse(StatusCodes.Status200OK, Data: user);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        [AuthorizePermission("USER03")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromForm] UpdateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var checkCodeExists = await _userService.CheckExistAsync("code", request.code, true, id);
            if (checkCodeExists)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CodeExist);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            if (!string.IsNullOrWhiteSpace(request.status_history_json))
            {
                request.status_history = JsonConvert.DeserializeObject<List<StatusHistoryDto>>(request.status_history_json) ?? new();
            }

            try
            {
                bool removeAvatar = Request.Headers["Remove-Avatar"].FirstOrDefault() == "1";
                await _userService.Update(id, request, removeAvatar);
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
        [AuthorizePermission("USER06")]
        public async Task<ActionResult<BaseResponse>> Delete(long id)
        {
            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _userService.Delete(id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/general")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetGeneral(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                var user = await _userService.FindByIdGeneral(id);
                if (user is null || user.deleted_at != null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
                }
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: user);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/general")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> UpdateGeneral(long id, [FromForm] UpdateUserGeneralDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }
           
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                var user = await _userService.FindByIdGeneral(id);
                if (user is null || user.deleted_at != null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
                }

                bool removeAvatar = Request.Headers["Remove-Avatar"].FirstOrDefault() == "1";
                await _userService.UpdateGeneral(user.id, request, removeAvatar);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("import")]
        [AuthorizePermission("USER04")]
        public async Task<ActionResult<BaseResponse>> Import([FromForm] ImportUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var result = await _userService.Import(request.file);

                if (result.ErrorDetails != null && result.ErrorMessage != null)
                {
                    return JJsonResponse(StatusCodes.Status400BadRequest, Message: result.Message, ErrorMessage: result.ErrorMessage, ErrorDetails: result.ErrorDetails);
                }
                else
                {
                    return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/report")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetReport(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var report = await _userService.GetReportByTargetId(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: report);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/skill")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetSkill(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var skills = await _userService.GetSkillByTargetId(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: skills);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/skill")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> UpdateSkill(long id, [FromBody] UpdateUserSkillDto request)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            if (!String.IsNullOrEmpty(request.experience_job))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_job, SkillTypeEnum.experience_job);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_job");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_field))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_field, SkillTypeEnum.experience_field);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_field");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_area))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_area, SkillTypeEnum.experience_area);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_area");
                }
            }

            if (!String.IsNullOrEmpty(request.specific_skill))
            {
                var exist = await _userService.CheckExistSkillAsync(request.specific_skill, SkillTypeEnum.specific_skill);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "specific_skill");
                }
            }

            try
            {
                await _userService.UpdateSkillByTargetId(id, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/certification")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetCertification(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var report = await _userService.GetCertificationByTargetId(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: report);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("{id}/certification/create")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> CreateUserCertification(long id, [FromBody] CreateUserCertificationDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var category = await _categoryService.FindById(request.certification_id, CategoryTypeEnum.certification);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }

            try
            {
                await _userService.CreateCertificationByTargetId(id, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/certification/{certificationId}")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> DeleteUserCertification(long id, long certificationId)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                await _userService.DeleteCertificationByTargetId(certificationId);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }


        [Authorize]
        [HttpGet("{id}/company-award")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetCompanyAward(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var report = await _userService.GetCompanyAwardByTargetId(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: report);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("{id}/company-award/create")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> CreateUserCompanyAward(long id, [FromBody] CreateUserCompanyAwardDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var category = await _categoryService.FindById(request.company_award_id, CategoryTypeEnum.company_award);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }

            try
            {
                await _userService.CreateCompanyAwardByTargetId(id, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/company-award/{awardId}")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> DeleteCompanyAward(long id, long awardId)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                await _userService.DeleteCompanyAwardByTargetId(awardId);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/project")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetProject(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var project = await _userService.GetProjectByTargetId(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: project);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("{id}/project/create")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> CreateProject(long id, [FromBody] CreateUserProjectDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var company = await _companyService.FindById(request.company_id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }

            if (!String.IsNullOrEmpty(request.experience_job))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_job, SkillTypeEnum.experience_job);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_job");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_field))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_field, SkillTypeEnum.experience_field);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_field");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_area))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_area, SkillTypeEnum.experience_area);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_area");
                }
            }

            if (!String.IsNullOrEmpty(request.specific_skill))
            {
                var exist = await _userService.CheckExistSkillAsync(request.specific_skill, SkillTypeEnum.specific_skill);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "specific_skill");
                }
            }

            if (!String.IsNullOrEmpty(request.participation_position))
            {
                var exist = await _userService.CheckExistParticipationAsync(request.participation_position, ParticipationTypeEnum.participation_position);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "participation_position");
                }
            }

            if (!String.IsNullOrEmpty(request.participation_process))
            {
                var exist = await _userService.CheckExistParticipationAsync(request.participation_process, ParticipationTypeEnum.participation_process);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "participation_process");
                }
            }

            try
            {
                await _userService.CreateProjectByTargetId(id, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/project/{projectId}/update")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> UpdateProject(long id, long projectId, [FromBody] UpdateUserProjectDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var company = await _companyService.FindById(request.company_id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }

            var project = await _userService.FindByProjectId(projectId);
            if (project is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ProjectNotFound);
            }

            if (!String.IsNullOrEmpty(request.experience_job))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_job, SkillTypeEnum.experience_job);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_job");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_field))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_field, SkillTypeEnum.experience_field);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_field");
                }
            }

            if (!String.IsNullOrEmpty(request.experience_area))
            {
                var exist = await _userService.CheckExistSkillAsync(request.experience_area, SkillTypeEnum.experience_area);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "experience_area");
                }
            }

            if (!String.IsNullOrEmpty(request.specific_skill))
            {
                var exist = await _userService.CheckExistSkillAsync(request.specific_skill, SkillTypeEnum.specific_skill);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "specific_skill");
                }
            }

            if (!String.IsNullOrEmpty(request.participation_position))
            {
                var exist = await _userService.CheckExistParticipationAsync(request.participation_position, ParticipationTypeEnum.participation_position);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "participation_position");
                }
            }

            if (!String.IsNullOrEmpty(request.participation_process))
            {
                var exist = await _userService.CheckExistParticipationAsync(request.participation_process, ParticipationTypeEnum.participation_process);
                if (!exist)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound, ErrorMessage: "participation_process");
                }
            }

            try
            {
                await _userService.UpdateProjectByTargetId(projectId, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/project/{projectId}/delete")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> DeleteProject(long id, long projectId)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var project = await _userService.FindByProjectId(projectId);
            if (project is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ProjectNotFound);
            }

            try
            {
                await _userService.DeleteProjectByTargetId(projectId);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/skill-sheet")]
        [AuthorizePermission("USER05")]
        public async Task<ActionResult<BaseResponse>> GetSkillSheet(long id)
        {
            var status = await _permissionService.checkAccessPermissionAsync(id);
            if (!status)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var user = await _userService.FindById(id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var data = await _userService.GetSkillSheetByTargetId(id);
                // Set license
                QuestPDF.Settings.License = LicenseType.Community;
                // Generate PDF
                var document = new SkillSheetDoc(data);
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", data.full_name + "_スキルシート.pdf");
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("member")]
        public async Task<ActionResult<BaseResponse>> GetUserMember([FromQuery] UserMemberParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            if (request.department_id > 0)
            {
                var department = await _organizationService.FindById(request.department_id, OrganizationTypeEnum.department);
                if (department is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            try
            {
                var users = await _userService.GetMember(request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: users);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("target")]
        public async Task<ActionResult<BaseResponse>> GetTargetUsers()
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var users = await _userService.GetTargetUsers();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: users);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("by-department")]
        public async Task<ActionResult<BaseResponse>> GetUserByDepartment([FromQuery] DepartmentParamDto request)
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
                var users = await _userService.GetUserByDepartment(request.department_id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: users);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }
    }
}
