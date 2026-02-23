import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.error = '';
    if (!this.email.trim() || !this.password) {
      this.error = 'Preencha e-mail e senha.';
      return;
    }
    this.loading = true;
    this.auth.login({ email: this.email.trim(), password: this.password }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res) this.router.navigate(['/dashboard']);
        else this.error = 'E-mail ou senha inválidos.';
      },
      error: () => {
        this.loading = false;
        this.error = 'Erro ao conectar. Verifique se a API de usuários está rodando.';
      },
    });
  }
}
