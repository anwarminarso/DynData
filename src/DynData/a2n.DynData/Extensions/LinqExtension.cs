
using a2n.DynData;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#nullable disable

namespace System.Linq
{
    public static class LinqExtension
    {
        private static MethodInfo mtdOrderByGeneric = null;
        private static MethodInfo mtdThenByGeneric = null;
        private static MethodInfo mtdCreateExpression = null;
        private static MethodInfo mtdCreateSelectExpression = null;

        private static MethodInfo mtdQueryableOrderByGeneric = null;
        private static MethodInfo mtdQueryableOrderByDescGeneric = null;
        private static MethodInfo mtdQueryableThenByGeneric = null;
        private static MethodInfo mtdQueryableThenByDescGeneric = null;
        private static MethodInfo mtdQueryableWhereGeneric = null;

        static LinqExtension()
        {
            mtdOrderByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "OrderBy" && t.IsGenericMethod).FirstOrDefault();
            mtdThenByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "ThenBy" && t.IsGenericMethod).FirstOrDefault();
            mtdCreateExpression = typeof(LinqExtension).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(t => t.Name == "CreateExpression" && t.IsGenericMethod).FirstOrDefault();
            mtdCreateSelectExpression = typeof(LinqExtension).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(t => t.Name == "CreateSelectExpression" && t.IsGenericMethod).FirstOrDefault();

            mtdQueryableOrderByGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "OrderBy" && t.GetParameters().Length == 2).FirstOrDefault();
            mtdQueryableOrderByDescGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "OrderByDescending" && t.GetParameters().Length == 2).FirstOrDefault();
            mtdQueryableThenByGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "ThenBy" && t.GetParameters().Length == 2).FirstOrDefault();
            mtdQueryableThenByDescGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "ThenByDescending" && t.GetParameters().Length == 2).FirstOrDefault();
            mtdQueryableWhereGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "Where")
                            .Where(t => t.ToString() == "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])")
                            .FirstOrDefault();
        }

        public static PagingResult ToPagingResult<T>(this IQueryable<T> source, Func<T, object> selector, int pageSize = 20, int pageIndex = 0, object context = null)
        {
            var result = new PagingResult();
            result.pageSize = pageSize;
            result.pageIndex = pageIndex;
            result.totalRows = 0;
            result.totalPages = 0;
            result.context = context;
            if (pageSize <= 0)
                result.pageSize = 20;
            if (result.pageIndex <= 0)
                result.pageIndex = 0;
            result.totalRows = source.Count();
            result.totalPages = (result.totalRows / result.pageSize);
            if (result.totalRows % result.pageSize > 0)
                result.totalPages++;
            if (result.pageIndex + 1 > result.totalPages)
                result.pageIndex = result.totalPages - 1;
            if (result.pageIndex < 0)
                result.pageIndex = 0;
            result.items = source.Skip(result.pageIndex * result.pageSize).Take(result.pageSize).Select(selector).ToArray();
            return result;
        }
        public static PagingResult<T> ToPagingResult<T>(this IQueryable<T> source, int pageSize = 20, int pageIndex = 0, object context = null)
        {
            var result = new PagingResult<T>();
            result.pageSize = pageSize;
            result.pageIndex = pageIndex;
            result.totalRows = 0;
            result.totalPages = 0;
            result.context = context;
            if (pageSize <= 0)
                result.pageSize = 20;
            if (result.pageIndex <= 0)
                result.pageIndex = 0;

            result.totalRows = source.Count();
            result.totalPages = (result.totalRows / result.pageSize);
            if (result.totalRows % result.pageSize > 0)
                result.totalPages++;
            if (result.pageIndex + 1 > result.totalPages)
                result.pageIndex = result.totalPages - 1;
            if (result.pageIndex < 0)
                result.pageIndex = 0;
            result.items = source.Skip(result.pageIndex * result.pageSize).Take(result.pageSize).ToArray();
            return result;
        }

        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return query;
            var propertyType = typeof(TSource).GetProperty(key).PropertyType;
            var fun = CreateExpression(typeof(TSource), propertyType, key);
            MethodInfo mtd = null;
            if (ascending)
            {
                mtd = mtdQueryableOrderByGeneric.MakeGenericMethod(typeof(TSource), propertyType);
            }
            else
            {
                mtd = mtdQueryableOrderByDescGeneric.MakeGenericMethod(typeof(TSource), propertyType);
            }
            return mtd.Invoke(null, new object[] { query, fun }) as IQueryable<TSource>;
        }
        public static IQueryable<dynamic> OrderBy(this IQueryable<dynamic> query, string key, Type sourceType, bool ascending = true)
        {
            var mtd = mtdOrderByGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }

        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return query;
            var propertyType = typeof(TSource).GetProperty(key).PropertyType;
            var fun = CreateExpression(typeof(TSource), propertyType, key);
            MethodInfo mtd = null;
            if (ascending)
            {
                mtd = mtdQueryableThenByGeneric.MakeGenericMethod(typeof(TSource), propertyType);
            }
            else
            {
                mtd = mtdQueryableThenByDescGeneric.MakeGenericMethod(typeof(TSource), propertyType);
            }
            return mtd.Invoke(null, new object[] { query, fun }) as IQueryable<TSource>;
        }
        public static IQueryable<dynamic> ThenBy(this IQueryable<dynamic> query, string key, Type sourceType, bool ascending = true)
        {
            var mtd = mtdThenByGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }

        public static IQueryable<object> Where(this IQueryable<object> query, object whereExp, Type sourceType)
        {
            var mtd = mtdQueryableWhereGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, whereExp }) as IQueryable<object>;
        }

        public static IQueryable<object> Select(this IQueryable<object> query, Type sourceType, params string[] fieldNames)
        {
            var mtd = mtdCreateSelectExpression.MakeGenericMethod(sourceType);
            var selector = mtd.Invoke(null, new object[] { fieldNames }) as Expression<Func<object, object>>;
            return query.Select(selector);
        }
        private static Expression<Func<T, dynamic>> CreateSelectExpression<T>(params string[] fieldNames)
        {
            if (fieldNames == null || fieldNames.Length == 0)
                throw new ArgumentNullException(nameof(fieldNames));
            Type sourceType = typeof(T);
            Type outputType = null; 
            var _fieldNameDist = fieldNames.Distinct().ToArray();
            string[] _fieldNames = fieldNames;
            if (fieldNames.Length != _fieldNameDist.Length)
                _fieldNames = _fieldNameDist;
            //object exObj = new ExpandoObject();
            //var dicExpObj =  exObj as IDictionary<string, object>;
            //foreach (var field in _fieldNames)
            //{
            //    dicExpObj.Add(field, null);
            //}
            //outputType = exObj.GetType();
            var xParameter = Expression.Parameter(sourceType, "t");

            var xNew = Expression.New(sourceType);
            // create initializers
            var bindings = _fieldNames.Select(t =>
            {
                var mi = typeof(T).GetProperty(t);
                var xOriginal = Expression.Property(xParameter, mi);
                return Expression.Bind(mi, xOriginal);
            });

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);
            // expression "t => new T { Field1 = t.Field1, Field2 = t.Field2 }"
            var lambda = Expression.Lambda<Func<T, dynamic>>(xInit, xParameter);
            return lambda;
        }
        
        private static object CreateExpression(Type sourceType, Type propertyType, string propertyName)
        {
            var mtd = mtdCreateExpression.MakeGenericMethod(sourceType, propertyType);
            return mtd.Invoke(null, new object[] { propertyName });
        }
        private static Expression<Func<TSource, TKey>> CreateExpression<TSource, TKey>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TSource), "x");
            Expression body = Expression.PropertyOrField(param, propertyName);
            return (Expression<Func<TSource, TKey>>)Expression.Lambda(body, param);
        }

    }
}
