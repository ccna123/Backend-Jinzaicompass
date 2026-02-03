namespace SystemBrightSpotBE.Services.PermissionService
{
    public interface IPermissionService
    {
        Task<bool> checkAccessPermissionAsync(long authorized_role_id);
        bool checkGrantPermission(long authorized_id);
        bool checkAccessCategoryPermission(string module);
        Task<bool> checkPlanPermissionAsync(long authorized_id);
    }
}