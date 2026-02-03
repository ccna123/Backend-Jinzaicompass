using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using SystemBrightSpotBE.Dtos.Auth;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private DataContext _context;
        private IAuthService _authService;
        public AuthController(
            DataContext context,
            IAuthService authService
        )
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public ActionResult<BaseResponse> Login(LoginDto auth)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var account = _context.users
                .Include(u => u.Tenant)
                .Where(u => u.email == auth.email)
                .Where(u => u.deleted_at == null)
                .FirstOrDefault();

            if (account is null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.EmailNotExist);
            }

            if (account.temp_password_used == true && account.temp_password_expired_at < DateTime.Now)
            {
                return JJsonResponse(StatusCodes.Status419AuthenticationTimeout, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.TempPasswordExpired);
            }

            if (account.role_id != (long)RoleEnum.SUPPER_ADMIN)
            {
                if (account.Tenant == null)
                {
                    return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantNotFound);
                }
                else
                {
                    switch (account.Tenant.status)
                    {
                        case (long)TenantStatusEnum.IN_PREVIEW:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantIsInpreview);
                        case (long)TenantStatusEnum.SCHEDULED:
                            return JJsonResponse(
                                StatusCodes.Status400BadRequest,
                                ErrorMessage: ServerResource.BadRequest,
                                ErrorDetails: string.Format(
                                    AuthResource.TenantIsScheduled,
                                    account.Tenant.start_date
                                )
                            );
                        case (long)TenantStatusEnum.SUSPENDED:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantIsSuspended);
                        case (long)TenantStatusEnum.EXPIRED:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantExpried);
                        default:
                            break;
                    }
                }
            }

            var checkLogin = _authService.Login(auth.email, auth.password);
            if (!checkLogin)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.AccoutNotCorrect);
            }

            // Get auth token
            var token = _authService.GetToken(account);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: new BaseTokenResponse(token, account, tokenHandler.ReadToken(token).ValidTo));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<BaseResponse>> ResetPassword(ResetPasswordDto auth)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var account = _context.users.Include(u => u.Tenant).Where(u => u.email == auth.email).FirstOrDefault();
            if (account is null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.EmailNotExist);
            }

            if (account.role_id != (long)RoleEnum.SUPPER_ADMIN)
            {
                if (account.Tenant == null)
                {
                    return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantNotFound);
                }
                else
                {
                    switch (account.Tenant.status)
                    {
                        case (long)TenantStatusEnum.IN_PREVIEW:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantIsInpreview);
                        case (long)TenantStatusEnum.SCHEDULED:
                            return JJsonResponse(
                                StatusCodes.Status400BadRequest,
                                ErrorMessage: ServerResource.BadRequest,
                                ErrorDetails: string.Format(
                                    AuthResource.TenantIsScheduled,
                                    account.Tenant.start_date
                                )
                            );
                        case (long)TenantStatusEnum.SUSPENDED:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantIsSuspended);
                        case (long)TenantStatusEnum.EXPIRED:
                            return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: AuthResource.TenantExpried);
                        default:
                            break;
                    }
                }
            }

            if (account.temp_password_used == true && account.temp_password_expired_at > DateTime.Now)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.TempPasswordNoExpired);
            }

            bool checkResetPassword = await _authService.ResetPassword(account.id);
            if (!checkResetPassword)
            {
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }

            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<BaseResponse>> ChangePassword(ChangePasswordDto auth)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var accountId = _authService.GetAccountId("Id");
            var account = _context.users.Where(u => u.id == accountId).FirstOrDefault();
            if (account is null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.AccoutNotExist);
            }

            bool checkChangePassword = await _authService.ChangePassword(account.id, auth.new_password);
            if (!checkChangePassword)
            {
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }

            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
        }

        [Authorize]
        [HttpGet("logout")]
        public ActionResult<BaseResponse> Logout()
        {
            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
        }
    }
}
