import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filter'
})

// Custom pipe for search text.
export class FilterPipe implements PipeTransform {

  transform(value: any, args?: any): any {
    if (!args[0]) {
      return value;
    } else if (value) {
      return value.filter(item => {
        for (let key in item) {
          if ((typeof item[key] === 'string' || item[key] instanceof String) &&
            (item[key].toLowerCase().includes(args))) {
            return true;
          }
        }
      });
    }
  }

}

@Pipe({
  name: 'filterMachineNamePipe'
})

export class FilterMachineNamePipe implements PipeTransform {
  transform(items: Array<any>, machineNameFilter: string): Array<any> {
      return items.filter(item => item.machineName.toLowerCase().includes(machineNameFilter.trim().toLowerCase()));
  }
}
