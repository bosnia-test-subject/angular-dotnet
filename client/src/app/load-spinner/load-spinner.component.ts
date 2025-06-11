import { Component, inject } from '@angular/core';
import { LoadingService } from '../_services/loading.service';
import { AsyncPipe, CommonModule } from '@angular/common';

@Component({
  selector: 'app-load-spinner',
  standalone: true,
  imports: [AsyncPipe, CommonModule],
  templateUrl: './load-spinner.component.html',
  styleUrl: './load-spinner.component.css',
})
export class LoadSpinnerComponent {
  loadingService = inject(LoadingService);
}
