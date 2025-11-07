using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApiLand9.Data.Models
{
    public class EmailConfigJsonOutput
    {
        public EmailConfigJsonOutput()
        {
            IsOk = true;
            ErrorMessage = string.Empty;
            Config = new List<spGetEmailConfigOutput>();
        }

        public bool IsOk { get; set; }
        public string ErrorMessage { get; set; }
        public List<spGetEmailConfigOutput> Config { get; set; }
    }
}
