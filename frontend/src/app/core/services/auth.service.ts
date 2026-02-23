import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { LoginRequest, LoginResponse, UserResponse, CreateUserRequest } from '../models/user.model';

const STORAGE_TOKEN = 'agro_token';
const STORAGE_USER_ID = 'agro_user_id';
const STORAGE_USER = 'agro_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usersUrl = `${environment.api.users}/api/users`;

  private token = signal<string | null>(this.getStoredToken());
  private userId = signal<string | null>(this.getStoredUserId());
  private user = signal<{ name: string; email: string } | null>(this.getStoredUser());

  isLoggedIn = computed(() => !!this.token() && !!this.userId());
  currentUser = computed(() => this.user());

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  register(body: CreateUserRequest): Observable<UserResponse | null> {
    return this.http.post<UserResponse>(this.usersUrl, body).pipe(catchError(() => of(null)));
  }

  login(credentials: LoginRequest): Observable<LoginResponse | null> {
    return this.http.post<LoginResponse>(`${this.usersUrl}/login`, {
      email: credentials.email,
      password: credentials.password,
    }).pipe(
      tap((res) => {
        if (res) {
          this.token.set(res.token);
          this.userId.set(res.userId);
          this.user.set({ name: res.name, email: res.email });
          localStorage.setItem(STORAGE_TOKEN, res.token);
          localStorage.setItem(STORAGE_USER_ID, res.userId);
          localStorage.setItem(STORAGE_USER, JSON.stringify({ name: res.name, email: res.email }));
        }
      }),
      catchError(() => of(null)),
    );
  }

  logout(): void {
    this.token.set(null);
    this.userId.set(null);
    this.user.set(null);
    localStorage.removeItem(STORAGE_TOKEN);
    localStorage.removeItem(STORAGE_USER_ID);
    localStorage.removeItem(STORAGE_USER);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.token();
  }

  getUserId(): string | null {
    return this.userId();
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(STORAGE_TOKEN);
  }

  private getStoredUserId(): string | null {
    return localStorage.getItem(STORAGE_USER_ID);
  }

  private getStoredUser(): { name: string; email: string } | null {
    try {
      const raw = localStorage.getItem(STORAGE_USER);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
