import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-icon',
  standalone: true,
  template: `<svg
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.6"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <path [attr.d]="paths[name] || paths['orders']" />
  </svg>`,
  styles: [
    ':host{display:inline-flex;width:18px;height:18px;flex-shrink:0}svg{width:100%;height:100%}',
  ],
})
export class IconComponent {
  @Input() name = 'orders';
  readonly paths: Record<string, string> = {
    dashboard: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z',
    orders: 'M6 3h12v18H6z M9 7h6 M9 11h6 M9 15h4',
    plus: 'M12 5v14 M5 12h14',
    search: 'M10.5 17a6.5 6.5 0 1 0 0-13 6.5 6.5 0 0 0 0 13 M16 16l5 5',
    menu: 'M4 6h16 M4 12h12 M4 18h8',
    box: 'M3 7l9-4 9 4v10l-9 4-9-4z M3 7l9 4 9-4 M12 11v10 M7.5 5l9 4',
    clock: 'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20 M12 6v6l4 2',
    check: 'M5 12l4 4L19 6',
    arrow: 'M5 12h14 M14 7l5 5-5 5',
    calendar: 'M4 5h16v16H4z M8 3v4 M16 3v4 M4 10h16',
    home: 'M3 10l9-7 9 7 M5 9v12h14V9 M9 21v-7h6v7',
  };
}
