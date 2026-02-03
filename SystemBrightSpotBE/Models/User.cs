using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class User : BaseEntity
    {
        [Key]
        public long id { get; set; }
        public string? avatar { get; set; } = String.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string first_name { get; set; } = String.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string last_name { get; set; } = String.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string first_name_kana { get; set; } = String.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string last_name_kana { get; set; } = String.Empty;
        [Required]
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string email { get; set; } = String.Empty;
        [Required]
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string code { get; set; } = String.Empty;
        [ForeignKey("Gender")]
        public long? gender_id { get; set; }
        public Gender? Gender { get; set; }
        public DateOnly? date_of_birth { get; set; }
        [StringLength(20, ErrorMessage = "MaxLength 20 characters")]
        public string? phone { get; set; } = String.Empty;
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string? address { get; set; } = String.Empty;
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string? nearest_station { get; set; } = String.Empty;
        [Comment(@"
            1 = System admin
            2 = Power User
            3 = Senior User
            4 = Contributor
            5 = Member")]
        [ForeignKey("Role")]
        public long? role_id { get; set; } = 5;
        public Role? Role { get; set; }
        [ForeignKey("Department")]
        public long? department_id { get; set; }
        public Department? Department { get; set; }
        [ForeignKey("Division")]
        public long? division_id { get; set; }
        public Division? Division { get; set; }
        [ForeignKey("Group")]
        public long? group_id { get; set; }
        public Group? Group { get; set; }
        [ForeignKey("Position")]
        public long? position_id { get; set; }
        public Position? Position { get; set; }
        [ForeignKey("EmploymentType")]
        public long? employment_type_id { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        [ForeignKey("EmploymentStatus")]
        public long? employment_status_id { get; set; }
        public EmploymentStatus? EmploymentStatus { get; set; }
        [Comment(@"
            0：手動作成
            1：テナントによる自動作成")]
        public bool? is_tenant_created { get; set; } = false;
        [Comment(@"
            0：デフォルト
            1：電子メールが正常に検証されたとき")]
        public bool? active { get; set; } = false;
        [Required]
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string password { get; set; } = String.Empty;
        [Comment(@"
            0：アカウントが最初のパスワードを変更し、確認されました
            1: アカウントの作成中に使用される一時的なパスワードまたはアカウントがアクティブ化された後にリセットされたパスワード")]
        public bool? temp_password_used { get; set; } = true;
        public DateTime? temp_password_expired_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public ICollection<UserStatusHistory>? UserStatusHistory { get; set; }
        public ICollection<UserCertification>? UserCertification { get; set; }
        public ICollection<UserCompanyAward>? UserCompanyAward { get; set; }
        public ICollection<UserExperienceJob>? UserExperienceJob { get; set; }
        public ICollection<UserExperienceField>? UserExperienceField { get; set; }
        public ICollection<UserExperienceArea>? UserExperienceArea { get; set; }
        public ICollection<UserSpecificSkill>? UserSpecificSkill { get; set; }
        public ICollection<Project>? Projects { get; set; }
    }
}
