namespace DAL.Base.EntityFilter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class EntityFilterAttribute(Type providerType, string propertyName) : Attribute
{
	public Type ProviderType { get; } = providerType;

	public string PropertyName { get; } = propertyName;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class EntityFilterAttribute<T>(string propertyName) : EntityFilterAttribute(typeof(T), propertyName)
{ }
