import { Component, inject, signal, computed, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/services/auth.service';
import { validateEmail, validateRequired } from '../../../shared/utils/validators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);

  email = signal('');
  password = signal('');
  
  // Track touched state for validation display
  emailTouched = signal(false);
  passwordTouched = signal(false);
  showPassword = signal(false);
  formSubmitAttempted = signal(false);

  // Expose auth service signals
  isLoading = this.authService.isLoading;
  error = this.authService.error;

  // Computed validation states
  emailValidation = computed(() => {
    const value = this.email();
    if (!value && !this.emailTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateEmail(value);
  });

  passwordValidation = computed(() => {
    const value = this.password();
    if (!value && !this.passwordTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateRequired(value, 'Password');
  });

  isFormValid = computed(() => {
    return this.emailValidation().valid && this.passwordValidation().valid;
  });

  // Check if field should show error
  showEmailError = computed(() => {
    return (this.emailTouched() || this.formSubmitAttempted()) && !this.emailValidation().valid;
  });

  showPasswordError = computed(() => {
    return (this.passwordTouched() || this.formSubmitAttempted()) && !this.passwordValidation().valid;
  });

  constructor() {
    // Reset form when authentication succeeds
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.resetForm();
      }
    });
  }

  ngOnInit(): void {
    // Clear any persisted errors from other auth pages
    this.authService.clearError();
    // Ensure form starts in pristine state
    this.resetForm();
  }

  private resetForm(): void {
    this.email.set('');
    this.password.set('');
    this.emailTouched.set(false);
    this.passwordTouched.set(false);
    this.formSubmitAttempted.set(false);
    this.showPassword.set(false);
  }

  updateEmail(value: string): void {
    this.email.set(value);
  }

  updatePassword(value: string): void {
    this.password.set(value);
  }

  onEmailBlur(): void {
    this.emailTouched.set(true);
  }

  onPasswordBlur(): void {
    this.passwordTouched.set(true);
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  onSubmit(): void {
    this.formSubmitAttempted.set(true);
    
    if (!this.isFormValid()) {
      return;
    }
    
    this.authService.login({
      email: this.email(),
      password: this.password()
    });
  }

  clearError(): void {
    this.authService.clearError();
  }
}
