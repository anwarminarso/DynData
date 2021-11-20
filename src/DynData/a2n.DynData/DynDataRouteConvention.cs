using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a2n.DynData
{
    public class DynDataRouteConvention<TDbContext> : IControllerModelConvention
        where TDbContext : DynDbContext, new()
    {
        private readonly string ControllerName = string.Empty;
        public DynDataRouteConvention()
        {
        }
        public DynDataRouteConvention(string ControllerName)
        {
            this.ControllerName = ControllerName;
        }
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType && controller.ControllerType == typeof(DynDataController<TDbContext>))
            {
                if (!String.IsNullOrEmpty(ControllerName))
                    controller.ControllerName = ControllerName;
                else
                    controller.ControllerName = typeof(TDbContext).Name;
            }
        }
    }
    public class DynDataRouteConvention<TDbContext, TTemplate> : IControllerModelConvention
        where TDbContext : DynDbContext, new()
        where TTemplate : QueryTemplate<TDbContext>, new()
    {
        private readonly string ControllerName = string.Empty;
        public DynDataRouteConvention()
        {
        }
        public DynDataRouteConvention(string ControllerName)
        {
            this.ControllerName = ControllerName;
        }
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType && controller.ControllerType == typeof(DynDataController<TDbContext, TTemplate>))
            {
                if (!String.IsNullOrEmpty(ControllerName))
                    controller.ControllerName = ControllerName;
                else
                    controller.ControllerName = typeof(TDbContext).Name;
            }
        }
    }
}
