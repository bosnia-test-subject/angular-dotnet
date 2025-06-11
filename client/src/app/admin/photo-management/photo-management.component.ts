import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { ToastrService } from 'ngx-toastr';
import { ButtonWrapperComponent } from '../../_forms/button-wrapper/button-wrapper.component';
import { Tag } from '../../_models/tag';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [ButtonWrapperComponent, FormsModule, CommonModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);

  private photosSubject = new BehaviorSubject<Photo[]>([]);
  private selectedTagSubject = new BehaviorSubject<string>('');
  public selectedTag$ = this.selectedTagSubject.asObservable();
  tags: Tag[] = [];
  newTag: Tag = {} as Tag;

  filteredPhotos$ = combineLatest([
    this.photosSubject.asObservable(),
    this.selectedTagSubject.asObservable(),
  ]).pipe(
    map(([photos, selectedTag]) => {
      if (!selectedTag) return photos;
      return photos.filter(photo => photo.tags?.includes(selectedTag));
    })
  );

  ngOnInit(): void {
    this.getApprovalPhotos();
    this.getTags();
  }

  onTagChange(tag: string) {
    this.selectedTagSubject.next(tag);
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
    this.adminService.getTags().subscribe({
      next: response => {
        this.tags = response;
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }

  removeTag(tagName: string) {
    this.adminService.removeTagByName(tagName).subscribe({
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

  approvePhoto(id: number) {
    this.adminService.approvePhoto(id).subscribe({
      next: () => {
        this.toastr.success('Photo succesfully approved');
        this.getApprovalPhotos();
      },
      error: error => console.log(error),
    });
  }

  rejectPhoto(id: number) {
    this.adminService.rejectPhoto(id).subscribe({
      next: () => {
        this.toastr.success('Photo succesfully rejected');
        this.getApprovalPhotos();
      },
      error: error => console.log(error),
    });
  }

  getApprovalPhotos() {
    this.adminService.getPhotosForApproval().subscribe({
      next: response => {
        this.photosSubject.next(response);
      },
      error: error =>
        this.toastr.error('Something unexpected happened: ' + error),
    });
  }
  trackByPhotoId(index: number, photo: Photo): number {
    return photo.id;
  }

  trackByTagId(index: number, tag: Tag): number {
    return tag.id;
  }
}
