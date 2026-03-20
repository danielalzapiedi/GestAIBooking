using GestAI.Application.Saas;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Xunit;

namespace GestAI.Tests;

public class SaasPermissionMapTests
{
    [Fact]
    public void Admin_Should_Have_Access_To_Configured_Modules_When_Plan_Allows_It()
    {
        var plan = BuildPlan(includesReports: true, includesOperations: true);

        Assert.True(SaasPermissionMap.HasAccess(InternalUserRole.Admin, plan, SaasModule.Configuration, isOwner: false));
        Assert.True(SaasPermissionMap.HasAccess(InternalUserRole.Admin, plan, SaasModule.Users, isOwner: false));
        Assert.True(SaasPermissionMap.HasAccess(InternalUserRole.Admin, plan, SaasModule.Reports, isOwner: false));
    }

    [Fact]
    public void Admin_Should_Not_Have_Access_When_Plan_Does_Not_Include_Module()
    {
        var plan = BuildPlan(includesReports: false, includesOperations: false);

        Assert.False(SaasPermissionMap.HasAccess(InternalUserRole.Admin, plan, SaasModule.Reports, isOwner: false));
        Assert.False(SaasPermissionMap.HasAccess(InternalUserRole.Admin, plan, SaasModule.Housekeeping, isOwner: false));
    }

    [Fact]
    public void Reception_Should_Only_Access_Operational_Commercial_Subset()
    {
        var plan = BuildPlan(includesReports: true, includesOperations: true);

        Assert.True(SaasPermissionMap.HasAccess(InternalUserRole.Reception, plan, SaasModule.Bookings, isOwner: false));
        Assert.True(SaasPermissionMap.HasAccess(InternalUserRole.Reception, plan, SaasModule.Payments, isOwner: false));
        Assert.False(SaasPermissionMap.HasAccess(InternalUserRole.Reception, plan, SaasModule.Configuration, isOwner: false));
        Assert.False(SaasPermissionMap.HasAccess(InternalUserRole.Reception, plan, SaasModule.Units, isOwner: false));
    }

    private static SaasPlanDefinition BuildPlan(bool includesReports, bool includesOperations)
        => new()
        {
            Id = 1,
            Code = SaasPlanCode.Pro,
            Name = "Pro",
            MaxProperties = 3,
            MaxUnits = 20,
            MaxUsers = 5,
            IncludesReports = includesReports,
            IncludesOperations = includesOperations,
            IncludesPublicPortal = true
        };
}
