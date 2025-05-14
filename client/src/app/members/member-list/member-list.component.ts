import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from '../member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { InputWrapperComponent } from '../../_forms/input-wrapper/input-wrapper.component';
import { ButtonWrapperComponent } from '../../_forms/button-wrapper/button-wrapper.component';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [
    MemberCardComponent,
    PaginationModule,
    FormsModule,
    ButtonsModule,
    InputWrapperComponent,
    ButtonWrapperComponent,
  ],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css',
})
export class MemberListComponent implements OnInit {
  private memberService = inject(MembersService);
  currentPage = 1;
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'females' },
  ];

  ngOnInit(): void {
    if (!this.memberService.paginatedResult()) this.loadMembers();
  }

  getPaginatedResult() {
    return this.memberService.paginatedResult();
  }
  getUserParams() {
    return this.memberService.userParams();
  }

  resetFilters() {
    this.memberService.resetUserParams();
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers();
  }

  pageChanged(event: any) {
    if (this.currentPage != event.page) {
      this.currentPage = event.page;
      this.getUserParams().pageNumber = this.currentPage;
      this.loadMembers();
    }
  }
}
