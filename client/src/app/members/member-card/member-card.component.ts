import { Component, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  accountService = inject(AccountService);
  member = input.required<Member>();
}
