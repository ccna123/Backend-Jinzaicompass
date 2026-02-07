using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Skill;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.SkillService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : BaseController
    {
        private readonly ILog _log;
        private readonly ISkillService _skillService;
        
        public SkillController(
            ISkillService skillService
        )
        {
            _log = LogManager.GetLogger(typeof(SkillController));
            _skillService = skillService;
        }

        [Authorize]
        [HttpGet("tree")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetTree()
        {
            try
            {
                var data = await _skillService.GetTree();
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
        [HttpGet("experience-job")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetExperienceJob()
        {
            try
            {
                var data = await _skillService.GetExperienceJob();
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
        [HttpGet("experience-field")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetExperienceFieldByJob([FromQuery] SkillParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _skillService.GetExperienceFieldByJob(request.ids);
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
        [HttpGet("experience-area")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetExperienceAreaByField([FromQuery] SkillParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _skillService.GetExperienceAreaByField(request.ids);
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
        [HttpGet("specific-skill")]
        [AuthorizePermission("CATEGORY01")]
        public async Task<ActionResult<BaseResponse>> GetSpecificSkillByArea([FromQuery] SkillParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _skillService.GetSpecificSkillByArea(request.ids);
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
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateSkillDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var checkNameExist = await _skillService.CheckNameExistAsync(request);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            if (request.type != SkillTypeEnum.experience_job && request.experience_job_id == null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: CategoryResource.ExperienceJobRequired);
            }

            if ((request.type == SkillTypeEnum.experience_area || request.type == SkillTypeEnum.specific_skill) && request.experience_field_id == null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: CategoryResource.ExperienceFieldRequired);
            }

            if (request.type == SkillTypeEnum.specific_skill && request.experience_area_id == null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: CategoryResource.ExperienceAreaRequired);
            }

            if (request.experience_job_id != null)
            {
                var department = await _skillService.FindById(request.experience_job_id.Value, SkillTypeEnum.experience_job);
                if (department is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            if (request.experience_field_id != null)
            {
                var division = await _skillService.FindById(request.experience_field_id.Value, SkillTypeEnum.experience_field);
                if (division is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            if (request.experience_area_id != null)
            {
                var division = await _skillService.FindById(request.experience_area_id.Value, SkillTypeEnum.experience_area);
                if (division is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            try
            {
                await _skillService.Create(request);
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
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdateSkillDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var category = await _skillService.FindById(id, request.type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }
            if (!category.delete_flag)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ServerResource.Forbidden);
            }

            var checkNameExist = await _skillService.CheckNameExistAsync(request, update: true, category.id);
            if (checkNameExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ApiResource.CategoryNameExist);
            }

            try
            {
                await _skillService.Update(category.id, request);
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
        public async Task<ActionResult<BaseResponse>> Delete(long id, [FromQuery] SkillTypeEnum type)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(400, ErrorDetails: ModelState);
            }

            var category = await _skillService.FindById(id, type);
            if (category is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
            }
            if (!category.delete_flag)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ServerResource.Forbidden);
            }

            bool hasCheck = await _skillService.HasCheck(category.id, type);
            if (hasCheck)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, Message: ApiResource.SkillHasMember);
            }

            try
            {
                await _skillService.Delete(category.id, type);
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
