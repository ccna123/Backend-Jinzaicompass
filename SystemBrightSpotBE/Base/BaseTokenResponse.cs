using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE.Base
{
    public class BaseTokenResponse
    {
        public string token { get; set; }
        public long id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string code { get; set; }
        public long? role_id { get; set; }
        public long? department_id { get; set; }
        public long? division_id { get; set; }  
        public long? group_id { get; set; }
        public long? tenant_id { get; set; }
        public bool is_tenant_created { get; set; }
        public bool? temp_password_used { get; set; }
        public DateTime expiredAt { get; set; }

        public BaseTokenResponse(
            string tokenHandler,
            User User,
            DateTime expired
        )
        {
            token = tokenHandler;
            id = User.id;
            first_name = User.first_name;
            last_name = User.last_name;
            code = User.code;
            role_id = User.role_id;
            department_id = User.department_id;
            division_id = User.division_id;
            group_id = User.group_id;
            tenant_id = User.tenant_id;
            is_tenant_created = User.is_tenant_created ?? false;
            temp_password_used = User.temp_password_used;
            expiredAt = expired;
        }
    }
}
