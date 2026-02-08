using System;
using System.IO;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FIXIT.Infrastructure.Data.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("constr");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new Exception("DB_CONNECTION is missing");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
                x => x.UseNetTopologySuite());

        return new AppDbContext(optionsBuilder.Options);
    }
}