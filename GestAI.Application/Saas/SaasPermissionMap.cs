using GestAI.Domain.Entities;
using GestAI.Domain.Enums;

namespace GestAI.Application.Saas;

public static class SaasPermissionMap
{
    public static bool HasAccess(InternalUserRole? role, SaasPlanDefinition? plan, SaasModule module, bool isOwner)
    {
        if (!IsEnabledByPlan(plan, module))
            return false;

        if (isOwner)
            return true;

        if (role is null)
            return false;

        return role.Value switch
        {
            InternalUserRole.Owner => true,
            InternalUserRole.Admin => true,
            InternalUserRole.Reception => module is SaasModule.Dashboard or SaasModule.Bookings or SaasModule.Guests or SaasModule.Payments,
            InternalUserRole.Operations => module is SaasModule.Dashboard or SaasModule.Housekeeping or SaasModule.Properties or SaasModule.Units,
            _ => false
        };
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
