import { Component, inject, signal, computed, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/services/auth.service';
import { 
  validateEmail, 
  validatePassword, 
  validatePasswordMatch, 
  validateFullName,
  getPasswordStrength 
} from '../../../shared/utils/validators';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private authService = inject(AuthService);

  fullName = signal('');
  email = signal('');
  password = signal('');
  confirmPassword = signal('');

  // Track touched state for validation display
  fullNameTouched = signal(false);
  emailTouched = signal(false);
  passwordTouched = signal(false);
  confirmPasswordTouched = signal(false);
  
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  formSubmitAttempted = signal(false);
  acceptTerms = signal(false);

  isLoading = this.authService.isLoading;
  error = this.authService.error;

  // Computed validation states
  fullNameValidation = computed(() => {
    const value = this.fullName();
    if (!value && !this.fullNameTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateFullName(value);
  });

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
    return validatePassword(value, 8);
  });

  confirmPasswordValidation = computed(() => {
    const value = this.confirmPassword();
    if (!value && !this.confirmPasswordTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validatePasswordMatch(this.password(), value);
  });

  passwordStrength = computed(() => {
    return getPasswordStrength(this.password());
  });

  // Password requirements checks
  passwordHasLength = computed(() => this.password().length >= 8);
  passwordHasUppercase = computed(() => /[A-Z]/.test(this.password()));
  passwordHasLowercase = computed(() => /[a-z]/.test(this.password()));
  passwordHasNumber = computed(() => /[0-9]/.test(this.password()));
  passwordHasSpecial = computed(() => /[^a-zA-Z0-9]/.test(this.password()));

  isFormValid = computed(() => {
    return this.fullNameValidation().valid && 
           this.emailValidation().valid && 
           this.passwordValidation().valid && 
           this.confirmPasswordValidation().valid &&
           this.acceptTerms();
  });

  // Check if field should show error
  showFullNameError = computed(() => {
    return (this.fullNameTouched() || this.formSubmitAttempted()) && !this.fullNameValidation().valid;
  });

  showEmailError = computed(() => {
    return (this.emailTouched() || this.formSubmitAttempted()) && !this.emailValidation().valid;
  });

  showPasswordError = computed(() => {
    return (this.passwordTouched() || this.formSubmitAttempted()) && !this.passwordValidation().valid;
  });

  showConfirmPasswordError = computed(() => {
    return (this.confirmPasswordTouched() || this.formSubmitAttempted()) && !this.confirmPasswordValidation().valid;
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
    this.fullName.set('');
    this.email.set('');
    this.password.set('');
    this.confirmPassword.set('');
    this.fullNameTouched.set(false);
    this.emailTouched.set(false);
    this.passwordTouched.set(false);
    this.confirmPasswordTouched.set(false);
    this.showPassword.set(false);
    this.showConfirmPassword.set(false);
    this.formSubmitAttempted.set(false);
    this.acceptTerms.set(false);
  }

  updateFullName(value: string): void {
    this.fullName.set(value);
  }

  updateEmail(value: string): void {
    this.email.set(value);
  }

  updatePassword(value: string): void {
    this.password.set(value);
  }

  updateConfirmPassword(value: string): void {
    this.confirmPassword.set(value);
  }

  onFullNameBlur(): void {
    this.fullNameTouched.set(true);
  }

  onEmailBlur(): void {
    this.emailTouched.set(true);
  }

  onPasswordBlur(): void {
    this.passwordTouched.set(true);
  }

  onConfirmPasswordBlur(): void {
    this.confirmPasswordTouched.set(true);
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(v => !v);
  }

  toggleTerms(): void {
    this.acceptTerms.update(v => !v);
  }

  onSubmit(): void {
    this.formSubmitAttempted.set(true);
    
    if (!this.isFormValid()) {
      return;
    }

    this.authService.register({
      fullName: this.fullName(),
      email: this.email(),
      password: this.password(),
      confirmPassword: this.confirmPassword()
    });
  }

  clearError(): void {
    this.authService.clearError();
  }
}
