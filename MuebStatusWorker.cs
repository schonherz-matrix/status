using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Status.Data;
using Status.Hubs;

namespace Status;

public class MuebStatusWorker : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger;
    private readonly IHubContext<StatusHub, IStatusClient> _roomStatusHub;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly UdpClient _udpClient = new();

    public MuebStatusWorker(ILogger<BackgroundService> logger,
        IHubContext<StatusHub, IStatusClient> roomStatusHub, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _roomStatusHub = roomStatusHub;
        _scopeFactory = scopeFactory;
        _udpClient.Client.ReceiveTimeout = 100;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTime.Now);

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchmatrixDbContext>();

                var roomStatuses = new Dictionary<int, MuebStatus>();
                foreach (var muebWithIp in await context.MuebWithIps.ToListAsync())
                {
                    var status = MuebStatus.Offline;
                    if (muebWithIp.IpConflict)
                    {
                        status = MuebStatus.IpConflict;
                    }
                    else
                    {
                        var ipEndPoint = new IPEndPoint(muebWithIp.IpAddress, 50000);

                        try
                        {
                            var command = Encoding.ASCII.GetBytes("SEM\x00\x0F");
                            await _udpClient.SendAsync(command, command.Length, ipEndPoint);

                            var panelStates = _udpClient.Receive(ref ipEndPoint);
                            if (panelStates.Length == 2)
                            {
                                if (panelStates[0] != 3 || panelStates[1] != 3)
                                    status = MuebStatus.PwmPanelOffline;
                                else
                                    status = MuebStatus.Online;
                            }
                            else
                            {
                                // Should not happen
                                _logger.LogError(
                                    "Invalid panel state response with length: {Length} at {IpAddress}",
                                    panelStates.Length, muebWithIp.IpAddress);
                            }
                        }
                        catch
                        {
                            _logger.LogInformation("MUEB {MuebId} with {Ip} not responding at {CurrentTime}",
                                muebWithIp.MuebId, muebWithIp.IpAddress, DateTime.Now);
                        }
                    }

                    roomStatuses.Add(muebWithIp.RoomId, status);
                }

                await _roomStatusHub.Clients.All.ShowRoomStatuses(roomStatuses);
            }

            await Task.Delay(1000);
        }
    }
}