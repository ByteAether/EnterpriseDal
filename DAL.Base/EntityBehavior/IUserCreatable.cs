using ByteAether.Ulid;

namespace DAL.Base.EntityBehavior;

public interface IUserCreatable
{
	Ulid CreatedByUserId { get; set; }
}
