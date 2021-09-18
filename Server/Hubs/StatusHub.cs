using Microsoft.AspNetCore.SignalR;
using Status.Shared;

namespace Status.Server.Hubs
{
    public class StatusHub : Hub<IStatusClient>
    {
    }
}