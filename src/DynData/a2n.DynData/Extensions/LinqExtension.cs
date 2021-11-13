
using a2n.DynData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#nullable disable

namespace System.Linq
{
    public static class LinqExtension
    {
        private static MethodInfo mtdOrderByGeneric = null;
        private static MethodInfo mtdWhereGeneric = null;

        public static PagingResult GetPagingResult<T>(this IQueryable<T> source, Func<T, object> selector, int pageSize = 20, int pageIndex = 0, object context = null)
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
        public static PagingResult<T> GetPagingResult<T>(this IQueryable<T> source, int pageSize = 20, int pageIndex = 0, object context = null)
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
            {
                return query;
            }

            var lambda = (dynamic)CreateExpression(typeof(TSource), key);

            return ascending
                ? Queryable.OrderBy(query, lambda)
                : Queryable.OrderByDescending(query, lambda);
        }
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return query;
            }

            var lambda = (dynamic)CreateExpression(typeof(TSource), key);

            return ascending
                ? Queryable.ThenBy(query, lambda)
                : Queryable.ThenByDescending(query, lambda);
        }
        public static IQueryable<object> OrderBy(this IQueryable<object> query, string key, Type sourceType, bool ascending = true)
        {
            if (mtdOrderByGeneric == null)
                mtdOrderByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "OrderBy" && t.IsGenericMethod).FirstOrDefault();
            var mtdGenerated = mtdOrderByGeneric.MakeGenericMethod(sourceType);
            return mtdGenerated.Invoke(null, new object[] { query, key, ascending }) as IQueryable<object>;
        }
        public static IQueryable<object> ThenBy(this IQueryable<object> query, string key, Type sourceType, bool ascending = true)
        {
            if (mtdOrderByGeneric == null)
                mtdOrderByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "ThenBy" && t.IsGenericMethod).FirstOrDefault();
            var mtdGenerated = mtdOrderByGeneric.MakeGenericMethod(sourceType);
            return mtdGenerated.Invoke(null, new object[] { query, key, ascending }) as IQueryable<object>;
        }
        public static IQueryable<object> Where(this IQueryable<object> query, object whereExp, Type sourceType)
        {
            if (mtdWhereGeneric == null)
            {
                mtdWhereGeneric = typeof(Queryable).GetMethods().Where(t => t.Name == "Where")
                                .Where(t => t.ToString() == "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])")
                                .FirstOrDefault();
            }
            var mtdGenerated = mtdWhereGeneric.MakeGenericMethod(sourceType);
            return mtdGenerated.Invoke(null, new object[] { query, whereExp }) as IQueryable<object>;
        }
        private static LambdaExpression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "x");

            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda(body, param);
        }
    }
}
