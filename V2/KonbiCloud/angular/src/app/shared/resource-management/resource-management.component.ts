import {
    Component, OnInit, ViewChild, Injector,
    Inject, Optional,
    Input, Output, EventEmitter
} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { concat, includes, remove, map } from 'lodash';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ResourceServiceProxy, ResourceDto, API_BASE_URL } from '@shared/service-proxies/service-proxies';


@Component({
    selector: 'app-resource-management',
    styleUrls: ['./resource-management.component.less'],
    templateUrl: './resource-management.component.html'
})
export class ResourceManagementComponent extends AppComponentBase implements OnInit {
    baseRemoteUrl: string;
    resourceList: ResourceDto[] = [];
    selectedIds: string[] = [];
    @Input() maxItems: number = 5; // 5MB
    @Output() selectImageUrl = new EventEmitter();

    @ViewChild('resourceManagementModal') modal: ModalDirective;
    constructor(injector: Injector,
        private resourceService: ResourceServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,

    ) {
        super(injector);
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
    }

    ngOnInit() {

    }

    reloadResources = () => {
        this.primengTableHelper.showLoadingIndicator();
        this.resourceService.getAll().subscribe(result => {
            this.resourceList = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    isSelected = (id) => {
        return includes(this.selectedIds, id);
    }

    selectItem = (selectedId) => {
        if (includes(this.selectedIds, selectedId)) {
            remove(this.selectedIds, item => item === selectedId);
        } else {
            if (this.selectedIds.length < this.maxItems) {
                this.selectedIds = concat(this.selectedIds, selectedId);
            }
        }
    }

    add = () => {
        if (this.selectedIds.length > 0) {
            const lst = map(this.selectedIds, id => {
                return this.resourceList.find(item => item.id === id);
            });

            this.selectImageUrl.emit(map(lst, (item) => {
                return {
                    thumbnail: item.thumbnail,
                    fileName: item.fileName
                };
            }));
            this.modal.hide();
        }
    }

    show = () => {
        this.reloadResources();
        this.modal.show();
    }

    close = () => {
        this.selectedIds = [];
        this.modal.hide();
    }

    handleImageLoadFailed = (event) => {
        event.target.src = `/assets/images/not_found.png`;
    }

    uploadStatusChange = (status) => {
        if (status) {
            this.reloadResources();
        }
    }
}
