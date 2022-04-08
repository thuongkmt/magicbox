import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { MachineServiceProxy } from "@shared/service-proxies/machine-service-proxies";
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-show-devices',
  templateUrl: './show-devices.component.html',
  styleUrls: ['./show-devices.component.css']
})
export class ShowDevicesComponent extends AppComponentBase {

  machine : any = null;
  devices: any[] = [];

  constructor(
    private injector: Injector,
    private machinesService: MachineServiceProxy,
    private _route: ActivatedRoute,
  ) {
    super(injector);
      this.machine={
        Name:''
      }
   }

  

  ngOnInit() {
    const machineId = this._route.snapshot.paramMap.get("machineId");
    this.machine.Name = this._route.snapshot.paramMap.get("machineName");
    //init devices
  
    this.machinesService.getMachineDevices(machineId)
    .subscribe(x=>{
      console.log(x);

      x.forEach(deviceItem => {
        var newDevice:any=deviceItem;
        var customInfoes:CustomInfo[]=JSON.parse(deviceItem.custonInfoesJson);
        newDevice.CustonInfoes = customInfoes;
        console.log(newDevice);
        this.devices.push(deviceItem);
      });
    });
  }

}

export class CustomInfo{
  Property:string
  Value:string
}
