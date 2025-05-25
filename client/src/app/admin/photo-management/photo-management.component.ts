import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { ToastrService } from 'ngx-toastr';
import { ButtonWrapperComponent } from '../../_forms/button-wrapper/button-wrapper.component';
import { Tag } from '../../_models/tag';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [ButtonWrapperComponent, FormsModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  toastr = inject(ToastrService);
  photos: Photo[] = [];
  tags: Tag[] = [];
  newTag: Tag = {} as Tag;
  ngOnInit(): void {
    this.getApprovalPhotos();
    this.getTags();
  }
  createTag(form: NgForm) {
    if (form.invalid) return;
    this.adminService.addTag(this.newTag).subscribe({
      next: () => {
        this.newTag.name = '';
        this.toastr.success('Tag successfully created');
        this.getTags();
        form.resetForm();
      },
      error: err => {
        this.toastr.error('Something unexpected happened: ' + err);
      },
    });
  }
  getTags() {
    return this.adminService.getTags().subscribe({
      next: response => {
        this.tags = response;
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }
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
  rejectPhoto(id: number) {
    return this.adminService.rejectPhoto(id).subscribe({
      next: () => {
        this.toastr.success('Photo succesfully rejected');
        this.getApprovalPhotos();
      },
      error: error => console.log(error),
    });
  }
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
