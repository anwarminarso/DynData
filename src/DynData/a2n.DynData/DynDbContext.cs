using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace a2n.DynData
{
    public abstract class DynDbContext : DbContext
    {
        private DynDbContextEventHandler Handler;
        private static object lockObj = new object();

        private static readonly Dictionary<Type, Dictionary<string, Type>> dicTables;
        private static readonly Dictionary<Type, Dictionary<string, Metadata[]>> dicMetadata;



        public DatabaseServer DBSetting { get; set; }
        public ServerVersion MySqlVersion { get; set; }

        public DynDbContext()
        {
        }
        public DynDbContext(DatabaseServer DBSetting)
        {
            this.DBSetting = DBSetting;
        }
        protected DynDbContext(DbContextOptions options)
            : base(options)
        {
        }

        static DynDbContext()
        {
            dicTables = new Dictionary<Type, Dictionary<string, Type>>();
            dicMetadata = new Dictionary<Type, Dictionary<string, Metadata[]>>();
        }

        public void PopulateMetadata()
        {
            lock (lockObj)
            {
                var dbCtxtType = this.GetType();
                if (dicTables.ContainsKey(dbCtxtType))
                    return;
                var tableTypes = new Dictionary<string, Type>();
                var metadataTypes = new Dictionary<string, Metadata[]>();
                var tableTypeArr = dbCtxtType.GetProperties().Where(t => t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                                    .Select(t => t.PropertyType.GetGenericArguments()[0]).ToArray();
                foreach (var tableType in tableTypeArr)
                {
                    tableTypes.Add(tableType.Name.ToString(), tableType);
                    var meta = GetEntityType(tableType);
                    var metaProps = meta.GetProperties();
                    List<Metadata> metadataLst = new List<Metadata>();
                    foreach (var p in metaProps)
                    {
                        if (Handler != null)
                            metadataLst.Add(new Metadata(p, Handler.OnMetaGenerated));
                        else
                            metadataLst.Add(new Metadata(p));
                    }
                    metadataTypes.Add(tableType.Name.ToString(), metadataLst.ToArray());
                }


                dicTables.Add(dbCtxtType, tableTypes);
                dicMetadata.Add(dbCtxtType, metadataTypes);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseDynData(DBSetting);
            }
            else
            {
                var extension = optionsBuilder.Options.FindExtension<DynDataNetOptionsExtension>();
                if (extension != null)
                {
                    this.DBSetting = extension.DBSetting;
                    this.Handler = extension.Handler;
                }
            }
        }


        public string[] GetAllTableViewNames()
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();
            return dicTables[dbCtxtType].Keys.ToArray();
        }
        public Type GetTableType(string tableName)
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();
            var dicTable = dicTables[dbCtxtType];
            if (dicTable.ContainsKey(tableName))
                return dicTable[tableName];
            else
                return null;
        }

        public DbSet<T> GetDBSet<T>()
            where T : class, new()
        {
            var type = typeof(T);
            var pi = this.GetType().GetProperties().Where(t => t.PropertyType.IsGenericType)
                .Select(t => new { pi = t, args = t.PropertyType.GetGenericArguments() })
                .Where(t => t.args[0] == type)
                .Select(t => t.pi).FirstOrDefault();

            //var dbsetType = typeof(DbSet<T>);
            //var pi = this.GetType().GetProperties().Where(t => t.PropertyType == dbsetType).FirstOrDefault();

            if (pi != null)
                return (DbSet<T>)pi.GetValue(this);
            else
                return null;
        }
        public object GetDBSet(Type type)
        {
            var mtd = this.GetType().GetMethod("GetDBSet", new Type[] { }).MakeGenericMethod(type);
            return mtd.Invoke(this, null);
        }
        public IQueryable<dynamic> GetQueryable(Type type)
        {
            var obj = GetDBSet(type);
            var dbsetType = typeof(DbSet<>).MakeGenericType(type);
            var asQueryableMtd = dbsetType.GetMethod("AsQueryable", new Type[0]);

            return asQueryableMtd.Invoke(obj, null) as IQueryable<dynamic>;
        }
        public IQueryable<dynamic> GetQueryable(string tableName)
        {
            var tableType = GetTableType(tableName);
            if (tableType == null)
                throw new Exception($"Table {tableName} not found");
            return GetQueryable(tableType);
        }

        public string[] GetPKNames<T>()
            where T : class, new()
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var metadataArr = dicMetadata[dbCtxtType][typeof(T).Name];
            return metadataArr.Where(t => t.IsPrimaryKey).Select(t => t.FieldName).ToArray();
        }
        public string[] GetPKNames(Type tp)
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();
            var metadataArr = dicMetadata[dbCtxtType][tp.Name];
            return metadataArr.Where(t => t.IsPrimaryKey).Select(t => t.FieldName).ToArray();
        }
        public PropertyInfo[] GetPKProperties<T>()
            where T : class, new()
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var metadataArr = dicMetadata[dbCtxtType][typeof(T).Name];
            return metadataArr.Where(t => t.IsPrimaryKey).Select(t => t.PropertyInfo).ToArray();
        }

        public PropertyInfo[] GetProperties(string tableName)
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var dicTable = dicTables[dbCtxtType];
            if (dicTable.ContainsKey(tableName))
                return dicMetadata[dbCtxtType][tableName].Select(t => t.PropertyInfo).ToArray();
            else
                return null;
        }
        public PropertyInfo[] GetProperties<T>()
        {
            var tableName = typeof(T).Name;
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var dicTable = dicTables[dbCtxtType];
            if (dicTable.ContainsKey(tableName))
                return dicMetadata[dbCtxtType][typeof(T).Name].Select(t => t.PropertyInfo).ToArray();
            else
                return null;
        }

        public Metadata[] GetMetadata(string tableName)
        {
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var dicTable = dicTables[dbCtxtType];
            if (dicTable.ContainsKey(tableName))
                return dicMetadata[dbCtxtType][tableName];
            else
                return null;
        }
        public Metadata[] GetMetadata<T>()
        {
            var tableName = typeof(T).Name;
            var dbCtxtType = this.GetType();
            if (!dicTables.ContainsKey(dbCtxtType))
                PopulateMetadata();

            var dicTable = dicTables[dbCtxtType];
            if (dicTable.ContainsKey(tableName))
                return dicMetadata[dbCtxtType][tableName];
            else
                return null;
        }

        public Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>()
            where T : class, new()
        {
            //return this.Entry<T>(Activator.CreateInstance<T>()).Metadata;
            return this.Model.FindEntityType(typeof(T));
            //if (mtdEntryWithoutDetectChanges == null)
            //{
            //    var mtds = typeof(DbContext).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            //    mtdEntryWithoutDetectChanges = mtds.Where(t => t.Name == "EntryWithoutDetectChanges" && t.IsGenericMethod == true).FirstOrDefault();
            //}
            //var obj = Activator.CreateInstance<T>();
            //var mtd = mtdEntryWithoutDetectChanges.MakeGenericMethod(typeof(T));
            //var entry = mtd.Invoke(this, new object[] { obj }) as Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>;
            //return entry.Metadata;
        }
        public Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType(Type tableType)
        {
            //var mtd = this.GetType().GetMethod("GetEntityType", new Type[] { }).MakeGenericMethod(tableType);
            //return mtd.Invoke(this, null) as Microsoft.EntityFrameworkCore.Metadata.IEntityType;
            return this.Model.FindEntityType(tableType);
        }

        public object FindByKey(string tableName, string jsonKeyValues)
        {
            return FindByKey(tableName, JObject.Parse(jsonKeyValues));
        }
        public object FindByKey(string tableName, System.Text.Json.JsonElement keyValues)
        {
            return FindByKey(tableName, JObject.Parse(keyValues.ToString()));
        }
        public object FindByKey(string tableName, JObject jKey)
        {
            if (jKey == null)
                throw new ArgumentNullException(nameof(jKey));
            if (jKey.Properties().Count() == 0)
                return null;
            var tableType = this.GetTableType(tableName);
            if (tableType == null)
                throw new Exception("Table not found");
            return FindByKey(this.GetTableType(tableName), jKey);
        }
        public object FindByKey(Type tableType, JObject jKey)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));
            if (jKey == null)
                throw new ArgumentNullException(nameof(jKey));
            if (jKey.Properties().Count() == 0)
                return null;

            PropertyInfo[] pkPropArr = null;
            pkPropArr = this.GetMetadata(tableType.Name).Where(t => t.IsPrimaryKey).Select(t => t.PropertyInfo).ToArray();
            var pkValues = pkPropArr.Where(t => jKey.ContainsKey(t.Name)).Select(t => jKey[t.Name].ToObject(t.PropertyType)).ToArray();

            return this.Find(tableType, pkValues);
        }

        public object Create(string tableName, string jsonValue)
        {
            var token = JToken.Parse(jsonValue);
            return Create(tableName, token);
        }
        public object[] Create(string tableName, System.Text.Json.JsonElement value)
        {
            var token = JToken.Parse(value.ToString());
            return Create(tableName, token);
        }
        public object[] Create(string tableName, JToken value)
        {
            var valueType = GetTableType(tableName);
            if (value.Type == JTokenType.Array)
            {
                var dataArr = value.ToObject(valueType.MakeArrayType()) as object[];

                if (Handler != null && !Handler.OnBeforeCreate(this, valueType, dataArr))
                    return null;
                AddRange(dataArr);
                return dataArr;
            }
            else
            {
                var data = value.ToObject(valueType);
                if (Handler != null && !Handler.OnBeforeCreate(this, valueType, data))
                    return null;
                Add(data);
                return new object[] { data };
            }
        }
        public object Create(string tableName, JObject value)
        {
            var valueType = GetTableType(tableName);
            var data = value.ToObject(valueType);
            if (Handler != null && !Handler.OnBeforeCreate(this, valueType, data))
                return null;
            Add(data);
            return data;
        }

        public object[] Update(string tableName, string jsonValue)
        {
            var token = JToken.Parse(jsonValue);
            return Update(tableName, token);
        }
        public object[] Update(string tableName, System.Text.Json.JsonElement value)
        {
            var token = JToken.Parse(value.ToString());
            return Update(tableName, token);
        }
        public object[] Update(string tableName, JToken value)
        {
            var metaArr = GetMetadata(tableName);
            List<object> result = new List<object>();
            var valueType = GetTableType(tableName);
            if (valueType == null)
                throw new Exception($"Table {tableName} not found");
            object[] dataArr = null;
            var metaArrr = GetMetadata(tableName);
            var metaPKArr = metaArrr.Where(t => t.IsPrimaryKey).ToArray();
            JObject[] jobjArr = null;
            if (value.Type == JTokenType.Array)
            {
                dataArr = value.ToObject(valueType.MakeArrayType()) as object[];
                jobjArr = (value as JArray).Select(t => t as JObject).ToArray();
            }
            else
            {
                var data = value.ToObject(valueType);
                dataArr = new object[] { data };
                jobjArr = new JObject[] { value as JObject };
            }
            for (int i = 0; i < dataArr.Length; i++)
            {
                var data = dataArr[i];
                var jObj = jobjArr[i];
                var oldData = FindByKey(valueType, jObj);
                if (oldData == null)
                    throw new Exception($"Data {jObj.ToString()} not found");

                if (Handler != null && !Handler.OnBeforeUpdate(this, valueType, oldData, jObj))
                    continue;

                var modifiedPropArr = metaArrr.Where(t => !t.IsPrimaryKey && jObj.ContainsKey(t.FieldName)).Select(t => t.PropertyInfo).ToArray();
                foreach (var modifiedProp in modifiedPropArr)
                {
                    object newValue = jObj[modifiedProp.Name];
                    if (newValue != null)
                        modifiedProp.SetValue(oldData, jObj[modifiedProp.Name].ToObject(modifiedProp.PropertyType));
                    else
                        modifiedProp.SetValue(oldData, null);
                }
                Handler?.OnAfterUpdate(this, valueType, oldData);
                result.Add(oldData);
            }
            return result.ToArray();
        }
        public object Update(string tableName, JObject value)
        {
            var valueType = GetTableType(tableName);
            var data = value.ToObject(valueType);
            var oldData = FindByKey(valueType, value);
            var metaArr = GetMetadata(tableName);
            if (oldData == null)
                throw new Exception($"Data {value.ToString()} not found");

            if (Handler != null && !Handler.OnBeforeUpdate(this, valueType, oldData, value))
                return null;

            var modifiedPropArr = metaArr.Where(t => !t.IsPrimaryKey && value.ContainsKey(t.FieldName)).Select(t => t.PropertyInfo).ToArray();
            foreach (var modifiedProp in modifiedPropArr)
            {
                object newValue = value[modifiedProp.Name];
                if (newValue != null)
                    modifiedProp.SetValue(oldData, value[modifiedProp.Name].ToObject(modifiedProp.PropertyType));
                else
                    modifiedProp.SetValue(oldData, null);
            }
            Handler?.OnAfterUpdate(this, valueType, oldData);
            return oldData;
        }



        public object[] Delete(string tableName, string jsonKeyValues)
        {
            var token = JToken.Parse(jsonKeyValues);
            return Delete(tableName, token);
        }
        public object[] Delete(string tableName, System.Text.Json.JsonElement keyValues)
        {
            var token = JToken.Parse(keyValues.ToString());
            return Delete(tableName, token);
        }
        public object[] Delete(string tableName, JToken jKey)
        {
            var metaArr = GetMetadata(tableName);
            List<object> result = new List<object>();
            var valueType = GetTableType(tableName);
            if (valueType == null)
                throw new Exception($"Table {tableName} not found");
            JObject[] jobjArr = null;
            if (jKey.Type == JTokenType.Array)
            {
                jobjArr = (jKey as JArray).Select(t => t as JObject).ToArray();
            }
            else
            {
                var data = jKey.ToObject(valueType);
                jobjArr = new JObject[] { jKey as JObject };
            }
            for (int i = 0; i < jobjArr.Length; i++)
            {
                var jObj = jobjArr[i];
                var oldData = FindByKey(valueType, jObj);
                if (oldData == null)
                    throw new Exception($"Data {jObj.ToString()} not found");

                if (Handler != null && !Handler.OnBeforeDelete(this, valueType, oldData))
                    continue;

                Remove(oldData);
                result.Add(oldData);
            }
            return result.ToArray();
        }
        public object Delete(string tableName, JObject jKey)
        {
            var valueType = GetTableType(tableName);
            var data = jKey.ToObject(valueType);
            var oldData = FindByKey(valueType, jKey);
            var metaArr = GetMetadata(tableName);
            if (oldData == null)
                throw new Exception($"Data {jKey.ToString()} not found");
            if (Handler != null && !Handler.OnBeforeDelete(this, valueType, oldData))
                return null;
            Remove(oldData);
            return oldData;
        }

        public IQueryable<dynamic> Query(string tableName, params ExpressionRule[] rules)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(tableName);
            var tableType = GetTableType(tableName);
            if (tableType == null)
                throw new Exception($"Table {tableName} not found");
            var propArr = GetProperties(tableName);
            var qry = GetQueryable(tableType);
            if (rules == null || rules.Length == 0)
                return qry;

            foreach (var rule in rules)
                rule.ValidatePropertyType(propArr);
            var whereExp = ExpressionBuilder.Build(tableType, rules);

            
            return qry.Where(whereExp, tableType);
        }

    }
}
