import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface HardwareMessage {
  configKey: string;
  data: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class HardwareService {
  private apiUrl = 'http://localhost:5168/api/Hardware';
  private hubUrl = 'http://localhost:5168/hubs/hardware';
  private hubConnection: signalR.HubConnection | undefined;

  // Observable stream for the UI to subscribe to
  public hardwareUpdates$ = new Subject<HardwareMessage>();

  constructor(private http: HttpClient) {
    this.startSignalRConnection();
  }

  // --- REST API CALLS ---

  public startDevice(key: string) {
    return this.http.get(`${this.apiUrl}/Start/${key}`, {});
  }

  public stopDevice(key: string) {
    return this.http.get(`${this.apiUrl}/Stop/${key}`, {});
  }

  public writeToDevice(key: string, payload: string, encoding: string = 'ASCII') {
    const body = {
      Data: payload,
      EncodingFormat: encoding
    };
    return this.http.post(`${this.apiUrl}/Write/${key}`, body);
  }

  // --- SIGNALR WEBSOCKET LOGIC ---

  private startSignalRConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .withAutomaticReconnect() // Resilient connection
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection established.'))
      .catch(err => console.error('Error while starting SignalR connection: ' + err));

    // Listen for the exact method name called from C# `_hubContext.Clients.All.SendAsync("ReceiveHardwareUpdate", ...)`
    this.hubConnection.on('ReceiveHardwareUpdate', (configKey: string, data: string) => {
      this.hardwareUpdates$.next({
        configKey: configKey,
        data: data,
        timestamp: new Date()
      });
    });
  }
  // NEW: Fetch the list of devices from the C# configuration
  public getAvailableDevices() {
    return this.http.get<string[]>(`${this.apiUrl}/Available`);
  }
}