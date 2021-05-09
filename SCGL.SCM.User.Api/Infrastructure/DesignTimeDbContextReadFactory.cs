using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCGL.SCM.User.Api.Infrastructure
{
    public class DesignTimeDbContextReadFactory : IDesignTimeDbContextFactory<DbReadDataContext>
    {
        public DbReadDataContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<DbReadDataContext>();
            var connectionString = configuration.GetConnectionString("DbReadDataContext");
            builder.UseSqlServer(connectionString);
            return new DbReadDataContext(builder.Options);
        }
    }
}
