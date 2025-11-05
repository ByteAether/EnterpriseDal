using ByteAether.Ulid;

namespace DAL.Base.EntityBehavior;

public interface IUserModifiable
{
	Ulid ModifiedByUserId { get; set; }
}
