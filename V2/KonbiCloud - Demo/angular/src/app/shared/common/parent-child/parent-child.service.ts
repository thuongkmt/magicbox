import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class ParentChildService {

  constructor() { }

  private subjects: Subject<any>[] = [];

  publish(eventName: string, data: any) {
    // ensure a subject for the event name exists
    this.subjects[eventName] = this.subjects[eventName] || new Subject();
    // publish event
    this.subjects[eventName].next(data);
  }

  on(eventName: string): Observable<any> {
    // ensure a subject for the event name exists
    this.subjects[eventName] = this.subjects[eventName] || new Subject();
    // return observable
    return this.subjects[eventName].asObservable();
  }

}
