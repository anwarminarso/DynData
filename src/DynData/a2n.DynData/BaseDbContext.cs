using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace a2n.DynData
{
    public abstract class BaseDbContext : DbContext
    {
        private static object lockObj = new object();

        private static MethodInfo mtdEntryWithoutDetectChanges;
        private static readonly Dictionary<Type, Dictionary<string, Type>> dicTables;
        private static readonly Dictionary<Type, Dictionary<string, Metadata[]>> dicMetadata;

        public DatabaseServer DBSetting { get; set; }
        public ServerVersion MySqlVersion { get; set; }

        public BaseDbContext()
        {
        }
        public BaseDbContext(DatabaseServer DBSetting)
        {
            this.DBSetting = DBSetting;
        }
        protected BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        static BaseDbContext()
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
                        metadataLst.Add(new Metadata(p));
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
                this.DBSetting = extension.DBSetting;
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
    }
}
