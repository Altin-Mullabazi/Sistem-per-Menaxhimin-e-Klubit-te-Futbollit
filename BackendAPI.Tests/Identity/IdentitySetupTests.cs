using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
namespace BackendAPI.Tests.Identity
{
    public class IdentitySetupTests
    {
        private static ServiceProvider BuildProvider(string databaseName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            services.AddLogging();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services.BuildServiceProvider();
        }

        [Fact]
        public void ApplicationUser_HasRequiredCustomProperties()
        {
            var type = typeof(ApplicationUser);

            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.CreatedAt)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.UpdatedAt)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.IsActive)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.FullName)));
        }

        [Fact]
        public void ApplicationUser_HasNavigationProperties()
        {
            var type = typeof(ApplicationUser);

            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.Clubs)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.Players)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.Contracts)));
            Assert.NotNull(type.GetProperty(nameof(ApplicationUser.Matches)));
        }

        [Fact]
        public async Task IdentityRoles_AreSeededInModel()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var roleNames = await context.Roles
                .Select(role => role.Name)
                .ToArrayAsync();

            Assert.Contains("Admin", roleNames);
            Assert.Contains("Manager", roleNames);
            Assert.Contains("User", roleNames);
        }

        [Fact]
        public async Task DatabaseSeeder_ReconcilesLegacyFanRoleIntoUser()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            if (!await roleManager.RoleExistsAsync("Fan"))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "Fan", NormalizedName = "FAN" });
            }

            var legacyUser = new ApplicationUser
            {
                UserName = "legacyfan@test.com",
                Email = "legacyfan@test.com",
                FirstName = "Legacy",
                LastName = "Fan",
                FullName = "Legacy Fan",
                Role = "Fan",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(legacyUser, "Pass@word1");
            Assert.True(createResult.Succeeded);
            await userManager.AddToRoleAsync(legacyUser, "Fan");

            await DatabaseSeeder.SeedDataAsync(context, userManager, roleManager);

            var roleNames = await context.Roles
                .Select(role => role.Name)
                .ToArrayAsync();

            Assert.Equal(4, roleNames.Length);
            Assert.DoesNotContain("Fan", roleNames);
            Assert.Contains("User", roleNames);

            var reloadedUser = await userManager.FindByEmailAsync("legacyfan@test.com");
            Assert.NotNull(reloadedUser);
            Assert.True(await userManager.IsInRoleAsync(reloadedUser!, "User"));
            Assert.False(await userManager.IsInRoleAsync(reloadedUser!, "Fan"));
        }

        [Fact]
        public void PasswordPolicy_IsConfiguredAsRequired()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();

            var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;

            Assert.Equal(8, identityOptions.Password.RequiredLength);
            Assert.True(identityOptions.Password.RequireUppercase);
            Assert.True(identityOptions.Password.RequireLowercase);
            Assert.True(identityOptions.Password.RequireDigit);
            Assert.True(identityOptions.Password.RequireNonAlphanumeric);
        }

        [Fact]
        public async Task Users_CanBeCreatedSuccessfully()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "fan1@test.com",
                Email = "fan1@test.com",
                FullName = "User One",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Pass@word1");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task EmailUniqueness_IsEnforced()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var first = new ApplicationUser
            {
                UserName = "dup@test.com",
                Email = "dup@test.com",
                FullName = "Dup One"
            };

            var second = new ApplicationUser
            {
                UserName = "dup2@test.com",
                Email = "dup@test.com",
                FullName = "Dup Two"
            };

            var firstResult = await userManager.CreateAsync(first, "Pass@word1");
            var secondResult = await userManager.CreateAsync(second, "Pass@word1");

            Assert.True(firstResult.Succeeded);
            Assert.False(secondResult.Succeeded);
            Assert.Contains(secondResult.Errors, error => error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Users_CanBeAssignedRoles()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await roleManager.CreateAsync(new IdentityRole("Admin"));

            var user = new ApplicationUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FullName = "Admin Test"
            };

            await userManager.CreateAsync(user, "Pass@word1");
            var addRoleResult = await userManager.AddToRoleAsync(user, "Admin");

            Assert.True(addRoleResult.Succeeded);
            Assert.True(await userManager.IsInRoleAsync(user, "Admin"));
        }

        [Fact]
        public async Task MultipleRoles_CanBeAssignedToSingleUser()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Manager"));

            var user = new ApplicationUser
            {
                UserName = "multi@test.com",
                Email = "multi@test.com",
                FullName = "Multi Role"
            };

            await userManager.CreateAsync(user, "Pass@word1");
            await userManager.AddToRoleAsync(user, "Admin");
            await userManager.AddToRoleAsync(user, "Manager");

            var roles = await userManager.GetRolesAsync(user);
            Assert.Contains("Admin", roles);
            Assert.Contains("Manager", roles);
        }

        [Fact]
        public void IdentityModel_ContainsExpectedTables()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tableNames = context.Model.GetEntityTypes()
                .Select(entity => entity.GetTableName())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.Contains("AspNetUsers", tableNames);
            Assert.Contains("AspNetRoles", tableNames);
            Assert.Contains("AspNetUserRoles", tableNames);
            Assert.Contains("AspNetUserClaims", tableNames);
            Assert.Contains("AspNetUserLogins", tableNames);
            Assert.Contains("AspNetUserTokens", tableNames);
        }

        [Fact]
        public void CreatedByRelationships_AreConfiguredForCoreEntities()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            AssertForeignKey(context, typeof(Club), nameof(Club.CreatedById));
            AssertForeignKey(context, typeof(Player), nameof(Player.CreatedById));
            AssertForeignKey(context, typeof(Contract), nameof(Contract.CreatedById));
            AssertForeignKey(context, typeof(Match), nameof(Match.CreatedById));
        }

        [Fact]
        public void AddIdentityTablesMigration_ContainsIdentityTableCreation()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var solutionRoot = Path.GetFullPath(Path.Combine(assemblyPath, "..", "..", "..", ".."));
            var migrationPath = Path.Combine(solutionRoot, "BackendAPI", "Migrations");

            if (!Directory.Exists(migrationPath))
            {
                throw new DirectoryNotFoundException($"Migrations folder was not found: {migrationPath}");
            }

            var identityMigrationFile = Directory
                .GetFiles(migrationPath, "*AddIdentityTables*.cs", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(file => !file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase));

            Assert.False(string.IsNullOrWhiteSpace(identityMigrationFile));

            var migrationSource = File.ReadAllText(identityMigrationFile!);

            Assert.Contains("AspNetUsers", migrationSource);
            Assert.Contains("AspNetRoles", migrationSource);
            Assert.Contains("AspNetUserRoles", migrationSource);
        }

        private static void AssertForeignKey(ApplicationDbContext context, Type entityType, string foreignKeyProperty)
        {
            var entity = context.Model.FindEntityType(entityType);
            Assert.NotNull(entity);

            var fk = entity!.GetForeignKeys()
                .FirstOrDefault(k => k.Properties.Any(p => p.Name == foreignKeyProperty)
                    && k.PrincipalEntityType.ClrType == typeof(ApplicationUser));

            Assert.NotNull(fk);
        }
    }
}

