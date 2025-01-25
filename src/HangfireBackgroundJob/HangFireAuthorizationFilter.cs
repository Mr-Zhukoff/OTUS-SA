using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace HangfireService;

public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
    //public bool Authorize([NotNull] DashboardContext context)
    //{
    //    return context.GetHttpContext().User.Identity.IsAuthenticated;
    //}
}
