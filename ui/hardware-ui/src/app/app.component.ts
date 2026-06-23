import { Component, OnInit, OnDestroy } from '@angular/core';
import { HardwareService, HardwareMessage } from './hardware.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  // Empty arrays waiting for API data
  public availableDevices: string[] = [];
  public selectedDevice: string = '';
  
  public writePayload: string = '';
  public writeEncoding: string = 'ASCII';
  
  public liveMessages: HardwareMessage[] = [];
  private updateSub: Subscription | undefined;

  constructor(private hardwareService: HardwareService) {}

  ngOnInit(): void {
    // 1. Fetch available devices dynamically
    this.hardwareService.getAvailableDevices().subscribe({
      next: (devices: string[]) => {
        this.availableDevices = devices;
        // Auto-select the first device in the list if available
        if (this.availableDevices.length > 0) {
          this.selectedDevice = this.availableDevices[0];
        }
      },
      error: (err) => console.error('Failed to load available hardware configurations', err)
    });

    // 2. Subscribe to real-time SignalR updates
    this.updateSub = this.hardwareService.hardwareUpdates$.subscribe((msg: HardwareMessage) => {
      this.liveMessages.unshift(msg); 
      if (this.liveMessages.length > 50) {
        this.liveMessages.pop();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.updateSub) {
      this.updateSub.unsubscribe();
    }
  }

  onStart() {
    if (!this.selectedDevice) return;
    this.hardwareService.startDevice(this.selectedDevice).subscribe({
      next: (res: any) => console.log(res.message),
      error: (err) => console.error(err)
    });
  }

  onStop() {
    if (!this.selectedDevice) return;
    this.hardwareService.stopDevice(this.selectedDevice).subscribe({
      next: (res: any) => console.log(res.message),
      error: (err) => console.error(err)
    });
  }

  onWrite() {
    if (!this.writePayload || !this.selectedDevice) return;
    
    this.hardwareService.writeToDevice(this.selectedDevice, this.writePayload, this.writeEncoding).subscribe({
      next: (res: any) => {
        console.log('Write success', res);
        this.liveMessages.unshift({
          configKey: this.selectedDevice,
          data: `[TX OUT] Sent command: ${this.writePayload}`,
          timestamp: new Date()
        });
        this.writePayload = ''; 
      },
      error: (err) => {
        console.error(err);
        this.liveMessages.unshift({
          configKey: this.selectedDevice,
          data: `[TX ERROR] Failed to send command. Ensure device is Started.`,
          timestamp: new Date()
        });
      }
    });
  }

  clearLogs() {
    this.liveMessages = [];
  }
}