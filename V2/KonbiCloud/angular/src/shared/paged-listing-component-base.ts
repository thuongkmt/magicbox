import { AppComponentBase } from "shared/common/app-component-base";
import { Injector, OnInit } from '@angular/core';

export class PagedResultDto {
    items: any[];
    totalCount: number;
}

export class EntityDto {
    id: number;
}

export class PagedRequestDto {
    skipCount: number;
    maxResultCount: number;
}