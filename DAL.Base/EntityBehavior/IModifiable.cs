namespace DAL.Base.EntityBehavior;

public interface IModifiable : IEntity
{
	DateTime ModifiedAt { get; set; }
}
