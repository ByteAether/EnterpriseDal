namespace DAL.Base.EntityBehavior;

public interface ICreatable : IEntity
{
	DateTime CreatedAt { get; set; }
}
