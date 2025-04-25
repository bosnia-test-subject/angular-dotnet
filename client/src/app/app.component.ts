import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  http = inject(HttpClient);
  // evade hardcoding
  adress: string = 'https://localhost:5001/api/users'
  title = 'Dating App';
  users: any;

  displayStudents() {
    
  }

  ngOnInit(): void {
    // hardcoding! nije preporucljivo
    this.http.get(this.adress).subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log("Request has completed"),
  });
  }
}
