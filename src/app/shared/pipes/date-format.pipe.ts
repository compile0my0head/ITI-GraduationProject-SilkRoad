import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateFormat',
  standalone: true
})
export class DateFormatPipe implements PipeTransform {
  transform(value: Date | string, format: string = 'short'): string {
    // TODO: Implement date formatting logic
    return new Date(value).toLocaleString();
  }
}
