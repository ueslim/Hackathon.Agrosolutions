import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  error = '';
  loading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.error = '';
    if (!this.name.trim()) {
      this.error = 'Informe o nome.';
      return;
    }
    if (!this.email.trim()) {
      this.error = 'Informe o e-mail.';
      return;
    }
    if (!this.password) {
      this.error = 'Informe a senha.';
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.error = 'As senhas não coincidem.';
      return;
    }
    if (this.password.length < 4) {
      this.error = 'A senha deve ter pelo menos 4 caracteres.';
      return;
    }

    this.loading = true;
    this.auth
      .register({
        name: this.name.trim(),
        email: this.email.trim().toLowerCase(),
        password: this.password,
      })
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res) this.router.navigate(['/login']);
          else this.error = 'E-mail já cadastrado ou erro no servidor.';
        },
        error: (err) => {
          this.loading = false;
          const msg = err?.error?.message ?? err?.error ?? err?.statusText;
          this.error = msg || 'Erro ao cadastrar. Verifique se a API está rodando.';
        },
      });
  }
}
