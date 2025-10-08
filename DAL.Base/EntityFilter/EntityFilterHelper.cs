using System.Linq.Expressions;
using System.Reflection;

namespace DAL.Base.EntityFilter;

internal static class EntityFilterHelper
{
	private const BindingFlags BindingFlags
		= System.Reflection.BindingFlags.Static
		| System.Reflection.BindingFlags.Public
		| System.Reflection.BindingFlags.NonPublic;

	public static Func<IQueryable<TEntity>, TContext, IQueryable<TEntity>>? GetEntityFilter<TEntity, TContext>()
	{
		var filters = FindAllInheritances(typeof(TEntity))
			.SelectMany(x => x
				.GetCustomAttributes<EntityFilterAttribute>()
				.Select(y => (InheritType: x, Attribute: y))
			)
			.Select(x => GetLambda<TEntity, TContext>(x.InheritType, x.Attribute))
			.ToArray();

		return filters.Any()
			? (q, dbCtx) => filters.Aggregate(q, (currQ, nextFunc) => nextFunc(currQ, dbCtx))
			: null;
	}

	private static HashSet<Type> FindAllInheritances(Type entityType)
	{
		var relevantTypes = new HashSet<Type>();
		var processQueue = new Queue<Type>();
		processQueue.Enqueue(entityType);

		while (processQueue.TryDequeue(out var processType))
		{
			if (processType.BaseType != null)
			{
				processQueue.Enqueue(processType.BaseType);
			}

			foreach (var iFace in processType.GetInterfaces())
			{
				processQueue.Enqueue(iFace);
			}

			relevantTypes.Add(processType);
		}

		return relevantTypes;
	}

	private static Func<IQueryable<TEntity>, TContext, IQueryable<TEntity>> GetLambda<TEntity, TContext>(
		Type inheritType,
		EntityFilterAttribute entityFilter
	)
	{
		var providerType = entityFilter.ProviderType;
		if (providerType.IsGenericType)
		{
			providerType = providerType.MakeGenericType(inheritType.GetGenericArguments());
		}

		var methodInfo = providerType
				.GetMethods(BindingFlags)
				.FirstOrDefault(m => m.Name == entityFilter.PropertyName && m.IsGenericMethodDefinition)
				?.MakeGenericMethod(typeof(TEntity))
			?? throw new ArgumentException(
				$"Method '{entityFilter.PropertyName}' not found in type '{providerType.FullName}' or is not a generic method definition."
			);

		var qParam = Expression.Parameter(typeof(IQueryable<TEntity>), "q");
		var dbCtxParam = Expression.Parameter(typeof(TContext), "dbCtx");

		var methodCall = Expression.Call(
			null, // Static method, no instance
			methodInfo,
			qParam,
			dbCtxParam
		);

		var lambda =
			Expression.Lambda<Func<IQueryable<TEntity>, TContext, IQueryable<TEntity>>>(
				methodCall,
				qParam,
				dbCtxParam
			);

		return lambda.Compile();
	}
}
