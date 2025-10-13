using ByteAether.Ulid;
using DAL.Base;
using DAL.Base.EntityBehavior;
using DAL.Base.EntityFilter;
using DAL.Context.Entity;
using LinqToDB;
using LinqToDB.Data;

namespace DAL.Context;

public partial class DbCtx : IDbCtx
{
	public IDbCtx.DbCtxAttributes Attributes { get; set; } = new();

	public ITable<IPermissionEntity> GetPermissions() => this.GetTable<Permission>();

	partial void InitDataContext()
	{
		InlineParameters = true;

		MappingSchema.SetConvertExpression<Ulid, DataParameter>(x => DataParameter.Binary(null, x.ToByteArray()));
		MappingSchema.SetConvertExpression<Ulid?, DataParameter>(x => DataParameter.Binary(
				null,
				x.HasValue ? x.Value.ToByteArray() : null
			)
		);
		MappingSchema.SetConvertExpression<byte[], Ulid>(x => Ulid.New(x));
		MappingSchema.SetConvertExpression<byte[]?, Ulid?>(x => x != null ? Ulid.New(x) : null);

		MappingSchema.ApplyEntityFilters<DbCtx>();
	}
}
