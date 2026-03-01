using log4net;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.UserService;

namespace SystemBrightSpotBE.Services.PermissionService
{
    public class PermissionService : IPermissionService
    {
        private readonly ILog _log;
        private IAuthService _authService;
        private IUserService _userService;

        public PermissionService(
            IAuthService authService,
            IUserService userService
        )
        {
            _log = LogManager.GetLogger(typeof(PermissionService));
            _authService = authService;
            _userService = userService;
        }

        public async Task<bool> checkAccessPermissionAsync(long authorizedId)
        {
            bool accessStatus = false;
            var roleId = _authService.GetAccountId("Role");

            if (roleId == (long)RoleEnum.SYSTEM_ADMIN)
            {
                accessStatus = true;
            }
            else
            {
                List<long> listManagedUsersId = await _userService.GetManagedUsersId();
                if (listManagedUsersId.Contains(authorizedId))
                {
                    accessStatus = true;
                }
            }

            return accessStatus;
        }

        public bool checkGrantPermission(long authorizedId)
        {
            bool grantStatus = false;
            var roleId = _authService.GetAccountId("Role");

            switch (roleId)
            {
                case (long)RoleEnum.SYSTEM_ADMIN:
                    grantStatus = true;
                    break;
                case (long)RoleEnum.POWER_USER:
                    if (authorizedId != (long)RoleEnum.SYSTEM_ADMIN && authorizedId != (long)RoleEnum.POWER_USER)
                    {
                        grantStatus = true;
                    }
                    break;
                case (long)RoleEnum.SENIOR_USER:
                    if (authorizedId == (long)RoleEnum.CONTRIBUTOR || authorizedId == (long)RoleEnum.MEMBER)
                    {
                        grantStatus = true;
                    }
                    break;
                case (long)RoleEnum.CONTRIBUTOR:
                    if (authorizedId == (long)RoleEnum.MEMBER)
                    {
                        grantStatus = true;
                    }
                    break;
                default:
                    break;
            }

            return grantStatus;
        }

        public bool checkAccessCategoryPermission(string module)
        {
            bool accessStatus = false;
            var roleId = _authService.GetAccountId("Role");

            if (roleId == (long)RoleEnum.SYSTEM_ADMIN)
            {
                accessStatus = true;
            }
            else
            {
                switch (module)
                {
                    case "CATEGORY01":
                    case "COMPANY01":
                        if (roleId == (long)RoleEnum.POWER_USER ||
                            roleId == (long)RoleEnum.SENIOR_USER ||
                            roleId == (long)RoleEnum.CONTRIBUTOR)
                        { accessStatus = true; }
                        break;
                    case "CATEGORY02":
                    case "CATEGORY03":
                    case "CATEGORY04":
                    case "COMPANY02":
                    case "COMPANY03":
                    case "COMPANY04":
                    case "COMPANY05":
                        accessStatus = false;
                        break;
                    default:
                        break;
                }
            }

            return accessStatus;
        }

        public async Task<bool> checkPlanPermissionAsync(long authorizedId)
        {
            bool accessStatus = false;
            var roleId = _authService.GetAccountId("Role");

            if (roleId == (long)RoleEnum.SYSTEM_ADMIN)
            {
                accessStatus = true;
            }
            else
            {
                List<long> listManagedUsersId = await _userService.GetManagedUsersId();
                if (!listManagedUsersId.Contains(authorizedId))
                {
                    accessStatus = false;
                }
            }

            return accessStatus;
        }

    }
}


