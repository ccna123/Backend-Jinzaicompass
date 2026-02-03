using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Category
{
    public class AddCategoryDto
    {
        [SwaggerSchema("Required")]
        [Required]
        [EnumDataType(typeof(CategoryTypeEnum))]
        public CategoryTypeEnum? type { get; set; }

        [SwaggerSchema("Required, Maxlength = 64")]
        [Required(ErrorMessageResourceType = typeof(CategoryResource), ErrorMessageResourceName = "NameRequired")]
        [StringLength(64, ErrorMessageResourceType = typeof(CategoryResource), ErrorMessageResourceName = "NameMaxLength")]
        public required string name { get; set; }
    }
}
