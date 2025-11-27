using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverContingencyRepository
{
    IReadOnlyList<HandoverContingencyPlanRecord> GetHandoverContingencyPlans(string handoverId);
    HandoverContingencyPlanRecord CreateContingencyPlan(string handoverId, string conditionText, string actionText, string priority, string createdBy);
    bool DeleteContingencyPlan(string handoverId, string contingencyId);
}
