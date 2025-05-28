import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { ToastrService } from 'ngx-toastr';
import { PhotoStats } from '../../_models/photoStats';

@Component({
  selector: 'app-photo-statistics',
  standalone: true,
  imports: [],
  templateUrl: './photo-statistics.component.html',
  styleUrl: './photo-statistics.component.css',
})
export class PhotoStatisticsComponent implements OnInit {
  private adminService = inject(AdminService);
  toastr = inject(ToastrService);
  photoStats: PhotoStats[] = [];
  usersWithNoMainPhoto: string[] = [];
  ngOnInit(): void {
    this.getPhotoApprovalStats();
    this.getUsersWithNoMainPhoto();
  }
  getUsersWithNoMainPhoto() {
    return this.adminService.getUsersWithNoMainPhoto().subscribe({
      next: response => {
        this.usersWithNoMainPhoto = response;
      },
    });
  }
  getPhotoApprovalStats() {
    return this.adminService.getPhotoApprovalStats().subscribe({
      next: response => {
        this.photoStats = response;
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }
}
