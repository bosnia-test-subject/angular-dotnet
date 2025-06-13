import {
  Directive,
  inject,
  Input,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewContainerRef,
} from '@angular/core';
import { AuthStoreService } from '../_services/auth-store.service';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[appHasRole]',
  standalone: true,
})
export class HasRoleDirective implements OnInit, OnDestroy {
  @Input() appHasRole: string[] = [];

  private authStore = inject(AuthStoreService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<any>);
  private subscription: Subscription | null = null;

  ngOnInit(): void {
    this.subscription = this.authStore.currentUser$.subscribe(user => {
      if (!user || !user.token) {
        this.viewContainerRef.clear();
        return;
      }

      const roles = this.extractRolesFromToken(user.token);
      const hasRole = this.appHasRole.some(role => roles.includes(role));

      if (hasRole) {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        this.viewContainerRef.clear();
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private extractRolesFromToken(token: string): string[] {
    try {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      const role = decodedToken.role;
      return Array.isArray(role) ? role : [role];
    } catch {
      return [];
    }
  }
}
