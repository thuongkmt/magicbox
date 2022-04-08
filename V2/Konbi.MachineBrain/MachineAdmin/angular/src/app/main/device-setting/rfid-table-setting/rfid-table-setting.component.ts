import { Component, OnInit, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@aspnet/signalr';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import * as $ from 'jquery';
import { debug } from 'util';

@Component({
    selector: 'app-rfid-table-setting',
    templateUrl: './rfid-table-setting.component.html',
    styleUrls: ['./rfid-table-setting.component.css']
})
export class RfidTableSettingComponent extends AppComponentBase {

    constructor(
        injector: Injector,
        public _zone: NgZone
    ) {
        super(injector);
    }
    isServiceRunning;
    comPortList = ["Fetching.."];
    selectedPort = "";
    detectedPlates = [];
    ngOnInit() {
        let self = this
        SignalRHelper.initSignalR(function (this) {

            // abp.signalr.connect();
            if (!self.isSignalrInited) {
                self.isSignalrInited = true;
                abp.signalr.autoConnect = true;
                abp.signalr.startConnection(abp.appPath + 'signalr-rfidtable', connection => {
                    self.configureConnection(connection);

                }).then(function (connection) {
                    self.isConnected = true;
                    //abp.event.trigger('app.rfidtable.connected');
                    connection.invoke('JoinGroup', "TableDeviceSettingGroup").then(function () {
                        console.log('JoinGroup TableDeviceSettingGroup');
                    });
                });
            }

        });

       
       
       

    }
    ngOnDestroy() {
        this.rftableHub.stop();
        abp.signalr.autoConnect = false;
    }
    rftableHub: HubConnection;
    isSignalrInited = false;
    isConnected = false;

    configureConnection(connection): void {
        // Set the common hub
        this.rftableHub = connection;

        // Reconnect if hub disconnects
        connection.onclose(e => {
            this.isConnected = false;
            if (e) {
                abp.log.debug('rftable connection closed with error: ' + e);
            } else {
                abp.log.debug('rftable disconnected');
            }

            if (!abp.signalr.autoConnect) {
                return;
            }
            abp.notify.warn("Disconnected from server");
            setTimeout(() => {
                abp.notify.warn("Reconnecting...");
                connection.start().then(result => {
                    this.isConnected = true;
                    abp.notify.info("Connected");
                });
            }, 5000);
        });

        // Register to get notifications
        this.registerMessageEvents(connection);
    }

    registerMessageEvents(connection): void {
        let self = this;
        connection.on('updateTableSettings', data => {
            console.log("updateTableSettings");
            console.log(data);
            self.isServiceRunning = data.isServiceRunning;
            if (data.selectedPort != '')
                self.selectedPort = data.selectedPort;
            self.comPortList = data.availablePorts;
            self.comPortList.unshift("");            

        });
        connection.on('detectedPlates', plates => {
            console.log(plates);
            self.detectedPlates = plates;
        });
        
    }


    startService(): void{
        console.log('start service');
        this.rftableHub.invoke('StartTableService', this.selectedPort).then(function (data) {
            console.log(data);
        });
    }
    stopService(): void {
        console.log('stop service');
        this.rftableHub.invoke('StopTableService').then(function (data) {
            console.log(data);            
        });
    }
    readPlates(): void {
        console.log('force to read plates');
        this.rftableHub.invoke('ReadPlates').then(function (data) {
            console.log(data);
        });
    }




}
