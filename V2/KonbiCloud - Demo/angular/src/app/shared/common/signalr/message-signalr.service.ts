import { Injectable, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@aspnet/signalr';

@Injectable()
export class MessageSignalrService extends AppComponentBase {

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
        connection.on('getMessage', message => {
            abp.event.trigger('app.message.messageReceived', message);
        });

        connection.on('getTestMessage', message => {
            console.log("signal message", message.message)
            abp.event.trigger('app.message.messageReceived', message);
            
        });

        // connection.on('getAllFriends', friends => {
        //     abp.event.trigger('abp.chat.friendListChanged', friends);
        // });

        // connection.on('getFriendshipRequest', (friendData, isOwnRequest) => {
        //     abp.event.trigger('app.chat.friendshipRequestReceived', friendData, isOwnRequest);
        // });

        connection.on('getUserConnectNotification', (friend, isConnected) => {
            abp.event.trigger('app.chat.userConnectionStateChanged',
                {
                    friend: friend,
                    isConnected: isConnected
                });
        });

        connection.on('getUserStateChange', (friend, state) => {
            abp.event.trigger('app.chat.userStateChanged',
                {
                    friend: friend,
                    state: state
                });
        });

        connection.on('getallUnreadMessagesOfUserRead', friend => {
            abp.event.trigger('app.chat.allUnreadMessagesOfUserRead',
                {
                    friend: friend
                });
        });

        connection.on('getReadStateChange', friend => {
            abp.event.trigger('app.chat.readStateChange',
                {
                    friend: friend
                });
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

        this.messageHub.invoke('sendMessage', messageData).then(result => {
            if (result) {
                abp.notify.warn(result);
            }

            if (callback) {
                callback();
            }
        }).catch(error => {
            abp.log.error(error);

            if (callback) {
                callback();
            }
        });
    }

    init(): void {
        this._zone.runOutsideAngular(() => {
            abp.signalr.connect();
            console.log(abp.appPath + 'signalr-test-message');
            abp.signalr.startConnection(abp.appPath + 'signalr-test-message', connection => {
                abp.event.trigger('app.message.connected');
                this.isMessageConnected = true;
                this.configureConnection(connection);
            });
        });
    }
}
