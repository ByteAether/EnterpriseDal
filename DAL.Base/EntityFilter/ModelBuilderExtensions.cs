using System.Reflection;
using DAL.Base.EntityBehavior;
using LinqToDB;
using LinqToDB.Mapping;

namespace DAL.Base.EntityFilter;

public static class ModelBuilderExtensions
{
	private static readonly MethodInfo _processMethodInfo
		= typeof(ModelBuilderExtensions).GetMethod(
			nameof(ProcessEntity),
			BindingFlags.Static | BindingFlags.NonPublic
		)!;

	public static void ApplyEntityFilters<TContext>(this MappingSchema mappings)
		where TContext : IDataContext
	{
		var builder = new FluentMappingBuilder(mappings);
		var baseEntityType = typeof(IEntity);

		var entities = AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(x => x.GetTypes())
			.Where(x =>
				x is { IsAbstract: false, IsClass: true, IsPublic: true }
				&& baseEntityType.IsAssignableFrom(x)
			);

		foreach (var e in entities)
		{
			_processMethodInfo
				.MakeGenericMethod(e, typeof(TContext))
				.Invoke(null, [builder]);
		}

		builder.Build();
	}

	private static void ProcessEntity<TEntity, TContext>(FluentMappingBuilder builder)
		where TContext : IDataContext
	{
		var filter = EntityFilterHelper.GetEntityFilter<TEntity, TContext>();
		if (filter != null)
		{
			builder.Entity<TEntity>().HasQueryFilter(filter);
		}
	}
}
