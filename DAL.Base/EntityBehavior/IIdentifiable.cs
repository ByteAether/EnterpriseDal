namespace DAL.Base.EntityBehavior;

public interface IIdentifiable<T> : IEntity
{
	T Id { get; set; }
}
