using SystemBrightSpotBE.Dtos.UserStatusHistory;
using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Dtos.User
{
    public class UpdateUserDto
    {
        [AllowedContentTypeAttribute(new[] { "image/jpeg", "image/png", "image/svg+xml" })]
        public IFormFile? avatar { get; set; } 
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameMaxLength")]
        [RegularExpression(
            @"^[\p{IsCJKUnifiedIdeographs}\u3400-\u4DBF\uF900-\uFAFF" +  // Han (Kanji + Compatibility)
            @"\u30A0-\u30FF" +                                           // Katakana
            @"\uFF66-\uFF9F" +                                           // Halfwidth Katakana
            @"\u30FC" +                                                  // ー
            @"\u3005" +                                                  // 々
            @"]+$",
            ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameRegx")]
        public string first_name { get; set; } = String.Empty;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameMaxLength")]
        [RegularExpression(
            @"^[\p{IsCJKUnifiedIdeographs}\u3400-\u4DBF\uF900-\uFAFF" +  // Han (Kanji + Compatibility)
            @"\u30A0-\u30FF" +                                           // Katakana
            @"\uFF66-\uFF9F" +                                           // Halfwidth Katakana
            @"\u30FC" +                                                  // ー
            @"\u3005" +                                                  // 々
            @"]+$",
            ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameRegx")]
        public string last_name { get; set; } = String.Empty;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameKanaRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameKanaMaxLength")]
        [RegularExpression(@"^[\u30A0-\u30FF\u30FC\s]+$", ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "FirstNameKanaRegx")]
        public string first_name_kana { get; set; } = String.Empty;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameKanaRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameKanaMaxLength")]
        [RegularExpression(@"^[\u30A0-\u30FF\u30FC\s]+$", ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "LastNameKanaRegx")]
        public string last_name_kana { get; set; } = String.Empty;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CodeRequired")]
        [RegularExpression(@"^[A-Za-z]{1,2}\d{1,10}$", ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CodeRegx")]
        public string code { get; set; } = String.Empty;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "GenderRequired")]
        [Range(1, 3, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "GenderRegx")]
        public long? gender_id { get; set; }
        [RegularExpression(@"^\d{4}/(0?[1-9]|1[0-2])/(0?[1-9]|[12]\d|3[01])$",
          ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "DateOfBirthRegx")]
        public string date_of_birth { get; set; } = String.Empty;
        [Range(1, 5, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "RoleRegx")]
        public long role_id { get; set; } = 5;
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "DepatmentRequired")]
        public long department_id { get; set; }
        public long? division_id { get; set; }
        public long? group_id { get; set; }
        public long? position_id { get; set; }
        public long? employment_type_id { get; set; }
        public long? employment_status_id { get; set; }
        public string? status_history_json { get; set; }
        [NotMapped]
        public List<StatusHistoryDto>? status_history { get; set; }
        [MaxLength(16, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "PhoneRegx")]
        public string? phone { get; set; } = String.Empty;
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "AddressMaxLength")]
        public string? address { get; set; } = String.Empty;
        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "NearestStationMaxLength")]
        public string? nearest_station { get; set; } = String.Empty;
    }
}
