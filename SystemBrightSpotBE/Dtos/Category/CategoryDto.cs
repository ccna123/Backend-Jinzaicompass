using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Category
{
    public class CategoryDto
    {
        public long id { get; set; }
        public string name { get; set; } = String.Empty;
        public bool delete_flag { get; set; } = true;
    }
}
