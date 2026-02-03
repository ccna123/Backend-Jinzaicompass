using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Category
{
    public class CategoryParamDto
    {
        [Required]
        [EnumDataType(typeof(CategoryTypeEnum))]
        public CategoryTypeEnum? type { get; set; }
    }
}
