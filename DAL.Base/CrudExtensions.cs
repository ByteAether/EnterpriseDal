using DAL.Base.EntityBehavior;
using LinqToDB;
using LinqToDB.Concurrency;
using LinqToDB.Data;
using LinqToDB.Linq;

namespace DAL.Base;

public static class CrudExtensions
{
	public static async Task<long> CreateAsync<T>(
		this IDbCtx ctx,
		IEnumerable<T> entities,
		CancellationToken cancellationToken = default
	)
		where T : class, IEntity
		=> (
			await ctx.BulkCopyAsync(
				entities.Select(e =>
					{
						if (e is ICreatable creatable && creatable.CreatedAt == default)
						{
							creatable.CreatedAt = DateTime.UtcNow;
						}

						if (e is IModifiable updateable && updateable.ModifiedAt == default)
						{
							updateable.ModifiedAt = DateTime.UtcNow;
						}

						return e;
					}
				),
				cancellationToken
			)
		).RowsCopied;

	public static Task<long> CreateAsync<T>(this IDbCtx ctx, T entity, CancellationToken cancellationToken = default)
		where T : class, IEntity
		=> ctx.CreateAsync([entity], cancellationToken);

	public static Task<int> ModifyAsync<T>(this IDbCtx ctx, T entity, CancellationToken cancellationToken = default)
		where T : class, IEntity
	{
		if (entity is IModifiable updateable)
		{
			updateable.ModifiedAt = DateTime.UtcNow;
		}

		return ctx.GetTable<T>().UpdateOptimisticAsync(entity, cancellationToken);
	}

	public static Task<int> ModifyAsync<T>(this IUpdatable<T> source, CancellationToken cancellationToken = default)
		where T : class
	{
		if (typeof(IModifiable).IsAssignableFrom(typeof(T)))
		{
			source = source.Set(
				x => Sql.Property<DateTime>(x, nameof(IModifiable.ModifiedAt)),
				DateTime.UtcNow
			);
		}

		return source.UpdateAsync(cancellationToken);
	}

	public static Task<int> ModifyAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
		where T : class
		=> source.AsUpdatable().ModifyAsync(cancellationToken);
}
