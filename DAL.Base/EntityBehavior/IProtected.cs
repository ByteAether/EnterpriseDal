using ByteAether.Ulid;
using DAL.Base.EntityFilter;
using LinqToDB;

namespace DAL.Base.EntityBehavior;

[EntityFilter<IProtected>(nameof(Filter))]
public interface IProtected
{
	Ulid GetPermissionObjectId();

	private static IQueryable<T> Filter<T>(IQueryable<T> q, IDbCtx dbCtx)
		where T : IProtected
		=> dbCtx.Attributes.UserId == null
			? q.Where(_ => false)
			: q.InnerJoin(
				dbCtx.GetPermissions(),
				(a, b) => b.ObjectId == a.GetPermissionObjectId() && b.SubjectId == dbCtx.Attributes.UserId,
				(a, b) => a
			);
}
