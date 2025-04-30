import { Component, inject, input, OnInit, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  private acountService = inject(AccountService);
  private toastr = inject(ToastrService);
  cancelRegister = output<boolean>();
  model: any = {}

  registerForm: FormGroup = new FormGroup({});

  initializeForm() 
  {
    this.registerForm = new FormGroup(
      {
        username: new FormControl(),
        password: new FormControl(),
        confirmPassword: new FormControl(),
      })
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  register() {
    console.log(this.registerForm.value);
    this.acountService.register(this.model).subscribe(
      {
        next: response => {
          console.log(response);
          this.cancel();
        },
        error: error => this.toastr.error(error.error),
      })
    console.log(this.model)
  }
  cancel() 
  {
    this.cancelRegister.emit(false);
  }
}
