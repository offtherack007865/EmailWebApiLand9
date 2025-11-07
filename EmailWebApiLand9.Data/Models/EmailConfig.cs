using System;
using System.Collections.Generic;

namespace EmailWebApiLand9.Data.Models;

public partial class EmailConfig
{
    public long Pk { get; set; }

    public bool? Enabled { get; set; }

    public string ClientId { get; set; } = null!;

    public string EncryptedClientSecret { get; set; } = null!;

    public string TenantId { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedTimestamp { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedTimestamp { get; set; }
}
