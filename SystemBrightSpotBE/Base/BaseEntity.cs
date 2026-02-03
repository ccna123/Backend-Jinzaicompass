using System.ComponentModel.DataAnnotations.Schema;
using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE.Base
{
    public class BaseEntity
    {
        public long? tenant_id { get; set; }
        [ForeignKey("tenant_id")]
        public Tenant? Tenant { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
