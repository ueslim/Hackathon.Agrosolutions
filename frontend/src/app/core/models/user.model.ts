export interface LoginRequest {
  email: string;
  password: string;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  name: string;
  email: string;
  token: string;
}

export interface UserResponse {
  id: string;
  name: string;
  email: string;
  createdAtUtc: string;
}
