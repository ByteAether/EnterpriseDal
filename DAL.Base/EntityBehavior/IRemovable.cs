using DAL.Base.EntityFilter;

namespace DAL.Base.EntityBehavior;

[EntityFilter<IRemovable>(nameof(Filter))]
public interface IRemovable : IEntity
{
	DateTime? RemovedAt { get; set; }

	private static IQueryable<T> Filter<T>(IQueryable<T> q, IDbCtx _) where T : IRemovable
		=> q.Where(x => x.RemovedAt == null);
}
