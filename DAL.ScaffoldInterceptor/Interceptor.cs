using System.Text.RegularExpressions;
using ByteAether.Ulid;
using DAL.Base.EntityBehavior;
using LinqToDB;
using LinqToDB.CodeModel;
using LinqToDB.DataModel;
using LinqToDB.Scaffold;
using LinqToDB.Schema;

namespace DAL.ScaffoldInterceptor;

public sealed class Interceptor : ScaffoldInterceptors
{
	public override TypeMapping? GetTypeMapping(
		DatabaseType databaseType,
		ITypeParser typeParser,
		TypeMapping? defaultMapping
	)
	{
		return databaseType.Name?.ToLower() switch
		{
			"ulid" => new(typeParser.Parse<Ulid>(), DataType.Binary),
			_ => base.GetTypeMapping(databaseType, typeParser, defaultMapping)
		};
	}

	public override void PreprocessAssociation(ITypeParser typeParser, AssociationModel associationModel)
	{
		base.PreprocessAssociation(typeParser, associationModel);

		// Ignore composite keys
		if (associationModel.FromColumns!.Length > 1)
		{
			return;
		}

		var fromCol = associationModel.FromColumns!.First();
		var toCol = associationModel.ToColumns!.First();

		// Replace [table]_[id] => [Entity]
		var newAssocName = fromCol.Metadata.Name!.Replace(
				associationModel.Target.Metadata.Name + "_" + toCol.Metadata.Name,
				associationModel.Target.Class.Name
			)
			.Replace("_", " ")
			.UCFirst()
			.Replace(" ", "");

		associationModel.Property!.Name = newAssocName;

		// Backreference: [Entity]s
		var newBackName = fromCol.Metadata.Name!.Replace(
					associationModel.Target.Metadata.Name + "_" + toCol.Metadata.Name,
					associationModel.Source.Class.Name
				)
				.Replace("_", " ")
				.UCFirst()
				.Replace(" ", "")
			+ "s";

		associationModel.BackreferenceProperty!.Name = newBackName;
	}

	public override void PreprocessEntity(ITypeParser typeParser, EntityModel entityModel)
	{
		base.PreprocessEntity(typeParser, entityModel);

		entityModel.ContextProperty = null;
		entityModel.Class.Namespace += ".Entity";

		// Base entity interface
		var addedInterfaces = new List<Type>
		{
			typeof(IEntity)
		};

		// IIdentifiable<>
		var idField = entityModel.Columns.FirstOrDefault(x =>
			x.Property.Name == nameof(IIdentifiable<int>.Id)
			&& x.Metadata.IsPrimaryKey
		);
		if (idField is not null)
		{
			var idType = GetColumnType(idField);
			if (idType is not null)
			{
				var identifiable = typeof(IIdentifiable<>).MakeGenericType(idType);
				addedInterfaces.Add(identifiable);
			}
		}

		// Add all found interfaces
		entityModel.Class.Interfaces ??= [];
		entityModel.Class.Interfaces.AddRange(addedInterfaces.Select(typeParser.Parse));
	}

	//
	private static Type? GetColumnType(ColumnModel column)
	{
		var fullName = string.Join(
			'.',
			column.Property.Type?.Namespace?.Select(x => x.Name).ToArray() ?? []
		);
		fullName += "." + column.Property.Type?.Name?.Name;

		var type = Type.GetType(fullName);
		if (type != null)
		{
			return type;
		}

		foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
		{
			type = a.GetType(fullName);
			if (type != null)
			{
				return type;
			}
		}

		return null;
	}
}

internal static partial class StringExtensions
{
	public static string UCFirst(this string s)
		=> UppercaseWordFirstLetter().Replace(s, match => match.Value.ToUpper());

	[GeneratedRegex("((^\\w)|(\\s|\\p{P})\\w)")]
	private static partial Regex UppercaseWordFirstLetter();
}
