using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace a2n.DynData
{
    public class DynDataNetOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;
        public DbContextOptionsExtensionInfo Info => _info ??= new DynDataNetExtensionInfo(this);
        public DatabaseServer DBSetting { get; set; }
        public DynDbContextEventHandler Handler { get; set; }
        public DynDataNetOptionsExtension()
        {
        }
        public DynDataNetOptionsExtension(DynDataNetOptionsExtension extension)
        {
            this.DBSetting = extension.DBSetting;
        }
        public void ApplyServices(IServiceCollection services)
        {
        }

        public void Validate(IDbContextOptions options)
        {
        }
        private class DynDataNetExtensionInfo : DbContextOptionsExtensionInfo
        {
            private int? _serviceProviderHash;
            private new DynDataNetOptionsExtension Extension
                => (DynDataNetOptionsExtension)base.Extension;
            public override bool IsDatabaseProvider => false;

            public override string LogFragment => String.Empty;

            public DynDataNetExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }
            public override int GetServiceProviderHashCode()
            {
                if (_serviceProviderHash == null)
                {
                    var hashCode = new HashCode();
                    //hashCode.Add(base.GetServiceProviderHashCode());
                    hashCode.Add(Extension.DBSetting);
                    _serviceProviderHash = hashCode.ToHashCode();
                }
                return _serviceProviderHash.Value;

            }
            public override void PopulateDebugInfo([NotNullAttribute] IDictionary<string, string> debugInfo)
            {
                debugInfo["a2n.DataAccess:" + nameof(Extension.DBSetting)] = HashCode.Combine(Extension.DBSetting).ToString(CultureInfo.InvariantCulture);
            }

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            {
                return true;
            }
        }
    }
}
