import { Component, OnInit, OnDestroy } from '@angular/core';
import { HardwareService, HardwareMessage } from './hardware.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  // Available devices from your hardware.dev.json
  public availableDevices: string[] = ['Scanner1_TCP', 'Conveyor_Modbus', 'Scale_Serial','Scanner_Mock'];
  public selectedDevice: string = 'Scanner1_TCP';
  
  public writePayload: string = '';
  public writeEncoding: string = 'ASCII';
  
  public liveMessages: HardwareMessage[] = [];
  private updateSub: Subscription | undefined;

  constructor(private hardwareService: HardwareService) {}

  ngOnInit(): void {
    // Subscribe to real-time SignalR updates
    this.updateSub = this.hardwareService.hardwareUpdates$.subscribe((msg: HardwareMessage) => {
      this.liveMessages.unshift(msg); // Push to top of the array
      // Keep only the last 50 messages to prevent browser memory leaks
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
    this.hardwareService.startDevice(this.selectedDevice).subscribe({
      next: (res: any) => console.log(res.message),
      error: (err) => console.error(err)
    });
  }

  onStop() {
    this.hardwareService.stopDevice(this.selectedDevice).subscribe({
      next: (res: any) => console.log(res.message),
      error: (err) => console.error(err)
    });
  }

  onWrite() {
    if (!this.writePayload) return;
    
    this.hardwareService.writeToDevice(this.selectedDevice, this.writePayload, this.writeEncoding).subscribe({
      next: (res: any) => {
        // Log the successful API call
        console.log('Write success', res);
        
        // NEW: Echo the outgoing command manually to the UI feed
        this.liveMessages.unshift({
          configKey: this.selectedDevice,
          data: `[TX OUT] Sent command: ${this.writePayload}`,
          timestamp: new Date()
        });

        // Clear the input box
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