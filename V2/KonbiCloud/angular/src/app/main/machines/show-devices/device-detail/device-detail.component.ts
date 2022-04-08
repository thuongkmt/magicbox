import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-device-detail',
  templateUrl: './device-detail.component.html',
  styleUrls: ['./device-detail.component.css']
})
export class DeviceDetailComponent implements OnInit {

  @Input() device: any;
  // device: any;
  constructor() { }

  ngOnInit() {
    // this.device =  {
    //   Name: 'VMC',
    //   Code: 'WHITE_VMC',
    //   Port: 'COM3',
    //   Status: 'Abnormal',
    //   CustonInfoes:[{"Property":"Level","Value":"W"}]
    // }
  }

}
 