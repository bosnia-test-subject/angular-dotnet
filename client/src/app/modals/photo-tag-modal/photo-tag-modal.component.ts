import { Component, inject } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
@Component({
  selector: 'app-photo-tag-modal',
  standalone: true,
  imports: [],
  templateUrl: './photo-tag-modal.component.html',
  styleUrl: './photo-tag-modal.component.css',
})
export class PhotoTagModalComponent {
  bsModalRef = inject(BsModalRef);
  username = '';
  title = '';
  availableTags: string[] = [];
  selectedTags: string[] = [];
  rolesUpdated = false;

  updateChecked(checkedValue: string) {
    if (this.selectedTags.includes(checkedValue)) {
      this.selectedTags = this.selectedTags.filter(r => r !== checkedValue);
    } else {
      this.selectedTags.push(checkedValue);
    }
  }

  onSelectRoles() {
    this.rolesUpdated = true;
    this.bsModalRef.hide();
  }
}
