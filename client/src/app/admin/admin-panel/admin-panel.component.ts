import { Component } from '@angular/core';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { UserManagementComponent } from '../user-management/user-management.component';
import { HasRoleDirective } from '../../_directives/has-role.directive';
import { PhotoManagementComponent } from '../photo-management/photo-management.component';
import { PhotoStatisticsComponent } from '../photo-statistics/photo-statistics.component';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [
    TabsModule,
    UserManagementComponent,
    HasRoleDirective,
    PhotoManagementComponent,
    PhotoStatisticsComponent,
  ],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
})
export class AdminPanelComponent {}
