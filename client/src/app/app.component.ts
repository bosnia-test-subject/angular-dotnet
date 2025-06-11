import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from './nav/nav.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { AuthStoreService } from './_services/auth-store.service';
import { AsyncPipe, NgIf } from '@angular/common';
import { LoadingService } from './_services/loading.service';
import { LoadSpinnerComponent } from "./load-spinner/load-spinner.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavComponent, NgxSpinnerComponent, AsyncPipe, NgIf, LoadSpinnerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  http = inject(HttpClient);
  loadingService = inject(LoadingService);
  private authService = inject(AuthStoreService);
  // evade hardcoding
  adress: string = 'https://localhost:5001/api/users';
  title = 'Dating App';
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  users: any;

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user = JSON.parse(userString);
    this.authService.setCurrentUser(user);
  }

  ngOnInit(): void {
    setTimeout(() => this.setCurrentUser(), 0);
  }
}
