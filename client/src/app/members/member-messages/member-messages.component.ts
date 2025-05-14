/* eslint-disable @angular-eslint/no-empty-lifecycle-method */
import {
  AfterViewChecked,
  Component,
  inject,
  input,
  ViewChild,
} from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';
import { TextInputComponent } from '../../_forms/text-input/text-input.component';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule, TextInputComponent],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css',
})
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer?: any;
  private messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = '';

  sendMessage() {
    this.messageService
      .sendMessage(this.username(), this.messageContent)
      ?.then(() => {
        this.messageForm?.reset();
        this.scrollToBottom();
      });
  }

  getMessageThread() {
    return this.messageService.messageThread();
  }

  ngAfterViewChecked(): void {}
  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop =
        this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
