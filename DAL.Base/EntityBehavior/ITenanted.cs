using ByteAether.Ulid;
using DAL.Base.EntityFilter;

namespace DAL.Base.EntityBehavior;

[EntityFilter<ITenanted>(nameof(Filter))]
public interface ITenanted : IEntity
{
	Ulid TenantId { get; }

	private static IQueryable<T> Filter<T>(IQueryable<T> q, IDbCtx ctx) where T : ITenanted
		=> q.Where(x => x.TenantId == ctx.Attributes.TenantId);
}
