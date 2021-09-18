using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Status.Server.Data;
using Status.Server.Hubs;
using Status.Shared;

namespace Status.Server
{
    public class MuebStatusWorker : BackgroundService
    {
        private readonly ILogger<BackgroundService> _logger;
        private readonly IHubContext<StatusHub, IStatusClient> _roomStatusHub;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UdpClient _udpClient;

        public MuebStatusWorker(ILogger<BackgroundService> logger,
            IHubContext<StatusHub, IStatusClient> roomStatusHub, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _roomStatusHub = roomStatusHub;
            _scopeFactory = scopeFactory;
            _udpClient = new UdpClient();
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
                                await _udpClient.SendAsync(new byte[] {0x09}, 1, ipEndPoint);
                                var ping = Encoding.ASCII.GetString(_udpClient.Receive(ref ipEndPoint)) == "pong";
                                if (ping)
                                {
                                    status = MuebStatus.Online;

                                    await _udpClient.SendAsync(new byte[] {0x0E}, 1, ipEndPoint);

                                    var panelStates = _udpClient.Receive(ref ipEndPoint);
                                    if ((panelStates[0] & 0xF0) == 0 || (panelStates[1] & 0x0F) == 0)
                                        status = MuebStatus.PwmPanelDisabled;
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
}