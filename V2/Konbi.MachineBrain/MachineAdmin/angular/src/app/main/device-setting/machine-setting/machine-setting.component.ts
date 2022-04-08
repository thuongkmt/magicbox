import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { SystemConfigDto, SystemConfigServiceProxy, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';
import { MachineServiceProxy, MachineComboboxDto, ListResultDtoOfMachineComboboxDto } from '@shared/service-proxies/machine-service-proxies';
import { debug } from 'util';

@Component({
  selector: 'app-machine-setting',
  templateUrl: './machine-setting.component.html',
  styleUrls: ['./machine-setting.component.css'],
  animations: [appModuleAnimation()]
})
export class MachineSettingComponent extends AppComponentBase {

  constructor(injector: Injector,
    private systemConfigService: SystemConfigServiceProxy,
    private machineServiceProxy: MachineServiceProxy) {
    super(injector);
  }

  optionsHID: any[];
  options: any[];
  optionsQr: any[];

  paymentOptions: any[];
  qrPaymentOptions: any[];
  cardInsertOptions: any[];
  readerBaurateOptions: any[];
  readerFrameOptions: any[];
  terminalTypeOptions: any[];

  cloudAPIUrl = '';
  
  cloudUrl = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Cloud.CloudApiUrl',
    value: ''
  });

  tenantId = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Cloud.TenantId',
    value: ''
  });

  useCloud = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Cloud.UseCloud',
    value: ''
  });

  machineId = new SystemConfigDto({
    name: 'RfidFridgeSetting.Machine.Id',
    value: ''
  });

  machineName = new SystemConfigDto({
    name: 'RfidFridgeSetting.Machine.Name',
    value: ''
  });

  paymentType = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Type',
    value: ''
  });

  lock = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.Lock',
    value: ''
  });

  qrCode = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.QrCodeReader',
    value: ''
  });


  inventory = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.Inventory',
    value: ''
  });

  cashlessTerminal = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.CashlessTerminal',
    value: ''
  });

  enableTemperature = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Temperature.Enable',
    value: ''
  });

  temperatureComport = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Temperature.Comport',
    value: ''
  });

  ezLinkMin = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire',
    value: ''
  });

  enableCreditCard = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.EnableCreditCard',
    value: ''
  });

  enableCpas = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.EnableCpas',
    value: ''
  });


  cardInsertType = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.CardInsertType',
    value: ''
  });

  cardInsertComport = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.CardInsertComport',
    value: ''
  });

  slackUrl = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Slack.URL',
    value: ''
  });

  slackUsername = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Slack.Username',
    value: ''
  });

  slackAlertChannel = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Slack.AlertChannel',
    value: ''
  });

  slackInfoChannel = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Slack.InfoChannel',
    value: ''
  });

  unstableTagChannel = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Slack.UnstableTagChannel',
    value: ''
  });


  bottomText = new SystemConfigDto({
    name: 'RfidFridgeSetting.Machine.BottomText',
    value: ''
  });

  advsImage = new SystemConfigDto({
    name: 'RfidFridgeSetting.Machine.AdvsImage',
    value: ''
  });

  host = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.Host',
    value: ''
  });

  partnerId = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.PartnerId',
    value: ''
  });

  partnerSecret = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.PartnerSecret',
    value: ''
  });

  clientId = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.ClientId',
    value: ''
  });

  clientSecret = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.ClientSecret',
    value: ''
  });

  grabId = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.GrabId',
    value: ''
  });

  terminalId = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.TerminalId',
    value: ''
  });

  qrPaymentType = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Qr.Type',
    value: ''
  });

  grabPayReserveAmount = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.GrabPay.ReserveAmount',
    value: ''
  });

  teraReserveAmount = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.TeraWallet.ReserveAmount',
    value: ''
  });

  teraHost = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.TeraWallet.Host',
    value: ''
  });

  payterPreauthAmount = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Payter.PreAuthAmount',
    value: ''
  });

  readerBaurate = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.RfidReaderBaurate',
    value: ''
  });

  readerFrame = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Comport.RfidReaderFrame',
    value: ''
  });

  terminalType = new SystemConfigDto({
    name: 'RfidFridgeSetting.System.Payment.Magic.TerminalType',
    value: ''
  });


  // tslint:disable-next-line: use-life-cycle-interface
  ngOnInit() {
    this.paymentOptions = ['NAYAX', 'MAGIC', 'QR','QR_NAYAX','QR_MAGIC','PAYTER'];
    this.qrPaymentOptions = ['ALL', 'GRABPAY','WALLET','CREDITCARD_WALLET'];
    this.cardInsertOptions = ['DEFAULT','SLIM'];
    this.readerBaurateOptions = ['38400','115200'];
    this.readerFrameOptions = ['8E1','8N1'];
    this.terminalTypeOptions = ['IUC','IM30'];

    this.optionsHID = ['COM1', 'COM2', 'COM3', 'COM4', 'COM5', 'COM6', 'COM7', 'COM8', 'COM9', 'COM10',
      'COM11', 'COM12', 'COM13', 'COM14', 'COM15', 'COM16', 'COM17', 'COM18', 'COM19', 'COM20',
      'COM21', 'COM22', 'COM23', 'COM24', 'COM25', 'COM26', 'COM27', 'COM28', 'COM29', 'COM30','FRIDGELOCK', 'HID'];

      this.optionsQr = ['COM1', 'COM2', 'COM3', 'COM4', 'COM5', 'COM6', 'COM7', 'COM8', 'COM9', 'COM10',
      'COM11', 'COM12', 'COM13', 'COM14', 'COM15', 'COM16', 'COM17', 'COM18', 'COM19', 'COM20',
      'COM21', 'COM22', 'COM23', 'COM24', 'COM25', 'COM26', 'COM27', 'COM28', 'COM29', 'COM30','USB'];

    this.options = this.optionsHID.slice(0, -2);
    this.getConfigs();
  }

  updateSettingsValue(value: SystemConfigDto) {
    this.systemConfigService.update(value)
      .finally(() => { })
      .subscribe(() => {
        this.notify.info(this.l('Saved Successfully'));
      });
  }

  updateBooleanSettingsValue(inputValue: SystemConfigDto) {
    if (inputValue.value === 'true') {
      inputValue.value = 'false';
    } else {
      inputValue.value = 'true';
    }
    this.updateSettingsValue(inputValue);
  }

  getConfigs(): void {
    this.systemConfigService.getAll()
      .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
        this.useCloud = result.items.find(x => x.name === this.useCloud.name);
        this.cloudUrl = result.items.find(x => x.name === this.cloudUrl.name);
        this.tenantId = result.items.find(x => x.name === this.tenantId.name);

        this.machineId = result.items.find(x => x.name === this.machineId.name);
        this.machineName = result.items.find(x => x.name === this.machineName.name);
        this.paymentType = result.items.find(x => x.name === this.paymentType.name);
        this.lock = result.items.find(x => x.name === this.lock.name);
        this.inventory = result.items.find(x => x.name === this.inventory.name);
        this.cashlessTerminal = result.items.find(x => x.name === this.cashlessTerminal.name);
        this.enableTemperature = result.items.find(x => x.name === this.enableTemperature.name);
        this.temperatureComport = result.items.find(x => x.name === this.temperatureComport.name);
        let cloudAPIUrlSetting = result.items.find(x => x.name === 'RfidFridgeSetting.System.Cloud.CloudApiUrl');
        this.cloudAPIUrl = cloudAPIUrlSetting == null ? '' : cloudAPIUrlSetting.value;
        this.ezLinkMin = result.items.find(x => x.name === this.ezLinkMin.name);
        this.enableCreditCard = result.items.find(x => x.name === this.enableCreditCard.name);
        this.enableCpas = result.items.find(x => x.name === this.enableCpas.name);

        this.slackUrl = result.items.find(x => x.name === this.slackUrl.name);
        this.slackInfoChannel = result.items.find(x => x.name === this.slackInfoChannel.name);
        this.slackAlertChannel = result.items.find(x => x.name === this.slackAlertChannel.name);
        this.slackUsername = result.items.find(x => x.name === this.slackUsername.name);
        this.advsImage = result.items.find(x => x.name === this.advsImage.name);
        this.bottomText = result.items.find(x => x.name === this.bottomText.name);
        // Set value for Grabpay.
        this.host = result.items.find(x => x.name === this.host.name);
        this.partnerId = result.items.find(x => x.name === this.partnerId.name);
        this.partnerSecret = result.items.find(x => x.name === this.partnerSecret.name);
        this.clientId = result.items.find(x => x.name === this.clientId.name);
        this.clientSecret = result.items.find(x => x.name === this.clientSecret.name);
        this.grabId = result.items.find(x => x.name === this.grabId.name);
        this.terminalId = result.items.find(x => x.name === this.terminalId.name);
        this.qrPaymentType = result.items.find(x => x.name === this.qrPaymentType.name);
        this.grabPayReserveAmount = result.items.find(x => x.name === this.grabPayReserveAmount.name);
        this.unstableTagChannel = result.items.find(x => x.name === this.unstableTagChannel.name);
        this.teraHost = result.items.find(x => x.name === this.teraHost.name);
        this.teraReserveAmount = result.items.find(x => x.name === this.teraReserveAmount.name);
        this.cardInsertType = result.items.find(x => x.name === this.cardInsertType.name);
        this.cardInsertComport = result.items.find(x => x.name === this.cardInsertComport.name);
        this.payterPreauthAmount = result.items.find(x => x.name === this.payterPreauthAmount.name);
        this.readerFrame = result.items.find(x => x.name === this.readerFrame.name);
        this.readerBaurate = result.items.find(x => x.name === this.readerBaurate.name);
        this.terminalType = result.items.find(x => x.name === this.terminalType.name);
        this.qrCode = result.items.find(x => x.name === this.qrCode.name);
      });
  }
}
