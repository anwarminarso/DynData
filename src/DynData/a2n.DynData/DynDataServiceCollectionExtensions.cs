using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace a2n.DynData
{
    public static class DynDataServiceCollectionExtensions
    {
        public static IServiceCollection AddDynDataApi<TDbContext>(this IServiceCollection services, string ControllerName)
            where TDbContext : DynDbContext, new()
        {
            services.AddControllers(o =>
                {
                    o.Conventions.Add(new DynDataRouteConvention<TDbContext>(ControllerName));
                })
                .ConfigureApplicationPartManager(o =>
                {
                    o.FeatureProviders.Add(new DynDataControllerFeatureProvider<TDbContext>());
                });
            return services;
        }
        public static IServiceCollection AddDynDataApi<TDbContext, TTemplate>(this IServiceCollection services, string ControllerName)
            where TDbContext : DynDbContext, new()
            where TTemplate : QueryTemplate<TDbContext>, new()
        {
            services.AddSingleton<TTemplate>();
            services.AddControllers(o =>
            {
                o.Conventions.Add(new DynDataRouteConvention<TDbContext, TTemplate>(ControllerName));
            })
                .ConfigureApplicationPartManager(o =>
                {
                    o.FeatureProviders.Add(new DynDataControllerFeatureProvider<TDbContext, TTemplate>());
                });
            return services;
        }
        public static IServiceCollection AddDynDataApi<TAPIAuth, TDbContext, TTemplate>(this IServiceCollection services, string ControllerName)
            where TDbContext : DynDbContext, new()
            where TTemplate : QueryTemplate<TDbContext>, new()
            where TAPIAuth : IDynDataAPIAuth
        {
            services.TryAddSingleton<RegisteredAPIAuth>();
            services.AddSingleton<TTemplate>();
            services.AddScoped(typeof(TAPIAuth));
            //services.TryAddSingleton(p =>
            //{
            //    var registerAPIAuth = p.GetService<RegisteredAPIAuth>();
            //    if (registerAPIAuth == null)
            //    {
            //        registerAPIAuth = p.GetService<RegisteredAPIAuth>();
            //    }
            //    registerAPIAuth!.Register<TAPIAuth, TDbContext>(ControllerName);
            //    return registerAPIAuth!;
            //});
            services.AddControllers(o =>
            {
                o.Conventions.Add(new DynDataRouteConvention<TDbContext, TTemplate, TAPIAuth>(ControllerName));
            })
                .ConfigureApplicationPartManager(o =>
                {
                    o.FeatureProviders.Add(new DynDataControllerFeatureProvider<TDbContext, TTemplate, TAPIAuth>());
                });
            return services;
        }
    }


    public static class DynDataApplicationBuilderExtensions
    {
        public static void RegisterDynDataServiceAPIAuth<TAPIAuth, TDbContext, TTemplate>(this IApplicationBuilder builder, string ControllerName)
            where TDbContext : DynDbContext, new()
            where TTemplate : QueryTemplate<TDbContext>, new()
            where TAPIAuth : IDynDataAPIAuth
        {
            var registeredAPIAuth = builder.ApplicationServices.GetService<RegisteredAPIAuth>();
            registeredAPIAuth!.Register<TAPIAuth, TDbContext, TTemplate>(ControllerName);
        }
    }
}
