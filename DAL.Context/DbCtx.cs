using ByteAether.Ulid;
using DAL.Base;
using LinqToDB.Data;

namespace DAL.Context;

public partial class DbCtx : IDbCtx
{
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
	}
}
