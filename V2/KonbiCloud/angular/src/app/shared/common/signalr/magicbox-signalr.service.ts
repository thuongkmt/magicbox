import { Injectable, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@aspnet/signalr';
import { MagicBoxMessageType } from './common-model';

@Injectable()
export class MagicBoxSignalrService extends AppComponentBase {

    constructor(
        injector: Injector,
        public _zone: NgZone
    ) {
        super(injector);
    }

    messageHub: HubConnection;
    isMessageConnected = false;

    configureConnection(connection): void {
        // Set the common hub
        this.messageHub = connection;

        // Reconnect if hub disconnects
        connection.onclose(e => {
            this.isMessageConnected = false;
            if (e) {
                abp.log.debug('Message connection closed with error: ' + e);
            } else {
                abp.log.debug('Message disconnected');
            }

            if (!abp.signalr.autoConnect) {
                return;
            }

            setTimeout(() => {
                connection.start().then(result => {
                    this.isMessageConnected = true;
                });
            }, 5000);
        });

        // Register to get notifications
        this.registerMessageEvents(connection);
    }

    registerMessageEvents(connection): void {
        connection.on('MagicBoxMessage', message => {
            if (message.messageType == MagicBoxMessageType.MachineStatus) {
                abp.event.trigger('MagicBoxMachineStatusMessage', message.machineId);
            }
            else if(message.messageType == MagicBoxMessageType.CurrentInventory)
            {
                abp.event.trigger('CurrentInventory', message.machineId);
            }
            else if(message.messageType == MagicBoxMessageType.ProductTag)
            {
                abp.event.trigger('ProductTag');
            }
            else if(message.messageType == MagicBoxMessageType.Topup)
            {
                abp.event.trigger('Topup');
            }
            else if(message.messageType == MagicBoxMessageType.Transaction)
            {
                abp.event.trigger('Transaction');
            }
        });
    }

    sendMessage(messageData, callback): void {
        if (!this.isMessageConnected) {
            if (callback) {
                callback();
            }

            abp.notify.warn(this.l('MessageIsNotConnectedWarning'));
            return;
        }

        // this.messageHub.invoke('sendMessage', messageData).then(result => {
        //     if (result) {
        //         abp.notify.warn(result);
        //     }

        //     if (callback) {
        //         callback();
        //     }
        // }).catch(error => {
        //     abp.log.error(error);

        //     if (callback) {
        //         callback();
        //     }
        // });
    }

    init(): void {
        this._zone.runOutsideAngular(() => {
            abp.signalr.connect();
            abp.signalr.startConnection(abp.appPath + 'signalr-magicbox', connection => {
                abp.event.trigger('app.message.connected');
                this.isMessageConnected = true;
                this.configureConnection(connection);
            });
        });
    }
}
