using System.Linq.Expressions;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataTables.AspNet.AspNetCore;
using static FastExpressionCompiler.ExpressionCompiler;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Storage;

namespace URegister.Infrastructure.Extensions
{
    /// <summary>
    /// Разширения на методите на LINQ
    /// </summary>
    public static class DataTablesExtension
    {

        /// <summary>
        /// Сортиране в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на данните</typeparam>
        /// <param name="source">Дърво, което трябва да бъде подредено</param>
        /// <param name="sortModels">Модел с данни за начина на сортиране</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, ICollection<Common.DatatableSortColumn> sortModels)
        {
            var expression = source.Expression;
            int count = 0;

            foreach (var item in sortModels)
            {
                var parameter = Expression.Parameter(source.ElementType, "x");
                var selector = Expression.PropertyOrField(parameter, item.Name);
                var method = item.Direction == (int)SortDirection.Descending ?
                    (count == 0 ? "OrderByDescending" : "ThenByDescending") :
                    (count == 0 ? "OrderBy" : "ThenBy");
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }

            return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
        }


        /// <summary>
        /// Търсене в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="source">Пълен сет данни</param>
        /// <param name="searchModel">Модел с колони, по които се търси</param>
        /// <param name="query">Стойност на полето за търсене</param>
        /// <returns></returns>
        public static IQueryable<T> SearchFor<T>(this IQueryable<T> source, ICollection<string> searchModel, string query)
        {
            if (searchModel?.Count() == 0 || String.IsNullOrEmpty(query))
            {
                return source;
            }

            var parameter = Expression.Parameter(source.ElementType, "x");
            Expression predicate = Expression.Constant(false, typeof(Boolean));
            Expression<Func<string>> lowerQuery = () => $"%{query}%";

            foreach (var item in searchModel)
            {
                var selector = Expression.PropertyOrField(parameter, item);

                if (selector.Type != typeof(string))
                {
                    continue;
                }


                Expression filter = null;

                if (Nullable.GetUnderlyingType(selector.Type) != null || selector.Type == typeof(string))
                {
                    filter = Expression.Condition(
                    Expression.NotEqual(selector, Expression.Constant(null, selector.Type)),
                    Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes)),
                    Expression.Constant(String.Empty)
                    );
                }
                else
                {
                    filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                }

                //Expression filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                filter = Expression.Call(filter, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));

                // var property = Expression.Property(param, "Name");
                filter = Expression.Call(
                          typeof(NpgsqlDbFunctionsExtensions),
                          nameof(NpgsqlDbFunctionsExtensions.ILike),
                          Type.EmptyTypes,
                          Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                          selector,
                          lowerQuery.Body);

