using ByteAether.Ulid;
using DAL.Base.EntityBehavior;
using LinqToDB;
using LinqToDB.Concurrency;
using LinqToDB.Data;
using LinqToDB.Internal.Linq;
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

						if (e is IUserCreatable userCreatable && userCreatable.CreatedByUserId == default)
						{
							userCreatable.CreatedByUserId = ctx.Attributes.UserId ?? Ulid.Empty;
						}

						if (e is IModifiable updateable && updateable.ModifiedAt == default)
						{
							updateable.ModifiedAt = DateTime.UtcNow;
						}

						if (e is IUserModifiable userUpdateable && userUpdateable.ModifiedByUserId == default)
						{
							userUpdateable.ModifiedByUserId = ctx.Attributes.UserId ?? Ulid.Empty;
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

		if (entity is IUserModifiable userUpdateable)
		{
			userUpdateable.ModifiedByUserId = ctx.Attributes.UserId ?? Ulid.Empty;
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

		if (typeof(IUserModifiable).IsAssignableFrom(typeof(T)))
		{
			var dbCtx = Internals.GetDataContext(source) as IDbCtx;
			source = source.Set(
				x => Sql.Property<Ulid>(x, nameof(IUserModifiable.ModifiedByUserId)),
				dbCtx?.Attributes.UserId ?? Ulid.Empty
			);
		}

		return source.UpdateAsync(cancellationToken);
	}

	public static Task<int> ModifyAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
		where T : class
		=> source.AsUpdatable().ModifyAsync(cancellationToken);

	public static Task<int> RemoveAsync<T>(this IDbCtx ctx, T entity, CancellationToken cancellationToken = default)
		where T : class, IEntity
	{
		if (entity is IRemovable removable)
		{
			removable.RemovedAt = DateTime.UtcNow;
			return ctx.ModifyAsync(entity, cancellationToken);
		}

		return ctx.GetTable<T>().DeleteOptimisticAsync(entity, cancellationToken);
	}

	public static Task<int> RemoveAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
		where T : class
	{
		if (typeof(IRemovable).IsAssignableFrom(typeof(T)))
		{
			var delSource = source.Set(
				x => Sql.Property<DateTime?>(x, nameof(IRemovable.RemovedAt)),
				DateTime.UtcNow
			);
			return delSource.ModifyAsync(cancellationToken);
		}

		return source.DeleteAsync(cancellationToken);
	}
}
