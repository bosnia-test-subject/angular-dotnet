import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { Tag } from '../../_models/tag';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';
@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
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
  private modalService = inject(BsModalService);
  tags: Tag[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> =
    new BsModalRef<RolesModalComponent>();

  ngOnInit(): void {
    this.initializeUploader();
    this.getUserPhotos();
  }
  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  // getUserPhotos() {
  //   this.memberService.getMember(this.member().userName).subscribe({
  //     next: member => {
  //       this.userPhotos = member.photos;
  //     },
  //   });
  // }
  getUserPhotos() {
    this.memberService.getPhotosWithTags().subscribe({
      next: photos => {
        this.userPhotos = photos;
      },
    });
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: () => {
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(
          x => x.id !== photo.id
        );
        this.memberChange.emit(updatedMember);
        this.getUserPhotos();
      },
    });
  }

  setMainPhoto(photo: Photo) {
    if (photo.isApproved) {
      this.memberService.setMainPhoto(photo).subscribe({
        next: () => {
          const user = this.accountService.currentUser();
          if (user) {
            (user.photoUrl = photo.url),
              this.accountService.setCurrentUser(user);
          }
          const updatedMember = { ...this.member() };
          updatedMember.photoUrl = photo.url;
          updatedMember.photos.forEach(p => {
            if (p.isMain) {
              p.isMain = false;
            }
            if (p.id === photo.id) {
              p.isMain = true;
            }
          });
          // this.memberChange.emit(updatedMember);
          this.getUserPhotos();
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
      const updatedMember = { ...this.member() };
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);
      if (photo.isMain) {
        const user = this.accountService.currentUser();
        if (user) {
          (user.photoUrl = photo.url), this.accountService.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) {
            p.isMain = false;
          }
          if (p.id === photo.id) {
            p.isMain = true;
          }
        });
        this.memberChange.emit(updatedMember);
      }
    };
  }

  // openRolesModal(user: User) {
  //   const initialState: ModalOptions = {
  //     class: 'modal-lg',
  //     initialState: {
  //       title: 'User roles',
  //       username: user.username,
  //       selectedRoles: [...user.roles],
  //       availableRoles: ['Admin', 'Moderator', 'Member'],
  //       users: this.users,
  //       rolesUpdated: false,
  //     },
  //   };
  //   this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
  //   this.bsModalRef.onHide?.subscribe({
  //     next: () => {
  //       if (this.bsModalRef.content && this.bsModalRef.content.rolesUpdated) {
  //         const selectedRoles = this.bsModalRef.content.selectedRoles;
  //         this.adminService
  //           .updateUserRoles(user.username, selectedRoles)
  //           .subscribe({
  //             next: roles => {
  //               user.roles = roles;
  //               this.getUsersWithRoles();
  //             },
  //           });
  //       }
  //     },
  //   });
  // }

  // getUsersWithRoles() {
  //   this.adminService.getUserWithRoles().subscribe({
  //     next: users => (this.users = users),
  //   });
  // }
}
