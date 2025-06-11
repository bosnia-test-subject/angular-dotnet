import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import {
  DecimalPipe,
  NgClass,
  NgFor,
  NgIf,
  NgStyle,
  AsyncPipe,
} from '@angular/common'; // Import AsyncPipe
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
import { AuthStoreService } from '../../_services/auth-store.service';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs'; // Import RxJS

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
    AsyncPipe, // Add AsyncPipe to imports
  ],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css',
})
export class PhotoEditorComponent implements OnInit {
  private authService = inject(AuthStoreService);
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService);

  member = input.required<Member>();
  memberChange = output<Member>();

  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;

  editingPhotoId: number | null = null;
  selectedTagNames: string[] = [];
  availableTags: string[] = [];

  private userPhotosSubject = new BehaviorSubject<Photo[]>([]);
  userPhotos$: Observable<Photo[]> = this.userPhotosSubject.asObservable();

  private selectedFilterTagSubject = new BehaviorSubject<string>('');
  selectedFilterTag$: Observable<string> =
    this.selectedFilterTagSubject.asObservable();

  private showApprovedOnlySubject = new BehaviorSubject<boolean>(false);
  showApprovedOnly$: Observable<boolean> =
    this.showApprovedOnlySubject.asObservable();

  filteredPhotos$: Observable<Photo[]>;

  constructor() {
    this.filteredPhotos$ = combineLatest([
      this.userPhotos$,
      this.selectedFilterTag$,
      this.showApprovedOnly$,
    ]).pipe(
      map(([photos, selectedTag, showApproved]) => {
        return photos.filter(photo => {
          const matchesTag = selectedTag
            ? photo.tags && photo.tags.includes(selectedTag)
            : true;
          const matchesApproval = showApproved ? photo.isApproved : true;
          return matchesTag && matchesApproval;
        });
      })
    );
  }

  ngOnInit(): void {
    this.initializeUploader();
    this.loadAvailableTags();
    this.getUserPhotos();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  onSelectedTagChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.selectedFilterTagSubject.next(target.value);
  }

  onShowApprovedOnlyChange(event: Event) {
    const target = event.target as HTMLInputElement;
    this.showApprovedOnlySubject.next(target.checked);
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

        const currentPhotos = this.userPhotosSubject.getValue();
        const updatedPhotos = currentPhotos.map(p =>
          p.id === photo.id
            ? { ...p, tags: p.tags?.filter(t => t !== tag) || [] }
            : p
        );
        this.userPhotosSubject.next(updatedPhotos);

        const memberPhotoIndex = this.member().photos.findIndex(
          p => p.id === photo.id
        );
        if (memberPhotoIndex > -1) {
          this.member().photos[memberPhotoIndex].tags =
            photo.tags?.filter(t => t !== tag) || [];
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

          const currentPhotos = this.userPhotosSubject.getValue();
          const updatedPhotos = currentPhotos.map(p =>
            p.id === photo.id ? { ...p, tags: [...this.selectedTagNames] } : p
          );
          this.userPhotosSubject.next(updatedPhotos);

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
      },
      error: err => {
        console.error('Failed to load tags', err);
      },
    });
  }

  getUserPhotos() {
    this.memberService.getPhotosWithTags().subscribe({
      next: photos => {
        this.userPhotosSubject.next(photos);
      },
    });
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: () => {
        const updatedPhotos = this.userPhotosSubject
          .getValue()
          .filter(x => x.id !== photo.id);
        this.userPhotosSubject.next(updatedPhotos);

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
          const user = this.authService.getCurrentUserSnapshot();
          if (user) {
            user.photoUrl = photo.url;
            this.authService.setCurrentUser(user);
          }

          const updatedPhotos = this.userPhotosSubject.getValue().map(p => ({
            ...p,
            isMain: p.id === photo.id,
          }));
          this.userPhotosSubject.next(updatedPhotos);

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
      authToken: 'Bearer ' + this.authService.getCurrentUserSnapshot()?.token,
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
      const currentPhotos = this.userPhotosSubject.getValue();
      this.userPhotosSubject.next([...currentPhotos, photo]);

      const updatedMember = { ...this.member() };
      updatedMember.photos = [...updatedMember.photos, photo];
      this.memberChange.emit(updatedMember);

      if (photo.isMain) {
        const user = this.authService.getCurrentUserSnapshot();
        if (user) {
          user.photoUrl = photo.url;
          this.authService.setCurrentUser(user);
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
