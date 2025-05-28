import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [
    NgIf,
    NgFor,
    NgStyle,
    NgClass,
    FileUploadModule,
    DecimalPipe,
    FormsModule,
  ],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css',
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService);
  member = input.required<Member>();
  userPhotos: Photo[] = [];
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();
  editingPhotoId: number | null = null;
  selectedTagNames: string[] = [];
  availableTags: string[] = [];
  selectedTag: string = '';
  filteredPhotos: Photo[] = [];

  ngOnInit(): void {
    this.initializeUploader();
    this.loadAvailableTags();
    this.getUserPhotos();
  }
  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  filterPhotosByTag() {
    if (!this.selectedTag) {
      this.filteredPhotos = this.userPhotos;
    } else {
      this.filteredPhotos = this.userPhotos.filter(
        photo => photo.tags && photo.tags.includes(this.selectedTag)
      );
    }
  }

  loadAvailableTags() {
    this.memberService.getAllTags().subscribe({
      next: tags => (this.availableTags = tags),
      error: err => console.error('Failed to load tags', err),
    });
  }

  openTagEditor(photo: Photo) {
    if (this.editingPhotoId === photo.id) {
      this.editingPhotoId = null;
      this.selectedTagNames = [];
      return;
    }

    this.editingPhotoId = photo.id;
    this.selectedTagNames = photo.tags ? [...photo.tags] : [];

    if (!this.availableTags.length) {
      this.loadAvailableTags();
      console.log(this.availableTags);
    }
  }
  toggleTagEditor(photoId: number) {
    if (this.editingPhotoId === photoId) {
      this.editingPhotoId = null;
    } else {
      this.editingPhotoId = photoId;
    }
  }

  toggleTagSelection(tag: string) {
    const index = this.selectedTagNames.indexOf(tag);
    if (index > -1) {
      this.selectedTagNames.splice(index, 1);
    } else {
      this.selectedTagNames.push(tag);
    }
  }

  removeTagFromPhoto(photo: Photo, tag: string) {
    this.memberService.removeTagFromPhoto(photo.id, tag).subscribe({
      next: () => {
        this.toastr.success('Tag removed successfully');
        photo.tags = photo.tags?.filter(t => t !== tag) || [];
        const memberPhotoIndex = this.member().photos.findIndex(
          p => p.id === photo.id
        );
        if (memberPhotoIndex > -1) {
          this.member().photos[memberPhotoIndex].tags = [...photo.tags];
        }
        this.memberChange.emit({
          ...this.member(),
          photos: [...this.member().photos],
        });
      },
      error: err => {
        this.toastr.error('Failed to remove tag');
        console.error(err);
      },
    });
  }

  submitTagsForPhoto(photo: Photo) {
    this.memberService
      .addTagToPhoto(photo.id, this.selectedTagNames)
      .subscribe({
        next: () => {
          this.toastr.success('Tags updated successfully');
          const photoIndex = this.userPhotos.findIndex(p => p.id === photo.id);
          if (photoIndex > -1) {
            this.userPhotos[photoIndex].tags = [...this.selectedTagNames];
          }
          const memberPhotoIndex = this.member().photos.findIndex(
            p => p.id === photo.id
          );
          if (memberPhotoIndex > -1) {
            this.member().photos[memberPhotoIndex].tags = [
              ...this.selectedTagNames,
            ];
          }
          this.memberChange.emit({
            ...this.member(),
            photos: [...this.member().photos],
          });
          this.editingPhotoId = null;
          this.selectedTagNames = [];
        },
        error: err => {
          this.toastr.error('Failed to update tags');
          console.error(err);
        },
      });
  }

  getAllTags() {
    this.memberService.getAllTags().subscribe({
      next: tags => {
        this.availableTags = tags;
        console.log('Available tags:', this.availableTags);
        console.log(tags);
      },
      error: err => {
        console.error('Failed to load tags', err);
      },
    });
  }
  getUserPhotos() {
    this.memberService.getPhotosWithTags().subscribe({
      next: photos => {
        this.userPhotos = photos;
        this.filterPhotosByTag();
      },
    });
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: () => {
        this.userPhotos = this.userPhotos.filter(x => x.id !== photo.id);
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(
          x => x.id !== photo.id
        );
        this.memberChange.emit(updatedMember);
      },
    });
  }

  setMainPhoto(photo: Photo) {
    if (photo.isApproved) {
      this.memberService.setMainPhoto(photo).subscribe({
        next: () => {
          const user = this.accountService.currentUser();
          if (user) {
            user.photoUrl = photo.url;
            this.accountService.setCurrentUser(user);
          }
          this.userPhotos = this.userPhotos.map(p => ({
            ...p,
            isMain: p.id === photo.id,
          }));
          const updatedMember = { ...this.member() };
          updatedMember.photoUrl = photo.url;
          updatedMember.photos = updatedMember.photos.map(p => ({
            ...p,
            isMain: p.id === photo.id,
          }));
          this.memberChange.emit(updatedMember);
        },
      });
    } else {
      this.toastr.error('You cannot set unapproved photos as MAIN!');
    }
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = file => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (items, response) => {
      const photo = JSON.parse(response);
      this.userPhotos.push(photo);
      const updatedMember = { ...this.member() };
      updatedMember.photos = [...updatedMember.photos, photo];
      this.memberChange.emit(updatedMember);
      if (photo.isMain) {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos = updatedMember.photos.map(p => ({
          ...p,
          isMain: p.id === photo.id,
        }));
        this.memberChange.emit(updatedMember);
      }
    };
  }
  trackPhoto(index: number, photo: Photo) {
    return photo.id;
  }
}
