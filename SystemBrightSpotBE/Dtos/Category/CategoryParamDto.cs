using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Category
{
    public class CategoryParamDto
    {
        [Required]
        [EnumDataType(typeof(CategoryTypeEnum))]
        public CategoryTypeEnum? type { get; set; }
    }
}
