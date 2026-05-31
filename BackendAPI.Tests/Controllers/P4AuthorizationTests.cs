using System;
using System.Linq;
using System.Reflection;
using FootballClubAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace BackendAPI.Tests.Controllers
{
    public class P4AuthorizationTests
    {
        [Theory]
        [InlineData(typeof(TransfersController))]
        [InlineData(typeof(ContractsController))]
        [InlineData(typeof(InjuriesController))]
        [InlineData(typeof(DashboardController))]
        public void P4Controllers_RequireAuthentication(Type controllerType)
        {
            var authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(authorize);
            Assert.True(string.IsNullOrWhiteSpace(authorize!.Roles));
        }

        [Theory]
        [InlineData(typeof(TransfersController), "CreateTransfer")]
        [InlineData(typeof(TransfersController), "UpdateTransfer")]
        [InlineData(typeof(ContractsController), "CreateContract")]
        [InlineData(typeof(ContractsController), "UpdateContract")]
        [InlineData(typeof(InjuriesController), "CreateInjury")]
        [InlineData(typeof(InjuriesController), "UpdateInjury")]
        public void P4WriteEndpoints_AreManagerOnly(Type controllerType, string methodName)
        {
            var authorize = GetAuthorizeAttribute(controllerType, methodName);

            Assert.Equal("Manager", authorize.Roles);
        }

        [Theory]
        [InlineData(typeof(TransfersController), "DeleteTransfer")]
        [InlineData(typeof(ContractsController), "DeleteContract")]
        [InlineData(typeof(InjuriesController), "DeleteInjury")]
        public void P4DeleteEndpoints_AreAdminOnly(Type controllerType, string methodName)
        {
            var authorize = GetAuthorizeAttribute(controllerType, methodName);

            Assert.Equal("Admin", authorize.Roles);
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(Type controllerType, string methodName)
        {
            var method = controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(method => method.Name == methodName);

            var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(authorize);
            return authorize!;
        }
    }
}
