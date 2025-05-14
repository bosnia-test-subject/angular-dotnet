import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { ToastrService } from 'ngx-toastr';
import { ButtonWrapperComponent } from "../../_forms/button-wrapper/button-wrapper.component";

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [ButtonWrapperComponent],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  toastr = inject(ToastrService);
  photos: Photo[] = [];

  ngOnInit(): void {
    this.getApprovalPhotos();
  }
  // task 19
  approvePhoto(id: number) {
    return this.adminService.approvePhoto(id).subscribe({
      next: () => {
        this.toastr.success('Photo succesfully approved');
        this.getApprovalPhotos();
        console.log('Photo approved');
      },
      error: error => console.log(error),
    });
  }
  // task 19
  rejectPhoto(id: number) {
    return this.adminService.rejectPhoto(id).subscribe({
      next: () => {
        this.toastr.success('Photo succesfully rejected');
        this.getApprovalPhotos();
      },
      error: error => console.log(error),
    });
  }
  // task 19
  getApprovalPhotos() {
    return this.adminService.getPhotosForApproval().subscribe({
      next: response => {
        this.photos = response;
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }
}
