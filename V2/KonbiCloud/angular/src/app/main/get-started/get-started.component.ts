import { Component, OnInit, Injector, ChangeDetectorRef } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { GetStartedServiceProxy, GetStartedDataOutput } from '@shared/service-proxies/get-started-service-proxies';
import { Router } from '@angular/router';

@Component({
  selector: 'app-get-started',
  templateUrl: './get-started.component.html',
  styleUrls: ['./get-started.component.css'],
  animations: [appModuleAnimation()]
})
export class GetStartedComponent extends AppComponentBase {

  public isLoading = false;
  public isShowCompleted = false;
  public lstGetStartedStep: GetStartedDataOutput[] = [];

  constructor(
    injector: Injector,
    private _getStartedServiceProxy: GetStartedServiceProxy,
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getGetStartedStatus()
  }

  getGetStartedStatus() {
    this.isLoading = true;
    this._getStartedServiceProxy.getGetStartedStatus().subscribe(result => {
      this.isLoading = false;
      if (result != null) {
        this.lstGetStartedStep = result

        if (this.lstGetStartedStep.find(x => x.stepDoneFlg == 0)) this.isShowCompleted = false;
        else this.isShowCompleted = true;

        this.changeDetectorRef.detectChanges();
      }
    });
  }

  btnCreateStepClick(step: GetStartedDataOutput) {
    if (step.stepId < 6) {
      let routerUrl = step.stepActionUrl;
      this.router.navigate([routerUrl]);
    }
  }

  goToDashboardClick() {
    this.router.navigate(["app/main/dashboard"]);
  }

}
