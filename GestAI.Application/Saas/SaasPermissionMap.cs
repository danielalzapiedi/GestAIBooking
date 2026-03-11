using GestAI.Domain.Entities;
using GestAI.Domain.Enums;

namespace GestAI.Application.Saas;

public static class SaasPermissionMap
{
    public static bool HasAccess(InternalUserRole? role, SaasPlanDefinition? plan, SaasModule module, bool isOwner)
    {
        if (isOwner) return IsEnabledByPlan(plan, module);
        if (role is null) return false;

        var byRole = role.Value switch
        {
            InternalUserRole.Owner => true,
            InternalUserRole.Admin => module is not SaasModule.Users || true,
            InternalUserRole.Reception => module is SaasModule.Dashboard or SaasModule.Bookings or SaasModule.Guests or SaasModule.Payments,
            InternalUserRole.Operations => module is SaasModule.Dashboard or SaasModule.Housekeeping or SaasModule.Properties or SaasModule.Units,
            _ => false
        };

        return byRole && IsEnabledByPlan(plan, module);
    }

    public static bool IsEnabledByPlan(SaasPlanDefinition? plan, SaasModule module)
    {
        if (plan is null) return false;
        return module switch
        {
            SaasModule.Reports => plan.IncludesReports,
            SaasModule.Housekeeping => plan.IncludesOperations,
            _ => true
        };
    }
}
