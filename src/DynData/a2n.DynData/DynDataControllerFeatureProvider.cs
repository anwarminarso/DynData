using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace a2n.DynData
{
    public class DynDataControllerFeatureProvider<TDbContext> : IApplicationFeatureProvider<ControllerFeature>
        where TDbContext : DynDbContext, new()
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.Add(typeof(DynDataController<>).MakeGenericType(typeof(TDbContext)).GetTypeInfo());
        }
    }
    public class DynDataControllerFeatureProvider<TDbContext, TTemplate> : IApplicationFeatureProvider<ControllerFeature>
        where TDbContext : DynDbContext, new()
        where TTemplate : QueryTemplate<TDbContext>, new()
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.Add(typeof(DynDataController<,>).MakeGenericType(typeof(TDbContext), typeof(TTemplate)).GetTypeInfo());
        }
    }
}
