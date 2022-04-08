import { TestBed, inject } from '@angular/core/testing';

import { ParentChildService } from './parent-child.service';

describe('ParentChildService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ParentChildService]
    });
  });

  it('should be created', inject([ParentChildService], (service: ParentChildService) => {
    expect(service).toBeTruthy();
  }));
});
