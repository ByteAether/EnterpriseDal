using ByteAether.Ulid;
using DAL.Base;
using DAL.Context;
using DAL.Context.Entity;
using LinqToDB;
using LinqToDB.Async;

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

await ctx.BeginTransactionAsync();

// Create tenant
var tenant = new Tenant
{
	Id = Ulid.New(),
	Name = "TestTenant"
};
await ctx.CreateAsync(tenant);

// Create entity
var u = new User
{
	Id = Ulid.New(),
	TenantId = tenant.Id,
	Username = "asd123"
};
await ctx.CreateAsync(u);

Console.WriteLine(ctx.LastQuery);

// Set DB Context parameters
ctx.Attributes = new(
	tenant.Id
);

// Modify entity
await ctx.GetTable<User>()
	.Where(x => x.Id == u.Id)
	.Set(x => x.Username, "asd1234")
	.ModifyAsync();

Console.WriteLine(ctx.LastQuery);

// Remove entity
await ctx.GetTable<User>()
	.Where(x => x.Id == u.Id)
	.RemoveAsync();

Console.WriteLine(ctx.LastQuery);

// Get posts that are tenanted through a user
await ctx.GetTable<Post>()
	.ToListAsync();

Console.WriteLine(ctx.LastQuery);

await ctx.RollbackTransactionAsync();

Console.WriteLine("Done.");
