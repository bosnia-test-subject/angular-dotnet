import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { ToastrService } from 'ngx-toastr';
import { ButtonWrapperComponent } from '../../_forms/button-wrapper/button-wrapper.component';
import { Tag } from '../../_models/tag';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [ButtonWrapperComponent, FormsModule, CommonModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  toastr = inject(ToastrService);
  photos: Photo[] = [];
  tags: Tag[] = [];
  newTag: Tag = {} as Tag;
  selectedTag: string = '';
  filteredPhotos: any[] = [];
  ngOnInit(): void {
    this.getApprovalPhotos();
    this.getTags();
    this.filteredPhotos = this.photos;
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
        if (err.status === 400) {
          this.toastr.error("You can't create duplicates!");
        } else {
          this.toastr.error('Something unexpected happened: ' + err);
        }
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
  removeTag(tagName: string) {
    return this.adminService.removeTagByName(tagName).subscribe({
      next: () => {
        this.toastr.success('Tag successfully removed');
        this.getTags();
        this.getApprovalPhotos();
      },
      error: error => {
        if (error.status === 400) {
          this.toastr.error("You can't remove a tag that is in use!");
        } else {
          this.toastr.error('Something unexpected happened: ' + error);
        }
      },
    });
  }
  filterPhotosByTag() {
    if (!this.selectedTag) {
      this.filteredPhotos = this.photos;
    } else {
      this.filteredPhotos = this.photos.filter(
        photo => photo.tags && photo.tags.includes(this.selectedTag)
      );
    }
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
        this.filterPhotosByTag();
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }
}
