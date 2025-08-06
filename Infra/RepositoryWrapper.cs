using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Seed_Admin.Infra;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Seed_Admin
{
	public interface IRepositoryBase<T> where T : class

	{
		IEnumerable<T> GetAll();
		IEnumerable<T> GetByCondition(Expression<Func<T, bool>> expression);
		PagedResult Get(int start = 0, int length = 10, string? sortColumn = "", string? sortColumnDir = "asc", string? searchValue = "");
		bool Any(Expression<Func<T, bool>> expression);
		T Add(T entity);
		void Add(List<T> entity);
		void Update(T entity);
		void Delete(T entity);
	}


	//public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
	public class RepositoryWrapper<T>(DataContext context) : IRepositoryBase<T> where T : class
	{
		public IEnumerable<T> GetAll() => context.Set<T>().AsNoTracking().ToList();
		public IEnumerable<T> GetByCondition(Expression<Func<T, bool>> expression) => context.Set<T>().Where(expression).AsNoTracking().ToList();

		public PagedResult Get(int start = 0, int length = 10, string? sortColumn = "", string? sortColumnDir = "asc", string? searchValue = "")
		{
			var users = context.Set<T>().AsNoTracking().AsQueryable();

			var recordsTotal = users.Count();

			if (!string.IsNullOrEmpty(searchValue))
				users = users.SearchAnyProperty(searchValue);

			if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
				users = sortColumnDir.ToLower() == "asc"
					? users.OrderByDynamic(sortColumn)
					: users.OrderByDynamic(sortColumn, true);

			var pagedData = users.Skip(start).Take(length).ToList<T>();

			return new PagedResult
			{
				StartIndex = start,
				Length = length,
				RecordsFiltered = pagedData.Count,
				RecordsTotal = recordsTotal,
				Data = pagedData
			};
		}

		public bool Any(Expression<Func<T, bool>> expression) => context.Set<T>().Any(expression);
		public T Add(T entity) { context.Set<T>().Add(entity); context.Set<T>().Entry(entity).Reload(); context.SaveChanges(); context.Set<T>().Entry(entity).State = EntityState.Detached; return entity; }
		public void Add(List<T> entity) { context.Set<T>().AddRange(entity); context.SaveChanges(); }
		public void Update(T entity) { context.Set<T>().Entry(entity).State = EntityState.Modified; context.SaveChanges(); context.Set<T>().Entry(entity).State = EntityState.Detached; }
		public void Delete(T entity) { context.Set<T>().Entry(entity).State = EntityState.Deleted; context.SaveChanges(); context.Set<T>().Entry(entity).State = EntityState.Detached; }
	}


	public interface IRepositoryWrapper
	{
		IRepositoryBase<T> Using<T>() where T : class;
		IDbContextTransaction BeginTransaction();
	}

    public class RepositoryWrapper(IDbContextFactory<DataContext> factory) : IRepositoryWrapper
    {
        public IRepositoryBase<T> Using<T>() where T : class
            => new RepositoryWrapper<T>(factory.CreateDbContext());

        public IDbContextTransaction BeginTransaction()
        {
            var context = factory.CreateDbContext();
            return context.Database.BeginTransaction();
        }
    }


    public static class IQueryableExtensions
	{
		public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool descending = false)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				return source;

			var parameter = Expression.Parameter(typeof(T), "x");
			var property = Expression.Property(parameter, propertyName);
			var lambda = Expression.Lambda(property, parameter);
			string methodName = descending ? "OrderByDescending" : "OrderBy";

			var result = Expression.Call(typeof(Queryable), methodName,
				new Type[] { typeof(T), property.Type },
				source.Expression, Expression.Quote(lambda));

			return source.Provider.CreateQuery<T>(result);
		}
	}

	public static class QueryableSearchExtensions
	{
		public static IQueryable<T> SearchAnyProperty<T>(this IQueryable<T> query, string searchValue)
		{
			if (string.IsNullOrWhiteSpace(searchValue))
				return query;

			var parameter = Expression.Parameter(typeof(T), "x");
			Expression? predicate = null;

			// Get all string properties
			var stringProperties = typeof(T).GetProperties()
				.Where(p => p.PropertyType == typeof(string) && p.GetCustomAttribute<NotMappedAttribute>() == null);

			foreach (var prop in stringProperties)
			{
				var property = Expression.Property(parameter, prop);
				var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

				var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
				var search = Expression.Constant(searchValue);
				var contains = Expression.Call(property, containsMethod, search);

				var notNullAndContains = Expression.AndAlso(notNull, contains);
				predicate = predicate == null ? notNullAndContains : Expression.OrElse(predicate, notNullAndContains);
			}

			if (predicate == null) return query;

			var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
			return query.Where(lambda);
		}

	}

}
