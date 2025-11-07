using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmailWebApiLand9.Data.Models;

public partial class EmailWebApi8Context : DbContext
{
    public EmailWebApi8Context(DbContextOptions<EmailWebApi8Context> options)
        : base(options)
    {
        string projectPath = AppDomain.CurrentDomain.BaseDirectory;
        IConfigurationRoot configuration =
            new ConfigurationBuilder()
                .SetBasePath(projectPath)
        .AddJsonFile(MyConstants.AppSettingsFile)
        .Build();
        Database.SetCommandTimeout(9000);
        MyConnectionString =
            configuration.GetConnectionString(MyConstants.ConnectionString);
    }

    public string MyConnectionString { get; set; }


    public virtual DbSet<EmailConfig> EmailConfigs { get; set; }

    public virtual DbSet<spGetEmailConfigOutput> SpGetEmailConfigOutputList { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<spGetEmailConfigOutput>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<EmailConfig>(entity =>
        {
            entity.HasKey(e => e.Pk);

            entity.ToTable("EmailConfig");

            entity.Property(e => e.ClientId).HasMaxLength(300);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedTimestamp).HasColumnType("datetime");
            entity.Property(e => e.EncryptedClientSecret).HasMaxLength(300);
            entity.Property(e => e.TenantId).HasMaxLength(300);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTimestamp).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
