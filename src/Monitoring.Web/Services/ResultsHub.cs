using Microsoft.AspNetCore.SignalR;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// SignalR hub used to push check results to connected clients. The scheduler
    /// writes results into the result store and broadcasts them on this hub.
    /// Clients subscribe to receive notifications when checks run.
    /// </summary>
    public class ResultsHub : Hub
    {
    }
}