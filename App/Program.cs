using DAL.Context;
using DAL.Context.Entity;
using LinqToDB;

Console.WriteLine("Hello, World!");

var ctx = new DbCtx(
	new(
		new(
			new(
				ProviderName: "SQLite",
				ConnectionString: "Data Source=database.db"
			)
		)
	)
);

var q = ctx.GetTable<User>();

Console.WriteLine($"Query: {q.ToSqlQuery().Sql}");

Console.WriteLine("Done.");
