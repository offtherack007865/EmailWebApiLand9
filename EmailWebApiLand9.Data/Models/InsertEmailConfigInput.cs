using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApiLand9.Data.Models
{
    public class InsertEmailConfigInput
    {
        public InsertEmailConfigInput()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            TenantId = string.Empty;
        }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }
}
