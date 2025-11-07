using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApiLand9.Data.Models
{
    public class spGetEmailConfigOutput
    {
        public bool? Enabled { get; set; }
        public string ClientId { get; set; } = null!;
        public string EncryptedClientSecret { get; set; } = null!;
        public string TenantId { get; set; } = null!;
    }
}
