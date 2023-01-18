using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Oracle.ManagedDataAccess.EntityFramework;
using Oracle.ManagedDataAccess.Client;
using NUnit.Framework;
using System.Configuration;
using System.Data;
using ConsoleTableExt;
using System.Data.Common;

namespace ClassicEntityFramework.KeyUserModel
{

    public class KeyUserContext : DbContext
    {
        public KeyUserContext() { }

        public KeyUserContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }
        

        public DbSet<KeyUser> KeyUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var keyUserConfig = modelBuilder.HasDefaultSchema("SQLALCHEMY").Entity<KeyUser>().ToTable("TKEYUSER");

            keyUserConfig.HasKey(x => x.Id).Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            keyUserConfig.Property(x => x.Id).HasColumnName("ID_USER");
            keyUserConfig.Property(x => x.Name).HasColumnName("DT_NAME").IsRequired();
            keyUserConfig.Property(x => x.MaxSize).HasColumnName("DT_MAX_SIZE");

            base.OnModelCreating(modelBuilder);
        }
    }
}