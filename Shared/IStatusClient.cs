using System.Collections.Generic;
using System.Threading.Tasks;

namespace Status.Shared
{
    public interface IStatusClient
    {
        Task ShowRoomStatuses(Dictionary<int, MuebStatus> roomStatuses);
    }
}