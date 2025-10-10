using System.Linq.Expressions;
using ByteAether.Ulid;
using DAL.Base.EntityBehavior;
using LinqToDB;

namespace DAL.Context.Entity;

public partial class Post : ITenanted
{
	[ExpressionMethod(nameof(GetTenantIdExpression))]
	public Ulid TenantId => GetTenantIdExpression().Compile()(this);

	private static Expression<Func<Post, Ulid>> GetTenantIdExpression()
		=> x => x.User.TenantId;
}
