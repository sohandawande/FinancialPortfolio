import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'auth-access-denied',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.css'
})
export class AccessDenied {

}