import { Component, inject, OnInit } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
import { HttpClient } from '@angular/common/http';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
 registerMode = false;
 http = inject(HttpClient);
 adress = "https://localhost:5001/api/"
 users: any;
 accountService = inject(AccountService);
 registerToggle() 
 {
  this.registerMode = !this.registerMode;
 }

 cancelRegisterMode(event: boolean) 
 {
  this.registerMode = event;
 }

 ngOnInit(): void {
   this.getUsers();
 }
  getUsers() 
  {
    this.http.get(this.adress + "users").subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log("Request has completed"),
    });
  }
}
