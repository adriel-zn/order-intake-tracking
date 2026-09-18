import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { IconComponent } from './shared/icon.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, FormsModule, DatePipe, IconComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  private readonly router = inject(Router);
  readonly navigationOpen = signal(true);
  readonly today = new Date();
  searchQuery = '';

  searchOrders(): void {
    void this.router.navigate(['/orders'], { queryParams: { q: this.searchQuery.trim() || null } });
  }
}
