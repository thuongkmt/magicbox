import { Component, OnInit } from '@angular/core';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.sass']
})
export class MessagesComponent implements OnInit {

  public receivedMessages: string[] = [];
  private topicSubscription: Subscription;

  
  constructor(private rxStompService: RxStompService) { }

  ngOnInit() {
    this.topicSubscription =  this.rxStompService.watch('/topic/demo').subscribe((message: Message) => {
      console.log(message.body);
      this.receivedMessages.push(message.body);
    });
  }

  ngOnDestroy() {
    this.topicSubscription.unsubscribe();
  }

  onSendMessage() {
    console.log('onmessage');
    const message = `Message generated at ${new Date}`;
    this.rxStompService.publish({destination: '/topic/demo', body: message});
  }

}
