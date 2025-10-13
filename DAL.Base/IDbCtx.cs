using ByteAether.Ulid;
using DAL.Base.EntityBehavior;
using LinqToDB;

namespace DAL.Base;

public interface IDbCtx : IDataContext
{
	DbCtxAttributes Attributes { get; set; }

	ITable<IPermissionEntity> GetPermissions();

	public record DbCtxAttributes(
		Ulid TenantId = default,
		Ulid? UserId = null
	);
}
