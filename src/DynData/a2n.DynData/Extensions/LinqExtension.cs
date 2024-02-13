
using a2n.DynData;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace System.Linq
{
    public static class LinqExtension
    {
        private static MethodInfo mtdOrderByGeneric = null;
        private static MethodInfo mtdOrderBy = null;
        private static MethodInfo mtdThenByGeneric = null;
        private static MethodInfo mtdCreateExpression = null;
        private static MethodInfo mtdCreateSelectExpression = null;

        private static MethodInfo mtdQueryableSelectByGeneric = null;
        private static MethodInfo mtdQueryableSelectManyByGeneric;
        private static MethodInfo mtdQueryableOrderByGeneric = null;
        private static MethodInfo mtdQueryableOrderByDescGeneric = null;
        private static MethodInfo mtdQueryableThenByGeneric = null;
        private static MethodInfo mtdQueryableThenByDescGeneric = null;
        private static MethodInfo mtdQueryableWhereGeneric = null;

        private static MethodInfo mtdGroupJoin;
        private static MethodInfo mtdJoin;
        private static MethodInfo mtdUnion;
        private static MethodInfo mtdDefaultIfEmptyGeneric;

        static LinqExtension()
        {
            mtdOrderByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "OrderBy" && t.IsGenericMethod).FirstOrDefault();
            mtdOrderBy = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            mtdThenByGeneric = typeof(LinqExtension).GetMethods().Where(t => t.Name == "ThenBy" && t.IsGenericMethod).FirstOrDefault();
            mtdCreateExpression = typeof(LinqExtension).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(t => t.Name == "CreateExpression" && t.IsGenericMethod).FirstOrDefault();
            mtdCreateSelectExpression = typeof(LinqExtension).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(t => t.Name == "CreateSelectExpression" && t.IsGenericMethod).FirstOrDefault();

            mtdGroupJoin = typeof(Queryable).GetMethods().Where(t => t.Name.StartsWith("GroupJoin") && t.GetParameters().Length == 5).FirstOrDefault();
            mtdDefaultIfEmptyGeneric = typeof(Enumerable).GetMethods().Where(t => t.Name.StartsWith("DefaultIfEmpty") && t.GetParameters().Length == 1).FirstOrDefault();
            mtdJoin = typeof(Queryable).GetMethods().Where(t => t.Name.StartsWith("Join") && t.GetParameters().Length == 5).FirstOrDefault();
            mtdUnion = typeof(Queryable).GetMethods().Where(t => t.Name.StartsWith("Union") && t.GetParameters().Length == 2).FirstOrDefault();
           

            mtdQueryableSelectByGeneric = typeof(Queryable).GetMethods()
                    .Where(t => t.Name == "Select" 
                            && t.ToString() == "System.Linq.IQueryable`1[TResult] Select[TSource,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TResult]])")
                    .FirstOrDefault();
            mtdQueryableSelectManyByGeneric = typeof(Queryable).GetMethods()
                    .Where(t => t.Name == "SelectMany"
                        && t.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
                    .FirstOrDefault();
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
            var elementType = query.ElementType;
            var propertyType = elementType.GetProperty(key).PropertyType;
            var fun = CreateExpression(elementType, propertyType, key);
            MethodInfo mtd = null;
            if (ascending)
            {
                mtd = mtdQueryableOrderByGeneric.MakeGenericMethod(elementType, propertyType);
            }
            else
            {
                mtd = mtdQueryableOrderByDescGeneric.MakeGenericMethod(elementType, propertyType);
            }
            return mtd.Invoke(null, new object[] { query, fun }) as IQueryable<TSource>;
        }
        public static IQueryable<dynamic> OrderBy(this IQueryable<dynamic> query, string key, Type sourceType, bool ascending = true)
        {
            var mtd = mtdOrderByGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }
        public static IQueryable OrderBy(this IQueryable query, string key, bool ascending = true)
        {
            var mtd = mtdOrderByGeneric.MakeGenericMethod(query.ElementType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = getPropertyInfo(typeof(T), name);
            var expr = getOrderExpression(typeof(T), propInfo);

            var genericMethod = mtdOrderBy.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            var propInfo = getPropertyInfo(typeof(T), name);
            var expr = getOrderExpression(typeof(T), propInfo);

            var genericMethod = mtdOrderBy.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return query;
            var elementType = query.ElementType;
            var propertyType = elementType.GetProperty(key).PropertyType;
            var fun = CreateExpression(elementType, propertyType, key);
            MethodInfo mtd = null;
            if (ascending)
            {
                mtd = mtdQueryableThenByGeneric.MakeGenericMethod(elementType, propertyType);
            }
            else
            {
                mtd = mtdQueryableThenByDescGeneric.MakeGenericMethod(elementType, propertyType);
            }
            return mtd.Invoke(null, new object[] { query, fun }) as IQueryable<TSource>;
        }
        public static IQueryable<dynamic> ThenBy(this IQueryable<dynamic> query, string key, Type sourceType, bool ascending = true)
        {
            var mtd = mtdThenByGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }
        public static IQueryable ThenBy(this IQueryable query, string key, bool ascending = true)
        {
            var mtd = mtdThenByGeneric.MakeGenericMethod(query.ElementType);
            return mtd.Invoke(null, new object[] { query, key, ascending }) as IQueryable<dynamic>;
        }

        public static IQueryable<object> Where(this IQueryable<object> query, object whereExp, Type sourceType)
        {
            var mtd = mtdQueryableWhereGeneric.MakeGenericMethod(sourceType);
            return mtd.Invoke(null, new object[] { query, whereExp }) as IQueryable<object>;
        }

        public static IQueryable Where(this IQueryable query, object whereExp)
        {
            var mtd = mtdQueryableWhereGeneric.MakeGenericMethod(query.ElementType);
            return mtd.Invoke(null, new object[] { query, whereExp }) as IQueryable<object>;
        }

        public static IQueryable<object> Select(this IQueryable<object> query, Type sourceType, params string[] fieldNames)
        {
            var mtd = mtdCreateSelectExpression.MakeGenericMethod(sourceType);
            var selectorObj = mtd.Invoke(null, new object[] { fieldNames });
            mtd = mtdQueryableSelectByGeneric.MakeGenericMethod(sourceType, typeof(object));
            return mtd.Invoke(null, new object[] { query, selectorObj }) as IQueryable<object>;
        }
        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var parent in source)
            {
                yield return parent;

                var children = selector(parent);
                foreach (var child in SelectRecursive(children, selector))
                    yield return child;
            }
        }

        public static IQueryable Select(this IQueryable qry, params string[] propertyNames)
        {
            var elementType = qry.ElementType;
            IQueryable result = null;
            var props = (from t in propertyNames
                         select (t, elementType.GetProperty(t).PropertyType)).ToArray();
            Type outputType = AnonymousType.CreateType(props, out var constructorInfo);

            #region Select
            {
                var parOutput = Expression.Parameter(elementType, "x");
                List<MemberBinding> bindings = new List<MemberBinding>();
                Expression[] memberExps = propertyNames.Select(t => Expression.Property(parOutput, t)).ToArray();

                var outputNew = Expression.New(constructorInfo, memberExps);
                var funcTypeResult = typeof(Func<,>).MakeGenericType(elementType, outputType);
                var lambdaResult = Expression.Lambda(funcTypeResult, outputNew, parOutput);
                var mtdOutput = mtdQueryableSelectByGeneric.MakeGenericMethod(elementType, outputType);
                result = mtdOutput.Invoke(null, new object[] { qry, lambdaResult }) as IQueryable;
            }
            #endregion
            return result;
        }
        public static IQueryable Select(this IQueryable qry, params (string, string)[] bindingParams)
        {
            var elementType = qry.ElementType;
            List<(string, Type)> props = new List<(string, Type)>();
            foreach (var bindingParam in bindingParams)
            {
                var propName = bindingParam.Item2;
                var fragments = propName.Split('.');
                Type propertyType = null;
                foreach (var fragment in fragments)
                {
                    if (propertyType == null)
                        propertyType = elementType.GetProperty(fragment).PropertyType;
                    else
                        propertyType = propertyType.GetProperty(fragment).PropertyType;
                }
                props.Add((bindingParam.Item1, propertyType));
            }
            Type outputType = AnonymousType.CreateType(props, true);
            return Select(qry, outputType, bindingParams);
        }
        public static IQueryable<T> Select<T>(this IQueryable qry, params (string, string)[] bindingParams)
            where T : class, new()
        {
            //var elementType = qry.ElementType;
            return Select(qry, typeof(T), bindingParams) as IQueryable<T>;
        }
        public static IQueryable Select(this IQueryable qry, Type outputType, params (string, string)[] bindingParams)
        {
            IQueryable result = null;
            var outputNew = Expression.New(outputType.GetConstructor(Type.EmptyTypes));

            #region Select
            {
                var elementType = qry.ElementType;
                var parOutput = Expression.Parameter(elementType, "x");
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var bindingParam in bindingParams)
                {
                    var value = bindingParam.Item2;
                    var bindingProp = outputType.GetProperty(bindingParam.Item1);

                    if (!string.IsNullOrEmpty(value))
                    {
                        var fragments = value.Split('.');
                        Expression memberExp = null;
                        foreach (var item in fragments)
                        {
                            if (memberExp == null)
                                memberExp = Expression.Property(parOutput, item);
                            else
                                memberExp = Expression.Property(memberExp, item);
                        }
                        if (bindingProp.PropertyType == memberExp.Type)
                            bindings.Add(Expression.Bind(bindingProp, memberExp));
                        else
                        {
                            var nullableProp = Nullable.GetUnderlyingType(bindingProp.PropertyType);
                            var nullableMember = Nullable.GetUnderlyingType(memberExp.Type);
                            if (nullableProp != null && nullableProp == memberExp.Type)
                                bindings.Add(Expression.Bind(bindingProp, Expression.Convert(memberExp, bindingProp.PropertyType)));
                            else if (nullableMember != null && nullableMember == bindingProp.PropertyType)
                                bindings.Add(Expression.Bind(bindingProp, Expression.Call(memberExp, memberExp.Type.GetMethod("GetValueOrDefault", new Type[] { }))));
                            else
                            {
                                throw new ArgumentException("bindingParams must have same type");
                            }
                        }
                    }
                    else
                    {
                        ConstantExpression constValue = null;
                        if (bindingProp.PropertyType.IsValueType)
                            constValue = Expression.Constant(Activator.CreateInstance(bindingProp.PropertyType), bindingProp.PropertyType);
                        else
                            constValue = Expression.Constant(null, bindingProp.PropertyType);
                        bindings.Add(Expression.Bind(bindingProp, constValue));
                    }
                }
                var outputInit = Expression.MemberInit(outputNew, bindings);
                var funcTypeResult = typeof(Func<,>).MakeGenericType(elementType, outputType);
                var lambdaResult = Expression.Lambda(funcTypeResult, outputInit, parOutput);
                var mtdOutput = mtdQueryableSelectByGeneric.MakeGenericMethod(elementType, outputType);
                result = mtdOutput.Invoke(null, new object[] { qry, lambdaResult }) as IQueryable;
            }
            #endregion
            return result;
        }

        public static IQueryable InnerJoin(this IQueryable qry1, IQueryable qry2, params (string, string)[] joinParams)
        {
            #region Validation
            if (joinParams == null || joinParams.Length == 0)
                throw new ArgumentNullException("LeftPropertyNames");
            #endregion

            #region Variables
            IQueryable<object> result = null;
            Type outputType = null;
            Type[] leftTypes = null;
            Type[] rightTypes = null;
            var leftType = qry1.ElementType;
            var rightType = qry2.ElementType;
            var outputOptions = new List<(string, Type)>();
            if (leftType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = leftType.GetProperties();
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx), t.PropertyType)).ToArray());
                leftTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                leftTypes = new[] { leftType };
                outputOptions.Add((getAlias(outputOptions.Count), leftType));
            }
            if (rightType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = rightType.GetProperties();
                var cnt = outputOptions.Count;
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx + cnt), t.PropertyType)).ToArray());
                rightTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                rightTypes = new[] { rightType };
                outputOptions.Add((getAlias(outputOptions.Count), rightType));
            }

            outputType = AnonymousType.CreateType(outputOptions, true, true);
            var outputTypeCount = outputType.GenericTypeArguments.Length;
            #endregion

            #region Join
            {
                LambdaExpression lambda1, lambda2, lambda3;
                Type funcType1, funcType2, outputJoinType;
                var funcType = typeof(Func<,,>).MakeGenericType(leftType, rightType, outputType);
                var par1 = Expression.Parameter(leftType, "x");
                var par2 = Expression.Parameter(rightType, "y");
                {
                    List<Expression> pars = new List<Expression>();
                    Expression leftInit, rightInit;
                    List<(string, Type, Expression, Expression)> bindingMeta = new List<(string, Type, Expression, Expression)>();
                    for (int i = 0; i < joinParams.Length; i++)
                    {
                        Type _leftType, _rightType;
                        var leftIdx = 0;
                        var rightIdx = 0;
                        PropertyInfo LeftProp = null;
                        PropertyInfo RightProp = null;
                        Expression new1 = null;
                        Expression new2 = null;
                        var leftPropName = joinParams[i].Item1.Split(".").Last();
                        var rightPropName = joinParams[i].Item2.Split(".").Last();
                        if (joinParams[i].Item1.Contains(".") && leftTypes.Length > 1)
                            leftIdx = getAliasIndex(joinParams[i].Item1.Split(".").First());
                        _leftType = leftTypes[leftIdx];
                        if (joinParams[i].Item2.Contains(".") && rightTypes.Length > 1)
                            rightIdx = getAliasIndex(joinParams[i].Item2.Split(".").First()) - leftTypes.Length;
                        _rightType = rightTypes[rightIdx];
                        LeftProp = _leftType.GetProperty(leftPropName);
                        RightProp = _rightType.GetProperty(rightPropName);
                        if (leftTypes.Length > 1)
                            new1 = Expression.Property(Expression.Property(par1, getAlias(leftIdx)), LeftProp.Name);
                        else
                            new1 = Expression.Property(par1, LeftProp.Name);
                        if (rightTypes.Length > 1)
                            new2 = Expression.Property(Expression.Property(par2, getAlias(rightIdx)), RightProp.Name);
                        else
                            new2 = Expression.Property(par2, RightProp.Name);

                        var nullableLeft = Nullable.GetUnderlyingType(LeftProp.PropertyType);
                        var nullableRight = Nullable.GetUnderlyingType(RightProp.PropertyType);
                        Type outputLeftType = LeftProp.PropertyType;
                        Type outputRightType = RightProp.PropertyType;
                        var outputSubJoinType = LeftProp.PropertyType;

                        if (outputLeftType != outputRightType)
                        {
                            if (nullableLeft != null && nullableLeft == outputRightType)
                                outputSubJoinType = outputRightType;
                            else if (nullableRight != null && nullableRight == outputLeftType)
                                outputSubJoinType = outputLeftType;
                            else
                                throw new ArgumentException("LeftPropertyNames and RightPropertyNames must have same type");
                        }
                        if (outputSubJoinType != outputLeftType)
                            new1 = Expression.Call(new1, outputLeftType.GetMethod("GetValueOrDefault", new Type[] { }));
                        if (outputSubJoinType != outputRightType)
                            new2 = Expression.Call(new2, outputRightType.GetMethod("GetValueOrDefault", new Type[] { }));
                        bindingMeta.Add(new($"{getAlias(i)}", outputSubJoinType, new1, new2));
                    }


                    if (joinParams.Length == 1)
                    {
                        outputJoinType = bindingMeta[0].Item2;
                        leftInit = bindingMeta[0].Item3;
                        rightInit = bindingMeta[0].Item4;
                    }
                    else
                    {
                        outputJoinType = AnonymousType.CreateType(bindingMeta.Select(t => (t.Item1, t.Item2)).ToArray());

                        leftInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item3).ToArray());

                        rightInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item4).ToArray());
                    }
                    funcType1 = typeof(Func<,>).MakeGenericType(leftType, outputJoinType);
                    funcType2 = typeof(Func<,>).MakeGenericType(rightType, outputJoinType);

                    List<MemberBinding> bindings = new List<MemberBinding>();
                    for (int i = 0; i < leftTypes.Length; i++)
                    {
                        var alias = getAlias(i);
                        if (leftTypes.Length > 1)
                        {
                            var prop = Expression.Property(par1, leftType.GetProperty(alias));
                            bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                        }
                        else
                            bindings.Add(Expression.Bind(outputType.GetProperty(alias), par1));

                    }
                    for (int i = 0; i < rightTypes.Length; i++)
                    {
                        if (rightTypes.Length > 1)
                        {
                            var alias = getAlias(leftTypes.Length + i);
                            var prop = Expression.Property(par2, rightType.GetProperty(getAlias(i)));
                            bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                        }
                        else
                            bindings.Add(Expression.Bind(outputType.GetProperty(getAlias(leftTypes.Length)), par2));

                    }
                    var outputInitExp = Expression.MemberInit(Expression.New(outputType.GetConstructor(Type.EmptyTypes)), bindings);

                    lambda1 = Expression.Lambda(funcType1, leftInit, par1);
                    lambda2 = Expression.Lambda(funcType2, rightInit, par2);
                    lambda3 = Expression.Lambda(funcType, outputInitExp, par1, par2);
                }
                var mtd = mtdJoin.MakeGenericMethod(leftType, rightType, outputJoinType, outputType);

                result = mtd.Invoke(null, new object[] { qry1, qry2, lambda1, lambda2, lambda3 }) as IQueryable<object>;

            }
            #endregion
            return result;
        }

        public static IQueryable LeftJoin(this IQueryable qry1, IQueryable qry2, params (string, string)[] joinParams)
        {
            #region Validation
            if (joinParams == null || joinParams.Length == 0)
                throw new ArgumentNullException("joinParams");
            #endregion

            #region Variables
            IQueryable<object> result = null;
            IQueryable<object> qrySelectGroup;
            Type outputType = null;
            Type[] leftTypes = null;
            Type[] rightTypes = null;
            var leftType = qry1.ElementType;
            var rightType = qry2.ElementType;
            Type groupJoinType = null;
            var enumType = typeof(IEnumerable<>).MakeGenericType(rightType);
            var outputOptions = new List<(string, Type)>();
            if (leftType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = leftType.GetProperties();
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx), t.PropertyType)).ToArray());
                leftTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                leftTypes = new[] { leftType };
                outputOptions.Add((getAlias(outputOptions.Count), leftType));
            }
            if (rightType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = rightType.GetProperties();
                var cnt = outputOptions.Count;
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx + cnt), t.PropertyType)).ToArray());
                rightTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                rightTypes = new[] { rightType };
                outputOptions.Add((getAlias(outputOptions.Count), rightType));
            }

            outputType = AnonymousType.CreateType(outputOptions, true, true);
            var outputTypeCount = outputType.GenericTypeArguments.Length;
            groupJoinType = AnonymousType.CreateType(new[] { ("u", leftType), ("v", enumType) }, true, true);
            #endregion

            #region Group Join
            {
                LambdaExpression lambda1, lambda2, lambda3;
                Type funcType1, funcType2, outputJoinType;
                var par1 = Expression.Parameter(leftType, "x");
                var par2 = Expression.Parameter(rightType, "y");
                var par3 = Expression.Parameter(enumType, "z");
                var funcType = typeof(Func<,,>).MakeGenericType(leftType, enumType, groupJoinType);
                var groupJoinInitExp = Expression.MemberInit(Expression.New(groupJoinType),
                     Expression.Bind(groupJoinType.GetProperty("u"), par1),
                     Expression.Bind(groupJoinType.GetProperty("v"), par3));
                {
                    List<(string, Type, Expression, Expression)> bindingMeta = new List<(string, Type, Expression, Expression)>();
                    Expression leftInit, rightInit;
                    for (int i = 0; i < joinParams.Length; i++)
                    {
                        Type _leftType, _rightType;
                        var leftIdx = 0;
                        var rightIdx = 0;
                        PropertyInfo LeftProp = null;
                        PropertyInfo RightProp = null;
                        Expression new1 = null;
                        Expression new2 = null;
                        var leftPropName = joinParams[i].Item1.Split(".").Last();
                        var rightPropName = joinParams[i].Item2.Split(".").Last();
                        if (joinParams[i].Item1.Contains(".") && leftTypes.Length > 1)
                            leftIdx = getAliasIndex(joinParams[i].Item1.Split(".").First());
                        _leftType = leftTypes[leftIdx];
                        if (joinParams[i].Item2.Contains(".") && rightTypes.Length > 1)
                            rightIdx = getAliasIndex(joinParams[i].Item2.Split(".").First()) - leftTypes.Length;
                        _rightType = rightTypes[rightIdx];
                        LeftProp = _leftType.GetProperty(leftPropName);
                        RightProp = _rightType.GetProperty(rightPropName);
                        if (leftTypes.Length > 1)
                            new1 = Expression.Property(Expression.Property(par1, getAlias(leftIdx)), LeftProp.Name);
                        else
                            new1 = Expression.Property(par1, LeftProp.Name);
                        if (rightTypes.Length > 1)
                            new2 = Expression.Property(Expression.Property(par2, getAlias(rightIdx)), RightProp.Name);
                        else
                            new2 = Expression.Property(par2, RightProp.Name);

                        var nullableLeft = Nullable.GetUnderlyingType(LeftProp.PropertyType);
                        var nullableRight = Nullable.GetUnderlyingType(RightProp.PropertyType);
                        Type outputLeftType = LeftProp.PropertyType;
                        Type outputRightType = RightProp.PropertyType;
                        var outputSubJoinType = LeftProp.PropertyType;

                        if (outputLeftType != outputRightType)
                        {
                            if (nullableLeft != null && nullableLeft == outputRightType)
                                outputSubJoinType = outputRightType;
                            else if (nullableRight != null && nullableRight == outputLeftType)
                                outputSubJoinType = outputLeftType;
                            else
                                throw new ArgumentException("LeftPropertyNames and RightPropertyNames must have same type");
                        }
                        if (outputSubJoinType != outputLeftType)
                            new1 = Expression.Call(new1, outputLeftType.GetMethod("GetValueOrDefault", new Type[] { }));
                        if (outputSubJoinType != outputRightType)
                            new2 = Expression.Call(new2, outputRightType.GetMethod("GetValueOrDefault", new Type[] { }));
                        bindingMeta.Add(new($"{getAlias(i)}", outputSubJoinType, new1, new2));
                    }

                    if (joinParams.Length == 1)
                    {
                        outputJoinType = bindingMeta[0].Item2;
                        leftInit = bindingMeta[0].Item3;
                        rightInit = bindingMeta[0].Item4;
                    }
                    else
                    {
                        outputJoinType = AnonymousType.CreateType(bindingMeta.Select(t => (t.Item1, t.Item2)).ToArray());

                        leftInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item3).ToArray());

                        rightInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item4).ToArray());
                    }

                    funcType1 = typeof(Func<,>).MakeGenericType(leftType, outputJoinType);
                    funcType2 = typeof(Func<,>).MakeGenericType(rightType, outputJoinType);

                    lambda1 = Expression.Lambda(funcType1, leftInit, par1);
                    lambda2 = Expression.Lambda(funcType2, rightInit, par2);
                    lambda3 = Expression.Lambda(funcType, groupJoinInitExp, par1, par3);
                }
                var mtd = mtdGroupJoin.MakeGenericMethod(leftType, rightType, outputJoinType, groupJoinType);
                qrySelectGroup = mtd.Invoke(null, new object[] { qry1, qry2, lambda1, lambda2, lambda3 }) as IQueryable<object>;
            }
            #endregion
            #region SelectMany
            {
                var parOutput1 = Expression.Parameter(groupJoinType, "x");
                var parOutput2 = Expression.Parameter(rightType, "y");
                var parSelectMany = Expression.Parameter(groupJoinType, "z");
                var mtdDefaultIfEmpty = mtdDefaultIfEmptyGeneric.MakeGenericMethod(rightType);
                var manyInit = Expression.Call(null, mtdDefaultIfEmpty, Expression.Property(parSelectMany, "v"));
                var funcTypeSelectMany = typeof(Func<,>).MakeGenericType(groupJoinType, enumType);
                var lambdaSelectMany = Expression.Lambda(funcTypeSelectMany, manyInit, parSelectMany);

                List<MemberBinding> bindings = new List<MemberBinding>();
                for (int i = 0; i < leftTypes.Length; i++)
                {
                    var alias = getAlias(i);
                    if (leftTypes.Length > 1)
                    {
                        var prop = Expression.Property(Expression.Property(parOutput1, "u"), leftType.GetProperty(alias));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), Expression.Property(parOutput1, "u")));
                    }

                }
                for (int i = 0; i < rightTypes.Length; i++)
                {
                    if (rightTypes.Length > 1)
                    {
                        var alias = getAlias(leftTypes.Length + i);
                        var prop = Expression.Property(parOutput2, rightType.GetProperty(getAlias(i)));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(getAlias(leftTypes.Length)), parOutput2));
                    }
                }
                var outputInitExp = Expression.MemberInit(Expression.New(outputType.GetConstructor(Type.EmptyTypes)), bindings);

                var funcTypeResult = typeof(Func<,,>).MakeGenericType(groupJoinType, rightType, outputType);
                var lambdaResult = Expression.Lambda(funcTypeResult, outputInitExp, parOutput1, parOutput2);
                var mtdOutput = mtdQueryableSelectManyByGeneric.MakeGenericMethod(groupJoinType, rightType, outputType);
                result = mtdOutput.Invoke(null, new object[] { qrySelectGroup, lambdaSelectMany, lambdaResult }) as IQueryable<object>;

            }
            #endregion

            return result;
        }
        public static IQueryable RightJoin(this IQueryable qry1, IQueryable qry2, params (string, string)[] joinParams)
        {
            #region Validation
            if (joinParams == null || joinParams.Length == 0)
                throw new ArgumentNullException("joinParams");
            #endregion

            #region Variables
            IQueryable<object> result = null;
            IQueryable<object> qrySelectGroup;
            Type outputType = null;
            Type[] leftTypes = null;
            Type[] rightTypes = null;
            var leftType = qry2.ElementType;
            var rightType = qry1.ElementType;
            Type groupJoinType = null;
            var enumType = typeof(IEnumerable<>).MakeGenericType(rightType);
            var outputOptions = new List<(string, Type)>();
            if (rightType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = rightType.GetProperties();
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx), t.PropertyType)).ToArray());
                rightTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                rightTypes = new[] { rightType };
                outputOptions.Add((getAlias(outputOptions.Count), rightType));
            }
            if (leftType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = leftType.GetProperties();
                var cnt = outputOptions.Count;
                outputOptions.AddRange(properties.Select((t, idx) => (getAlias(idx + cnt), t.PropertyType)).ToArray());
                leftTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                leftTypes = new[] { leftType };
                outputOptions.Add((getAlias(outputOptions.Count), leftType));
            }

            outputType = AnonymousType.CreateType(outputOptions, true, true);
            var outputTypeCount = outputType.GenericTypeArguments.Length;
            groupJoinType = AnonymousType.CreateType(new[] { ("u", leftType), ("v", enumType) }, true, false);
            #endregion

            #region Group Join
            {
                LambdaExpression lambda1, lambda2, lambda3;
                Type funcType1, funcType2, outputJoinType;
                var par1 = Expression.Parameter(leftType, "x");
                var par2 = Expression.Parameter(rightType, "y");
                var par3 = Expression.Parameter(enumType, "z");
                var funcType = typeof(Func<,,>).MakeGenericType(leftType, enumType, groupJoinType);
                var groupJoinInitExp = Expression.MemberInit(Expression.New(groupJoinType),
                     Expression.Bind(groupJoinType.GetProperty("u"), par1),
                     Expression.Bind(groupJoinType.GetProperty("v"), par3));
                //var groupJoinInitExp = Expression.MemberInit(Expression.New(groupJoinType.GetConstructor(new Type[] { leftType, enumType }), par1, par3));

                {

                    List<(string, Type, Expression, Expression)> bindingMeta = new List<(string, Type, Expression, Expression)>();
                    Expression leftInit, rightInit;
                    for (int i = 0; i < joinParams.Length; i++)
                    {
                        Type _leftType, _rightType;
                        var leftIdx = 0;
                        var rightIdx = 0;
                        PropertyInfo LeftProp = null;
                        PropertyInfo RightProp = null;
                        Expression new1 = null;
                        Expression new2 = null;
                        var leftPropName = joinParams[i].Item2.Split(".").Last();
                        var rightPropName = joinParams[i].Item1.Split(".").Last();
                        if (joinParams[i].Item2.Contains(".") && leftTypes.Length > 1)
                            leftIdx = getAliasIndex(joinParams[i].Item2.Split(".").First()) - rightTypes.Length;
                        _leftType = leftTypes[leftIdx];
                        if (joinParams[i].Item1.Contains(".") && rightTypes.Length > 1)
                            rightIdx = getAliasIndex(joinParams[i].Item1.Split(".").First());
                        _rightType = rightTypes[rightIdx];
                        LeftProp = _leftType.GetProperty(leftPropName);
                        RightProp = _rightType.GetProperty(rightPropName);
                        if (leftTypes.Length > 1)
                            new1 = Expression.Property(Expression.Property(par1, getAlias(leftIdx)), LeftProp.Name);
                        else
                            new1 = Expression.Property(par1, LeftProp.Name);
                        if (rightTypes.Length > 1)
                            new2 = Expression.Property(Expression.Property(par2, getAlias(rightIdx)), RightProp.Name);
                        else
                            new2 = Expression.Property(par2, RightProp.Name);

                        var nullableLeft = Nullable.GetUnderlyingType(LeftProp.PropertyType);
                        var nullableRight = Nullable.GetUnderlyingType(RightProp.PropertyType);
                        Type outputLeftType = LeftProp.PropertyType;
                        Type outputRightType = RightProp.PropertyType;
                        var outputSubJoinType = LeftProp.PropertyType;

                        if (outputLeftType != outputRightType)
                        {
                            if (nullableLeft != null && nullableLeft == outputRightType)
                                outputSubJoinType = outputRightType;
                            else if (nullableRight != null && nullableRight == outputLeftType)
                                outputSubJoinType = outputLeftType;
                            else
                                throw new ArgumentException("LeftPropertyNames and RightPropertyNames must have same type");
                        }
                        if (outputSubJoinType != outputLeftType)
                            new1 = Expression.Call(new1, outputLeftType.GetMethod("GetValueOrDefault", new Type[] { }));
                        if (outputSubJoinType != outputRightType)
                            new2 = Expression.Call(new2, outputRightType.GetMethod("GetValueOrDefault", new Type[] { }));
                        bindingMeta.Add(new($"{getAlias(i)}", outputSubJoinType, new1, new2));
                    }

                    if (joinParams.Length == 1)
                    {
                        outputJoinType = bindingMeta[0].Item2;
                        leftInit = bindingMeta[0].Item3;
                        rightInit = bindingMeta[0].Item4;
                    }
                    else
                    {
                        outputJoinType = AnonymousType.CreateType(bindingMeta.Select(t => (t.Item1, t.Item2)).ToArray());

                        leftInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item3).ToArray());

                        rightInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item4).ToArray());
                    }

                    funcType1 = typeof(Func<,>).MakeGenericType(leftType, outputJoinType);
                    funcType2 = typeof(Func<,>).MakeGenericType(rightType, outputJoinType);

                    lambda1 = Expression.Lambda(funcType1, leftInit, par1);
                    lambda2 = Expression.Lambda(funcType2, rightInit, par2);
                    lambda3 = Expression.Lambda(funcType, groupJoinInitExp, par1, par3);
                }
                var mtd = mtdGroupJoin.MakeGenericMethod(leftType, rightType, outputJoinType, groupJoinType);
                qrySelectGroup = mtd.Invoke(null, new object[] { qry2, qry1, lambda1, lambda2, lambda3 }) as IQueryable<object>;
            }
            #endregion
            #region SelectMany
            {
                var parOutput1 = Expression.Parameter(groupJoinType, "x");
                var parOutput2 = Expression.Parameter(rightType, "y");
                var parSelectMany = Expression.Parameter(groupJoinType, "z");
                var mtdDefaultIfEmpty = mtdDefaultIfEmptyGeneric.MakeGenericMethod(rightType);
                var manyInit = Expression.Call(null, mtdDefaultIfEmpty, Expression.Property(parSelectMany, "v"));
                var funcTypeSelectMany = typeof(Func<,>).MakeGenericType(groupJoinType, enumType);
                var lambdaSelectMany = Expression.Lambda(funcTypeSelectMany, manyInit, parSelectMany);

                List<MemberBinding> bindings = new List<MemberBinding>();

                for (int i = 0; i < rightTypes.Length; i++)
                {
                    var alias = getAlias(i);
                    if (rightTypes.Length > 1)
                    {
                        var prop = Expression.Property(parOutput2, rightType.GetProperty(alias));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), parOutput2));
                    }
                }
                for (int i = 0; i < leftTypes.Length; i++)
                {
                    if (leftTypes.Length > 1)
                    {
                        var alias = getAlias(rightTypes.Length + i);
                        var prop = Expression.Property(Expression.Property(parOutput1, "u"), leftType.GetProperty(getAlias(i)));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(getAlias(rightTypes.Length)), Expression.Property(parOutput1, "u")));
                    }
                }
                var outputInitExp = Expression.MemberInit(Expression.New(outputType.GetConstructor(Type.EmptyTypes)), bindings);

                var funcTypeResult = typeof(Func<,,>).MakeGenericType(groupJoinType, rightType, outputType);
                var lambdaResult = Expression.Lambda(funcTypeResult, outputInitExp, parOutput1, parOutput2);
                var mtdOutput = mtdQueryableSelectManyByGeneric.MakeGenericMethod(groupJoinType, rightType, outputType);
                result = mtdOutput.Invoke(null, new object[] { qrySelectGroup, lambdaSelectMany, lambdaResult }) as IQueryable<object>;

            }
            #endregion

            return result;
        }

        private static IQueryable RightJoin(this IQueryable qry1, IQueryable qry2, Type outputType, params (string, string)[] joinParams)
        {
            #region Validation
            if (joinParams == null || joinParams.Length == 0)
                throw new ArgumentNullException("joinParams");
            if (!outputType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
                throw new ArgumentNullException("invalid outputType");
            #endregion

            #region Variables
            IQueryable<object> result = null;
            IQueryable<object> qrySelectGroup;
            Type[] leftTypes = null;
            Type[] rightTypes = null;
            var leftType = qry2.ElementType;
            var rightType = qry1.ElementType;
            Type groupJoinType = null;
            var enumType = typeof(IEnumerable<>).MakeGenericType(rightType);
            if (rightType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = rightType.GetProperties();
                rightTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                rightTypes = new[] { rightType };
            }
            if (leftType.CustomAttributes.Any(t => t.AttributeType == typeof(AnonymousLinqAttribute)))
            {
                var properties = leftType.GetProperties();
                leftTypes = properties.Select(t => t.PropertyType).ToArray();
            }
            else
            {
                leftTypes = new[] { leftType };
            }
            var outputTypeCount = outputType.GenericTypeArguments.Length;
            groupJoinType = AnonymousType.CreateType(new[] { ("u", leftType), ("v", enumType) }, true, false);
            #endregion

            #region Group Join
            {
                LambdaExpression lambda1, lambda2, lambda3;
                Type funcType1, funcType2, outputJoinType;
                var par1 = Expression.Parameter(leftType, "x");
                var par2 = Expression.Parameter(rightType, "y");
                var par3 = Expression.Parameter(enumType, "z");
                var funcType = typeof(Func<,,>).MakeGenericType(leftType, enumType, groupJoinType);
                var groupJoinInitExp = Expression.MemberInit(Expression.New(groupJoinType),
                     Expression.Bind(groupJoinType.GetProperty("u"), par1),
                     Expression.Bind(groupJoinType.GetProperty("v"), par3));
                //var groupJoinInitExp = Expression.MemberInit(Expression.New(groupJoinType.GetConstructor(new Type[] { leftType, enumType }), par1, par3));

                {

                    List<(string, Type, Expression, Expression)> bindingMeta = new List<(string, Type, Expression, Expression)>();
                    Expression leftInit, rightInit;
                    for (int i = 0; i < joinParams.Length; i++)
                    {
                        Type _leftType, _rightType;
                        var leftIdx = 0;
                        var rightIdx = 0;
                        PropertyInfo LeftProp = null;
                        PropertyInfo RightProp = null;
                        Expression new1 = null;
                        Expression new2 = null;
                        var leftPropName = joinParams[i].Item2.Split(".").Last();
                        var rightPropName = joinParams[i].Item1.Split(".").Last();
                        if (joinParams[i].Item2.Contains(".") && leftTypes.Length > 1)
                            leftIdx = getAliasIndex(joinParams[i].Item2.Split(".").First()) - rightTypes.Length;
                        _leftType = leftTypes[leftIdx];
                        if (joinParams[i].Item1.Contains(".") && rightTypes.Length > 1)
                            rightIdx = getAliasIndex(joinParams[i].Item1.Split(".").First());
                        _rightType = rightTypes[rightIdx];
                        LeftProp = _leftType.GetProperty(leftPropName);
                        RightProp = _rightType.GetProperty(rightPropName);
                        if (leftTypes.Length > 1)
                            new1 = Expression.Property(Expression.Property(par1, getAlias(leftIdx)), LeftProp.Name);
                        else
                            new1 = Expression.Property(par1, LeftProp.Name);
                        if (rightTypes.Length > 1)
                            new2 = Expression.Property(Expression.Property(par2, getAlias(rightIdx)), RightProp.Name);
                        else
                            new2 = Expression.Property(par2, RightProp.Name);

                        var nullableLeft = Nullable.GetUnderlyingType(LeftProp.PropertyType);
                        var nullableRight = Nullable.GetUnderlyingType(RightProp.PropertyType);
                        Type outputLeftType = LeftProp.PropertyType;
                        Type outputRightType = RightProp.PropertyType;
                        var outputSubJoinType = LeftProp.PropertyType;

                        if (outputLeftType != outputRightType)
                        {
                            if (nullableLeft != null && nullableLeft == outputRightType)
                                outputSubJoinType = outputRightType;
                            else if (nullableRight != null && nullableRight == outputLeftType)
                                outputSubJoinType = outputLeftType;
                            else
                                throw new ArgumentException("LeftPropertyNames and RightPropertyNames must have same type");
                        }
                        if (outputSubJoinType != outputLeftType)
                            new1 = Expression.Call(new1, outputLeftType.GetMethod("GetValueOrDefault", new Type[] { }));
                        if (outputSubJoinType != outputRightType)
                            new2 = Expression.Call(new2, outputRightType.GetMethod("GetValueOrDefault", new Type[] { }));
                        bindingMeta.Add(new($"{getAlias(i)}", outputSubJoinType, new1, new2));
                    }

                    if (joinParams.Length == 1)
                    {
                        outputJoinType = bindingMeta[0].Item2;
                        leftInit = bindingMeta[0].Item3;
                        rightInit = bindingMeta[0].Item4;
                    }
                    else
                    {
                        outputJoinType = AnonymousType.CreateType(bindingMeta.Select(t => (t.Item1, t.Item2)).ToArray());

                        leftInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item3).ToArray());

                        rightInit = Expression.New(
                            outputJoinType.GetConstructor(bindingMeta.Select(t => t.Item2).ToArray()),
                            bindingMeta.Select(t => t.Item4).ToArray());
                    }

                    funcType1 = typeof(Func<,>).MakeGenericType(leftType, outputJoinType);
                    funcType2 = typeof(Func<,>).MakeGenericType(rightType, outputJoinType);

                    lambda1 = Expression.Lambda(funcType1, leftInit, par1);
                    lambda2 = Expression.Lambda(funcType2, rightInit, par2);
                    lambda3 = Expression.Lambda(funcType, groupJoinInitExp, par1, par3);
                }
                var mtd = mtdGroupJoin.MakeGenericMethod(leftType, rightType, outputJoinType, groupJoinType);
                qrySelectGroup = mtd.Invoke(null, new object[] { qry2, qry1, lambda1, lambda2, lambda3 }) as IQueryable<object>;
            }
            #endregion
            #region SelectMany
            {
                var parOutput1 = Expression.Parameter(groupJoinType, "x");
                var parOutput2 = Expression.Parameter(rightType, "y");
                var parSelectMany = Expression.Parameter(groupJoinType, "z");
                var mtdDefaultIfEmpty = mtdDefaultIfEmptyGeneric.MakeGenericMethod(rightType);
                var manyInit = Expression.Call(null, mtdDefaultIfEmpty, Expression.Property(parSelectMany, "v"));
                var funcTypeSelectMany = typeof(Func<,>).MakeGenericType(groupJoinType, enumType);
                var lambdaSelectMany = Expression.Lambda(funcTypeSelectMany, manyInit, parSelectMany);

                List<MemberBinding> bindings = new List<MemberBinding>();

                for (int i = 0; i < rightTypes.Length; i++)
                {
                    var alias = getAlias(i);
                    if (rightTypes.Length > 1)
                    {
                        var prop = Expression.Property(parOutput2, rightType.GetProperty(alias));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), parOutput2));
                    }
                }
                for (int i = 0; i < leftTypes.Length; i++)
                {
                    if (leftTypes.Length > 1)
                    {
                        var alias = getAlias(rightTypes.Length + i);
                        var prop = Expression.Property(Expression.Property(parOutput1, "u"), leftType.GetProperty(getAlias(i)));
                        bindings.Add(Expression.Bind(outputType.GetProperty(alias), prop));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(outputType.GetProperty(getAlias(rightTypes.Length)), Expression.Property(parOutput1, "u")));
                    }
                }
                var outputInitExp = Expression.MemberInit(Expression.New(outputType.GetConstructor(Type.EmptyTypes)), bindings);

                var funcTypeResult = typeof(Func<,,>).MakeGenericType(groupJoinType, rightType, outputType);
                var lambdaResult = Expression.Lambda(funcTypeResult, outputInitExp, parOutput1, parOutput2);
                var mtdOutput = mtdQueryableSelectManyByGeneric.MakeGenericMethod(groupJoinType, rightType, outputType);
                result = mtdOutput.Invoke(null, new object[] { qrySelectGroup, lambdaSelectMany, lambdaResult }) as IQueryable<object>;

            }
            #endregion

            return result;
        }

        public static IQueryable FullJoin(this IQueryable qry1, IQueryable qry2, params (string, string)[] joinParams)
        {
            IQueryable result = null;
            IQueryable resultLeft = qry1.LeftJoin(qry2, joinParams);
            IQueryable resultRight = qry1.RightJoin(qry2, resultLeft.ElementType, joinParams);
            var mtd = mtdUnion.MakeGenericMethod(resultLeft.ElementType);
            result = mtd.Invoke(null, new object[] { resultLeft, resultRight }) as IQueryable<object>;
            return result;
        }
        
        
        private static Expression<Func<T, dynamic>> CreateSelectExpression<T>(params string[] fieldNames)
        {
            if (fieldNames == null || fieldNames.Length == 0)
                throw new ArgumentNullException(nameof(fieldNames));
            Type sourceType = typeof(T);
            //Type outputType = null; 
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



        private static string getAlias(int idx)
        {
            return new string((char)(idx + 97), 1);
        }
        private static int getAliasIndex(string alias)
        {
            return (int)alias[0] - 97;
        }
        private static PropertyInfo getPropertyInfo(Type objType, string name)
        {
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
            if (matchedProperty == null)
                throw new ArgumentException("name");

            return matchedProperty;
        }
        private static LambdaExpression getOrderExpression(Type objType, PropertyInfo pi)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }
        public static void ExportToCSV(this IQueryable<object> query, Metadata[] metadataArr, StreamWriter writer)
        {
            bool firstField = true;
            List<Metadata> metaFilter = new List<Metadata>();
            foreach (var item in metadataArr)
            {
                if (item.CustomAttributes != null)
                {
                    var obj = item.CustomAttributes;
                    var propHidden = obj.GetType().GetProperty("Hidden");
                    if (propHidden != null)
                    {
                        var val = propHidden.GetValue(obj);
                        if (val != null && Boolean.TryParse(val.ToString(), out bool hidden))
                        {
                            if (hidden)
                                continue;
                        }
                    }
                }
                metaFilter.Add(item);
            }
            for (int i = 0; i < metaFilter.Count; i++)
            {
                var meta = metaFilter[i];
                if (firstField)
                    firstField = false;
                else
                    writer.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                writer.Write("\"{0}\"", meta.FieldLabel.Replace("\"", "\"\""));
            }
            writer.WriteLine();

            // this method actually execute IDataReader
            // no need to convert.. thx EF 6
            foreach (var item in query)
            {
                firstField = true;
                for (int i = 0; i < metaFilter.Count; i++)
                {
                    var meta = metaFilter[i];
                    var pi = meta.PropertyInfo;
                    var val = pi.GetValue(item, null);
                    if (firstField)
                        firstField = false;
                    else
                        writer.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    if (val == null)
                        writer.Write("\"\"");
                    else
                    {
                        var txt = string.Empty;
                        if (!string.IsNullOrEmpty(meta.DataFormatString))
                            txt = string.Format(meta.DataFormatString, val);
                        else
                            txt = val.ToString();
                        txt = txt.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\"", "\"\"");
                        writer.Write("\"{0}\"", txt);
                    }
                }
                writer.WriteLine();
            }
        }

        public static void ExportToCSV<T>(this IQueryable<T> query, StreamWriter writer)
        {
            var sytemProps = typeof(T).GetProperties().Where(t => t.PropertyType.Namespace == "System").ToArray();
            bool firstField = true;
            
            for (int i = 0; i < sytemProps.Length; i++)
            {
                var pi = sytemProps[i];
                if (firstField)
                    firstField = false;
                else
                    writer.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                writer.Write("\"{0}\"", pi.Name.ToHumanReadable());
            }
            writer.WriteLine();

            // this method actually execute IDataReader
            // no need to convert/use SqlDataReader.. thx EF 6
            foreach (var item in query)
            {
                firstField = false;
                for (int i = 0; i < sytemProps.Length; i++)
                {
                    var pi = sytemProps[i];
                    var val = pi.GetValue(item, null);
                    if (firstField)
                        firstField = false;
                    else
                        writer.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    if (val == null)
                        writer.Write("\"\"");
                    else
                    {
                        var txt = val.ToString();
                        txt = txt.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\"", "\"\"");
                        writer.Write("\"{0}\"", txt);
                    }
                }
                writer.WriteLine();
            }
        }

        public static void ExportToExcel(this IQueryable<dynamic> query, Type valueType, Metadata[] metadataArr, Stream strm)
        {
            LiteExcelWriter xlWriter = new LiteExcelWriter();
            List<Metadata> metaFilter = new List<Metadata>();
            foreach (var item in metadataArr)
            {
                if (item.CustomAttributes != null)
                {
                    var obj = item.CustomAttributes;
                    var propHidden = obj.GetType().GetProperty("Hidden");
                    if (propHidden != null)
                    {
                        var val = propHidden.GetValue(obj);
                        if (val != null && Boolean.TryParse(val.ToString(), out bool hidden))
                        {
                            if (hidden)
                                continue;
                        }
                    }
                }
                metaFilter.Add(item);
            }
            xlWriter.Render(query, valueType, metaFilter.ToArray(), strm);
        }
        public static void ExportToExcel<T>(this IQueryable<T> query, Stream strm)
        {
            LiteExcelWriter xlWriter = new LiteExcelWriter();
            xlWriter.Render(query, strm);
        }
    }
}
