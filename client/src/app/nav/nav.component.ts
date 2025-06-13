import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';

import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ButtonWrapperComponent } from '../_forms/button-wrapper/button-wrapper.component';
import { AuthStoreService } from '../_services/auth-store.service';
import { CommonModule } from '@angular/common';
import { map, Observable } from 'rxjs';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [
    FormsModule,
    BsDropdownModule,
    RouterLink,
    RouterLinkActive,
    ButtonWrapperComponent,
    CommonModule,
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
})
export class NavComponent {
  private router = inject(Router);
  accountService = inject(AccountService);
  authStore = inject(AuthStoreService);
  private toastr = inject(ToastrService);

  user$: Observable<User | null>;
  isLoggedIn$ = this.authStore.isLoggedIn$;
  isAdmin$ = this.authStore.roles$.pipe(map(roles => roles.includes('Admin')));

  model: any = {};

  constructor() {
    this.user$ = this.authStore.currentUser$;
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
      },
      error: error => this.toastr.error(error.error),
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
