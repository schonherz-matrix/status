using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Status.Hubs
{
    public class StatusHub : Hub<IStatusClient>
    {
    }

    public interface IStatusClient
    {
        Task ShowRoomStatuses(Dictionary<int, MuebStatus> roomStatuses);
    }

    public enum MuebStatus
    {
        Offline,
        Online,
        PwmPanelOffline,
        IpConflict
    }
}