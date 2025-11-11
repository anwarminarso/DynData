using Microsoft.EntityFrameworkCore.Infrastructure;
using a2n.DynData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.EntityFrameworkCore
{
    public static class DynDataOptionsBuilderExtension
    {
        public static DbContextOptionsBuilder UseDynData(this DbContextOptionsBuilder optionsBuilder, DatabaseServer dbSetting)
        {
            var extension = (DynDataNetOptionsExtension)(
                optionsBuilder.Options.FindExtension<DynDataNetOptionsExtension>() ??
                new DynDataNetOptionsExtension());
            extension.DBSetting = dbSetting;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            switch (dbSetting.Provider)
            {
                case DatabaseProvider.SqlServer:
                    optionsBuilder = optionsBuilder.UseSqlServer(dbSetting.ConnectionString);
                    break;
#if !DISABLE_POSTGRESQL
                case DatabaseProvider.Postgres:
                    optionsBuilder = optionsBuilder.UseNpgsql(dbSetting.ConnectionString);
                    break;
#endif
#if !DISABLE_MYSQL
                case DatabaseProvider.MySql:
                    optionsBuilder = optionsBuilder.UseMySql(dbSetting.ConnectionString, ServerVersion.AutoDetect(dbSetting.ConnectionString));
                    break;
#endif
                case DatabaseProvider.Sqlite:
                    optionsBuilder = optionsBuilder.UseSqlite(dbSetting.ConnectionString);
                    break;
                default:
                    break;
            }
            return optionsBuilder;
        }
        public static DbContextOptionsBuilder UseDynData(this DbContextOptionsBuilder optionsBuilder, DatabaseServer dbSetting, DynDbContextEventHandler eventHandler)
        {
            var extension = (DynDataNetOptionsExtension)(
                optionsBuilder.Options.FindExtension<DynDataNetOptionsExtension>() ??
                new DynDataNetOptionsExtension());
            extension.DBSetting = dbSetting;
            extension.Handler = eventHandler;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            switch (dbSetting.Provider)
            {
                case DatabaseProvider.SqlServer:
                    optionsBuilder = optionsBuilder.UseSqlServer(dbSetting.ConnectionString);
                    break;
#if !DISABLE_POSTGRESQL
                case DatabaseProvider.Postgres:
                    optionsBuilder = optionsBuilder.UseNpgsql(dbSetting.ConnectionString);
                    break;
#endif
#if !DISABLE_MYSQL
                case DatabaseProvider.MySql:
                    optionsBuilder = optionsBuilder.UseMySql(dbSetting.ConnectionString, ServerVersion.AutoDetect(dbSetting.ConnectionString));
                    break;
#endif
                case DatabaseProvider.Sqlite:
                    optionsBuilder = optionsBuilder.UseSqlite(dbSetting.ConnectionString);
                    break;
                default:
                    break;
            }
            return optionsBuilder;
        }
    }
}
