import { Component, OnInit, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { UploadServiceProxy } from '@shared/service-proxies/upload-service-proxies';

@Component({
    selector: 'app-file-upload',
    styleUrls: ['./file-upload.component.less'],
    templateUrl: './file-upload.component.html'
})
export class FileUploadComponent implements OnInit {
    errors: Array<string> = [];
    isDrop: boolean = false;
    @Input() fileExt: string = "JPG, GIF, PNG";
    @Input() maxFiles: number = 15;
    @Input() maxSize: number = 5; // 5MB
    @Input() thumbWidth: number = 120; 
    @Input() thumbHeight: number = 120; 
    @Output() uploadStatus = new EventEmitter();

    constructor(private uploadService: UploadServiceProxy) { }

    ngOnInit() { }

    changeFiles(event) {
        let files = event.target.files;
        this.saveFiles(files);
    }

    @HostListener('dragover', ['$event']) onDragOver(event) {
        this.isDrop = true;
        event.preventDefault();
    }

    @HostListener('dragenter', ['$event']) onDragEnter(event) {
        this.isDrop = true;
        event.preventDefault();
    }

    @HostListener('dragend', ['$event']) onDragEnd(event) {
        this.isDrop = false;
        event.preventDefault();
    }

    @HostListener('dragleave', ['$event']) onDragLeave(event) {
        this.isDrop = false;
        event.preventDefault();
    }

    @HostListener('drop', ['$event']) onDrop(event) {
        this.isDrop = false;
        event.preventDefault();
        event.stopPropagation();
        let files = event.dataTransfer.files;
        this.saveFiles(files);
    }

    saveFiles(files) {
        this.errors = [];
        if (files.length > 0 && (!this.isValidFiles(files))) {
            this.uploadStatus.emit(false);
            return;
        }

        if (files.length > 0) {
            let formData: FormData = new FormData();
            for (let j = 0; j < files.length; j++) {
                formData.append("file[]", files[j], files[j].name);
            }

            const params = {
                thumbWidth: this.thumbWidth,
                thumbHeight: this.thumbHeight
            }

            this.uploadService.upload(formData, params)
                .subscribe(
                    success => {
                        this.uploadStatus.emit(true);
                    },
                    error => {
                        this.uploadStatus.emit(false);
                        this.errors.push(error.ExceptionMessage);
                    });
        }
    }

    private isValidFiles(files) {
        if (files.length > this.maxFiles) {
            this.errors.push("Error: At a time you can upload only " + this.maxFiles + " files");
            return;
        }
        this.isValidFileExtension(files);
        return this.errors.length === 0;
    }

    private isValidFileExtension(files) {
        let extensions = (this.fileExt.split(',')).map(function (x) { return x.toLocaleUpperCase().trim()});
        for (let i = 0; i < files.length; i++) {
            let ext = files[i].name.toUpperCase().split('.').pop() || files[i].name;
            let exists = extensions.includes(ext);
            if (!exists) {
                this.errors.push("Error (Extension): " + files[i].name);
            }
            this.isValidFileSize(files[i]);
        }
    }

    private isValidFileSize(file) {
        let fileSizeinMB = file.size / (1024 * 1000);
        let size = Math.round(fileSizeinMB * 100) / 100;
        if (size > this.maxSize) {
            this.errors.push("Error (File Size): " + file.name + ": exceed file size limit of " + this.maxSize + "MB ( " + size + "MB )");
        }
    }
}
