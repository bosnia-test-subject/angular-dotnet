import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';

@Component({
  selector: 'app-button-wrapper',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonsModule],
  templateUrl: './button-wrapper.component.html',
  styleUrl: './button-wrapper.component.css',
})
export class ButtonWrapperComponent {
  @Input() text: string = '';
  @Input() disabled: boolean = false;
  @Input() type: string = 'button';
  @Input() classStyle: string = 'btn btn-primary';
  @Input() btnRadio: string = '';
  @Input() name: string = '';
  @Input() model: any;
  @Output() modelChange = new EventEmitter<any>();
  @Output() clicked = new EventEmitter<void>();

  onClick() {
    this.clicked.emit();
  }

  onChange(value: any) {
    this.modelChange.emit(value);
  }
}
