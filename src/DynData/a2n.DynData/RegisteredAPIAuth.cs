using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a2n.DynData
{
    public class RegisteredAPIAuth
    {
        private readonly IServiceProvider provider;
        private readonly IServiceScopeFactory factory;
        private readonly Dictionary<string, Tuple<Type, Type, Type>> dicRegisteredSchemaName;

        internal void Register<TAPIAuth, TDynDbContext, TTemplate>(string schemaName)
            where TAPIAuth : IDynDataAPIAuth
            where TDynDbContext : DynDbContext
            where TTemplate : IQueryTemplate
        {
            var tApiAuth = typeof(TAPIAuth);
            var tDynDbContext = typeof(TDynDbContext);
            var tTemplate = typeof(TTemplate);
            //if (!dicRegistered.ContainsKey(tApiAuth))
            //    dicRegistered.Add(tApiAuth, new List<Type>());
            //if (!dicRegistered[tApiAuth].Contains(tDynDbContext))
            //    dicRegistered[tApiAuth].Add(tDynDbContext);
            if (!dicRegisteredSchemaName.ContainsKey(schemaName))
                dicRegisteredSchemaName.Add(schemaName, new Tuple<Type, Type, Type>(tApiAuth, tDynDbContext, tTemplate));
        }

        public RegisteredAPIAuth(IServiceProvider provider, IServiceScopeFactory factory)
        {
            this.provider = provider;
            this.factory = factory;
            dicRegisteredSchemaName = new Dictionary<string, Tuple<Type, Type, Type>>();
        }
        public void GetDynDbContexts(Action<string, IDynDataAPIAuth, DynDbContext, IQueryTemplate> action)
        {
            using (var scope = factory.CreateScope())
            {
                foreach (var schema in dicRegisteredSchemaName.Keys)
                {
                    var tuple = dicRegisteredSchemaName[schema];
                    var apiAuth = (IDynDataAPIAuth)scope.ServiceProvider.GetService(tuple.Item1)!;
                    var db = (DynDbContext)scope.ServiceProvider.GetService(tuple.Item2)!;
                    var qryTpl = (IQueryTemplate)scope.ServiceProvider.GetService(tuple.Item3)!;
                    action(schema, apiAuth, db, qryTpl);
                }
            }
        }
        //public void GetDynDbContexts(IDynDataAPIAuth apiAuth, Action<DynDbContext[]> action)
        //{
        //    using (var scope = factory.CreateScope())
        //    {
        //        var apiAuthType = apiAuth.GetType();
        //        var dbLst = new List<DynDbContext>();
        //        foreach (var item in dicRegistered[apiAuthType])
        //        {
        //            var db = scope.ServiceProvider.GetService(item);
        //            dbLst.Add((DynDbContext)db!);
        //        }
        //        action(dbLst.ToArray());
        //    }
        //}
    }
}
