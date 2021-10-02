import {Component, Inject, OnInit} from '@angular/core';
import * as signalR from '@microsoft/signalr';

enum MuebStatus {
  Offline,
  Online,
  PwmPanelOffline,
  IpConflict
}

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  readonly columns: number[] = Array.from(Array(8).keys());
  readonly rows: number[] = Array.from(Array(13).keys());
  readonly roomCount: number = 104;
  hubConnection: signalR.HubConnection;
  roomStatuses: Map<string, MuebStatus> = new Map<string, MuebStatus>();
  percent: number = 0;
  onlineRoomCount: number = 0;

  readonly roomStatusToCssClass: Map<MuebStatus, string> = new Map<MuebStatus, string>([[MuebStatus.Offline, 'table-warning'],
    [MuebStatus.Online, 'table-success'],
    [MuebStatus.PwmPanelOffline, 'table-dark'],
    [MuebStatus.IpConflict, 'table-danger']
  ]);
  readonly MuebStatus = MuebStatus;

  constructor(@Inject('BASE_URL') baseUrl: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(baseUrl + 'hubs/status')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))

    this.hubConnection.on('ShowRoomStatuses', (data) => {
      this.roomStatuses = new Map(Object.entries(data));
      this.onlineRoomCount = Array.from(this.roomStatuses.values()).filter(x => x === MuebStatus.Online).length;
      this.percent = this.onlineRoomCount / this.roomCount;
    });
  }

  ngOnInit(): void {
  }

  getRoomStatusCssClass(roomNumber: number): string {
    if (this.roomStatuses && this.roomStatuses.has(roomNumber.toString())) {
      return this.roomStatusToCssClass.get(this.roomStatuses.get(roomNumber.toString())!)!;
    }

    return '';
  }
}