                //filter = Expression.Call(filter, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), Expression.Constant(lowerQuery));
                predicate = Expression.OrElse(predicate, filter);
            }

            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { source.ElementType },
                source.Expression,
                Expression.Lambda<Func<T, bool>>(predicate, new ParameterExpression[] { parameter }));

            return source.Provider.CreateQuery<T>(whereCallExpression);
        }

        /// <summary>
        /// Генерира отговор на AJAX заявка на DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Сет от данни за страницата</param>
        /// <param name="dataCount">Общ брой записи</param>
        /// <returns></returns>
        public static IActionResult GetResponseServerPaging<T>(this IDataTablesRequest request, ICollection<T> data, int dataCount) where T : class
        {
            var dtResponse = DataTablesResponse.Create(request, dataCount, dataCount, data);

            var settings = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNamingPolicy = new DataTablesResponseDataNamingPolicy()
            };

            return new JsonResult(dtResponse, settings);
        }

        public static Common.DatatableRequest GetDataTablesRequestProto(this IDataTablesRequest request)
        {
            var result = new Common.DatatableRequest
            {
                Length = request.Length,
                Start = request.Start,
                Filter = request.Search.Value
            };
            result.OrderBy.AddRange(
                request.Columns
                       .Where(x => x.Sort != null)
                       .Select(x => new Common.DatatableSortColumn
                       {
                           Direction = (int)x.Sort.Direction,
                           Name = x.Name
                       })
                       .ToList());
            result.SearchColumn.AddRange(
                request.Columns
                       .Where(x => x.IsSearchable)
                       .Select(x => x.Name)
                       .ToList());
            return result;
        }

        public static async Task<(IQueryable<T>, int)> GetFilteredData<T>(this Common.DatatableRequest request, IQueryable<T> source)
        {
            var query = source.SearchFor(request.SearchColumn, request.Filter);
            var countAll = await query.CountAsync();

            if (request.Length < 0)
            {
                query = query.OrderBy(request.OrderBy);
            }
            else
            {
                query = query.OrderBy(request.OrderBy).Skip(request.Start).Take(request.Length);
            }
            return (query, countAll);
        }

        /// <summary>
        /// Генерира отговор на AJAX заявка на DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <param name="filteredData">Филтриран сет от данни</param>
        /// <returns></returns>
        public static IActionResult GetResponse<T>(this IDataTablesRequest request, IQueryable<T> data, IQueryable<T> filteredData = null, Dictionary<string, object> additionalParameters = null, bool fromDatabase = true) where T : class
        {
            (var dataPage, var dataCount) =  request.GetResponseData(data, filteredData, additionalParameters, fromDatabase);
            return request.GetResponseJson<T>(dataPage, dataCount, additionalParameters);
        }
        public static IActionResult GetResponseJson<T>(this IDataTablesRequest request, IQueryable<T> data, int dataCount,  Dictionary<string, object> additionalParameters = null) where T : class
        {
            int filteredCount = dataCount;
            if (request.Search.Value != null)
            {
                filteredCount = calcCountInDataset<T>(data);
            }

            var dtResponse = DataTablesResponse.Create(request, dataCount, filteredCount, data, additionalParameters);

            var settings = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNamingPolicy = new DataTablesResponseDataNamingPolicy()
            };

            return new JsonResult(dtResponse, settings);
        }

        public static (IQueryable<T>, int) GetResponseData<T>(this IDataTablesRequest request, IQueryable<T> data, IQueryable<T> filteredData = null, Dictionary<string, object> additionalParameters = null, bool fromDatabase = true) where T : class
        {
            if (filteredData == null)
            {
                filteredData = request.GetFilteredData(data, fromDatabase);
                if (!fromDatabase)
                {
                    filteredData = filteredData.ToList().AsQueryable();
                }
            }

            var orderColums = request.Columns.Where(x => x.Sort != null);
            IQueryable<T> dataPage = null;

            if (request.Length < 0)
            {
                dataPage = filteredData.OrderBy(orderColums);
            }
            else
            {
                dataPage = filteredData.OrderBy(orderColums).Skip(request.Start).Take(request.Length);
            }

            int dataCount = calcCountInDataset<T>(data);
            return (dataPage, dataCount);
        }
        /// <summary>
        /// Използва текста в полето за търсене за филтрация на данните по колоните, 
        /// маркирани като колони за търсене
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <returns></returns>
        public static IQueryable<T> GetFilteredData<T>(this IDataTablesRequest request, IQueryable<T> data, bool fromDatabase = true)
        {
            var filteredData = data;

            if (request.Search.Value != null)
            {
                var searchColumns = request.Columns.Where(c => c.IsSearchable);
                if (fromDatabase)
                {
                    filteredData = data.SearchForNet8(searchColumns, request.Search.Value);
                }
                else
                {
                    filteredData = data.SearchFor(searchColumns, request.Search.Value);
                }
            }

            return filteredData;
        }

        /// <summary>
        /// Търсене в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="source">Пълен сет данни</param>
        /// <param name="searchModel">Модел с колони, по които се търси</param>
        /// <param name="query">Стойност на полето за търсене</param>
        /// <returns></returns>
        public static IQueryable<T> SearchForNet8<T>(this IQueryable<T> source, IEnumerable<DataTables.AspNet.Core.IColumn> searchModel, string query)
        {
            if (searchModel?.Count() == 0 || String.IsNullOrEmpty(query))
            {
                return source;
            }

            var parameter = Expression.Parameter(source.ElementType, "x");
            Expression predicate = Expression.Constant(false, typeof(Boolean));
            Expression<Func<string>> lowerQuery = () => $"%{query}%";

            foreach (var item in searchModel.Where(c => c.IsSearchable))
            {
                var selector = Expression.PropertyOrField(parameter, item.Name);

                if (selector.Type != typeof(string))
                {
                    continue;
                }


                Expression filter = null;

                if (Nullable.GetUnderlyingType(selector.Type) != null || selector.Type == typeof(string))
                {
                    filter = Expression.Condition(
                    Expression.NotEqual(selector, Expression.Constant(null, selector.Type)),
                    Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes)),
                    Expression.Constant(String.Empty)
                    );
                }
                else
                {
                    filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                }

                //Expression filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                filter = Expression.Call(filter, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));

                // var property = Expression.Property(param, "Name");
                filter = Expression.Call(
                          typeof(NpgsqlDbFunctionsExtensions),
                          nameof(NpgsqlDbFunctionsExtensions.ILike),
                          Type.EmptyTypes,
                          Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                          selector,
                          lowerQuery.Body);

                //filter = Expression.Call(filter, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), Expression.Constant(lowerQuery));
                predicate = Expression.OrElse(predicate, filter);
            }

            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { source.ElementType },
                source.Expression,
                Expression.Lambda<Func<T, bool>>(predicate, new ParameterExpression[] { parameter }));

            return source.Provider.CreateQuery<T>(whereCallExpression);
        }
        /// <summary>
        /// Търсене в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="source">Пълен сет данни</param>
        /// <param name="searchModel">Модел с колони, по които се търси</param>
        /// <param name="query">Стойност на полето за търсене</param>
        /// <returns></returns>
        public static IQueryable<T> SearchFor<T>(this IQueryable<T> source, IEnumerable<DataTables.AspNet.Core.IColumn> searchModel, string query)
        {
            if (searchModel?.Count() == 0 || String.IsNullOrEmpty(query))
            {
                return source;
            }

            var parameter = Expression.Parameter(source.ElementType, "x");
            Expression predicate = Expression.Constant(false, typeof(Boolean));
            Expression<Func<string>> lowerQuery = () => query.ToLower();

            foreach (var item in searchModel.Where(c => c.IsSearchable))
            {
                var selector = Expression.PropertyOrField(parameter, item.Name);
                Expression filter = null;

                if (Nullable.GetUnderlyingType(selector.Type) != null || selector.Type == typeof(string))
                {
                    filter = Expression.Condition(
                    Expression.NotEqual(selector, Expression.Constant(null, selector.Type)),
                    Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes)),
                    Expression.Constant(String.Empty)
                    );
                }
                else
                {
                    filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                }

                //Expression filter = Expression.Call(selector, selector.Type.GetMethod("ToString", Type.EmptyTypes));
                filter = Expression.Call(filter, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                filter = Expression.Call(filter, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), lowerQuery.Body);
                predicate = Expression.OrElse(predicate, filter);
            }

            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { source.ElementType },
                source.Expression,
                Expression.Lambda<Func<T, bool>>(predicate, new ParameterExpression[] { parameter }));

            return source.Provider.CreateQuery<T>(whereCallExpression);
        }

        private static int calcCountInDataset<T>(IQueryable<T> data) where T : class
        {
            try
            {
                //Съкращава резултата до основната таблица и пряко свързаните подчинени
                return data.Select(x => new { id = 1 }).Count();
            }
            catch
            {
                return data.Count();
            }
        }

        /// <summary>
        /// Сортиране в DataTables
        /// </summary>
        /// <typeparam name="T">Тип на данните</typeparam>
        /// <param name="source">Дърво, което трябва да бъде подредено</param>
        /// <param name="sortModels">Модел с данни за начина на сортиране</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<DataTables.AspNet.Core.IColumn> sortModels)
        {
            var expression = source.Expression;
            int count = 0;

            foreach (var item in sortModels)
            {
                var parameter = Expression.Parameter(source.ElementType, "x");
                var selector = Expression.PropertyOrField(parameter, item.Name);
                var method = item.Sort.Direction == DataTables.AspNet.Core.SortDirection.Descending ?
                    (count == 0 ? "OrderByDescending" : "ThenByDescending") :
                    (count == 0 ? "OrderBy" : "ThenBy");
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }

            return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
        }
    }
}
