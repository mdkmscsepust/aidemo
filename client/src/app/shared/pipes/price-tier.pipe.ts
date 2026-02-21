import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'priceTier', standalone: true })
export class PriceTierPipe implements PipeTransform {
  transform(tier: string | number): string {
    const map: Record<string, string> = {
      'Budget': '$', 'Moderate': '$$', 'Upscale': '$$$', 'Fine': '$$$$',
      '0': '$', '1': '$$', '2': '$$$', '3': '$$$$'
    };
    return map[String(tier)] ?? '$$';
  }
}
