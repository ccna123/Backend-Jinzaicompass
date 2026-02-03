using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Contact;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.ContactServices;

namespace SystemBrightSpotBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{
    private readonly ILog _log;
    private readonly IContactService _contactService;
    public ContactController(IContactService contactService)
    {
        _log = LogManager.GetLogger(typeof(ContactController));
        _contactService = contactService;
    }


    [Authorize]
    [HttpPost("")]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateContactDto request)
    {
        if (!ModelState.IsValid)
        {
            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
        }

        try
        {
            await _contactService.Create(request);
            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
        }
        catch (Exception ex)
        {
            _log.Error(ex.ToString());
            return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
        }
    }
}