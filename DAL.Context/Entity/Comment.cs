using System.Linq.Expressions;
using ByteAether.Ulid;
using DAL.Base.EntityBehavior;
using LinqToDB;

namespace DAL.Context.Entity;

public partial class Comment : IProtected
{
	[ExpressionMethod(nameof(GetPermissionObjectIdExpression))]
	public Ulid GetPermissionObjectId() => Post.Id;

	private static Expression<Func<Comment, Ulid>> GetPermissionObjectIdExpression()
		=> x => x.Post.Id;
}
