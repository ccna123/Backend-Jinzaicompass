
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Attributes
{
    public class AuthorizePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _module;
        private readonly long SystemAdmin = 1;
        private readonly long PowerUser = 2;
        private readonly long SeniorUser = 3;
        private readonly long Contributor = 4;
        private readonly long Member = 5;
        private readonly long SupperAdmin = 6;

        public AuthorizePermissionAttribute(string module)
        {
            _module = module;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var db = context.HttpContext.RequestServices.GetService<DataContext>();
            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            bool access_status = false;
            long? role = authService?.GetAccountId("Role");

            if (role == SupperAdmin)
            {
                switch (_module)
                {
                    case "TENANT01":
                    case "TENANT02":
                    case "TENANT03":
                    case "TENANT04":
                    case "TENANT05":
                    case "USER04":
                    case "USER05":
                    case "SETTING01":
                    case "MONITORINGSYSTEM01":
                    case "MONITORINGSYSTEM02":
                    case "MONITORINGSYSTEM03":
                    case "MONITORINGSYSTEM04":
                        access_status = true;
                        break;
                    default:
                        access_status = false;
                        break;
                }
            }

            if (role == SystemAdmin)
            {
                return;
            }

            if (role == PowerUser || role == SeniorUser || role == Contributor)
            {
                var noAccess = new List<string> { "CATEGORY02", "CATEGORY03", "CATEGORY04", "COMPANY02", "COMPANY03", "COMPANY04", "SETTING02" };

                if (!noAccess.Contains(_module))
                {
                    return;
                }
            }

            if (role == Member)
            {
                switch (_module)
                {
                    case "USER04":
                    case "USER05":
                    case "REPORT01":
                    case "CATEGORY01":
                    case "COMPANY01":
                    case "SETTING01":
                        access_status = true;
                        break;
                    default:
                        break;
                }
            }

            if (!access_status)
            {
                context.Result = new JsonResult(new { message = ServerResource.Forbidden }) { StatusCode = 403 };
            }
        }
    }
}