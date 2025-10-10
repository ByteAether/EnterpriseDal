using ByteAether.Ulid;
using LinqToDB;

namespace DAL.Base;

public interface IDbCtx : IDataContext
{
	DbCtxAttributes Attributes { get; set; }

	public record DbCtxAttributes(Ulid TenantId = default);
}
