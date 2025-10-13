using ByteAether.Ulid;

namespace DAL.Base.EntityBehavior;

public interface IPermissionEntity
{
	Ulid SubjectId { get; }
	Ulid ObjectId { get; }
}
