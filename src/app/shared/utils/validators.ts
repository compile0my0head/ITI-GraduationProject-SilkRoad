// SilkRoad Form Validators
// Comprehensive validation utilities for all forms

export interface ValidationResult {
  valid: boolean;
  message: string;
}

export interface FieldValidation {
  touched: boolean;
  dirty: boolean;
  errors: string[];
}

// Email validation
export function validateEmail(email: string): ValidationResult {
  if (!email || email.trim() === '') {
    return { valid: false, message: 'Email is required' };
  }
  
  const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  if (!emailRegex.test(email.trim())) {
    return { valid: false, message: 'Please enter a valid email address' };
  }
  
  return { valid: true, message: '' };
}

// Password validation
export function validatePassword(password: string, minLength: number = 8): ValidationResult {
  if (!password || password === '') {
    return { valid: false, message: 'Password is required' };
  }
  
  if (password.length < minLength) {
    return { valid: false, message: `Password must be at least ${minLength} characters` };
  }
  
  // Check for at least one uppercase letter
  if (!/[A-Z]/.test(password)) {
    return { valid: false, message: 'Password must contain at least one uppercase letter' };
  }
  
  // Check for at least one lowercase letter
  if (!/[a-z]/.test(password)) {
    return { valid: false, message: 'Password must contain at least one lowercase letter' };
  }
  
  // Check for at least one number
  if (!/[0-9]/.test(password)) {
    return { valid: false, message: 'Password must contain at least one number' };
  }
  
  return { valid: true, message: '' };
}

// Password confirmation validation
export function validatePasswordMatch(password: string, confirmPassword: string): ValidationResult {
  if (!confirmPassword || confirmPassword === '') {
    return { valid: false, message: 'Please confirm your password' };
  }
  
  if (password !== confirmPassword) {
    return { valid: false, message: 'Passwords do not match' };
  }
  
  return { valid: true, message: '' };
}

// Required field validation
export function validateRequired(value: string | number | null | undefined, fieldName: string): ValidationResult {
  if (value === null || value === undefined || (typeof value === 'string' && value.trim() === '')) {
    return { valid: false, message: `${fieldName} is required` };
  }
  return { valid: true, message: '' };
}

// Min length validation
export function validateMinLength(value: string, minLength: number, fieldName: string): ValidationResult {
  if (!value || value.trim().length < minLength) {
    return { valid: false, message: `${fieldName} must be at least ${minLength} characters` };
  }
  return { valid: true, message: '' };
}

// Max length validation
export function validateMaxLength(value: string, maxLength: number, fieldName: string): ValidationResult {
  if (value && value.trim().length > maxLength) {
    return { valid: false, message: `${fieldName} must be no more than ${maxLength} characters` };
  }
  return { valid: true, message: '' };
}

// Number range validation
export function validateNumberRange(value: number | null, min: number, max: number, fieldName: string): ValidationResult {
  if (value === null || value === undefined) {
    return { valid: false, message: `${fieldName} is required` };
  }
  
  if (isNaN(value)) {
    return { valid: false, message: `${fieldName} must be a valid number` };
  }
  
  if (value < min) {
    return { valid: false, message: `${fieldName} must be at least ${min}` };
  }
  
  if (value > max) {
    return { valid: false, message: `${fieldName} must be no more than ${max}` };
  }
  
  return { valid: true, message: '' };
}

// Positive number validation
export function validatePositiveNumber(value: number | null, fieldName: string, allowZero: boolean = true): ValidationResult {
  if (value === null || value === undefined) {
    return { valid: false, message: `${fieldName} is required` };
  }
  
  if (isNaN(value)) {
    return { valid: false, message: `${fieldName} must be a valid number` };
  }
  
  if (allowZero ? value < 0 : value <= 0) {
    return { valid: false, message: `${fieldName} must be ${allowZero ? 'zero or' : ''} greater than zero` };
  }
  
  return { valid: true, message: '' };
}

// Phone number validation
export function validatePhone(phone: string): ValidationResult {
  if (!phone || phone.trim() === '') {
    return { valid: true, message: '' }; // Phone is usually optional
  }
  
  // Remove common formatting characters
  const cleaned = phone.replace(/[\s\-\(\)\.]/g, '');
  
  // Check if it's a valid phone number (10-15 digits, optionally starting with +)
  const phoneRegex = /^\+?[0-9]{10,15}$/;
  if (!phoneRegex.test(cleaned)) {
    return { valid: false, message: 'Please enter a valid phone number' };
  }
  
  return { valid: true, message: '' };
}

// URL validation
export function validateUrl(url: string): ValidationResult {
  if (!url || url.trim() === '') {
    return { valid: true, message: '' }; // URL is usually optional
  }
  
  try {
    new URL(url);
    return { valid: true, message: '' };
  } catch {
    return { valid: false, message: 'Please enter a valid URL' };
  }
}

// Date validation
export function validateDate(dateString: string, fieldName: string): ValidationResult {
  if (!dateString || dateString.trim() === '') {
    return { valid: false, message: `${fieldName} is required` };
  }
  
  const date = new Date(dateString);
  if (isNaN(date.getTime())) {
    return { valid: false, message: `Please enter a valid ${fieldName.toLowerCase()}` };
  }
  
  return { valid: true, message: '' };
}

// Future date validation
export function validateFutureDate(dateString: string, fieldName: string): ValidationResult {
  const baseValidation = validateDate(dateString, fieldName);
  if (!baseValidation.valid) {
    return baseValidation;
  }
  
  const date = new Date(dateString);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  if (date < today) {
    return { valid: false, message: `${fieldName} must be today or in the future` };
  }
  
  return { valid: true, message: '' };
}

// Date range validation
export function validateDateRange(startDate: string, endDate: string): ValidationResult {
  if (!startDate || !endDate) {
    return { valid: true, message: '' };
  }
  
  const start = new Date(startDate);
  const end = new Date(endDate);
  
  if (end < start) {
    return { valid: false, message: 'End date must be after start date' };
  }
  
  return { valid: true, message: '' };
}

// Name validation (for full names)
export function validateFullName(name: string): ValidationResult {
  if (!name || name.trim() === '') {
    return { valid: false, message: 'Full name is required' };
  }
  
  if (name.trim().length < 2) {
    return { valid: false, message: 'Full name must be at least 2 characters' };
  }
  
  if (name.trim().length > 100) {
    return { valid: false, message: 'Full name must be no more than 100 characters' };
  }
  
  // Check for at least two parts (first and last name) - optional strict mode
  // const parts = name.trim().split(/\s+/);
  // if (parts.length < 2) {
  //   return { valid: false, message: 'Please enter your first and last name' };
  // }
  
  return { valid: true, message: '' };
}

// Price validation
export function validatePrice(price: number | null): ValidationResult {
  if (price === null || price === undefined) {
    return { valid: false, message: 'Price is required' };
  }
  
  if (isNaN(price)) {
    return { valid: false, message: 'Please enter a valid price' };
  }
  
  if (price < 0) {
    return { valid: false, message: 'Price cannot be negative' };
  }
  
  // Check for reasonable maximum
  if (price > 999999999) {
    return { valid: false, message: 'Price is too high' };
  }
  
  return { valid: true, message: '' };
}

// Stock quantity validation
export function validateStockQuantity(quantity: number | null): ValidationResult {
  if (quantity === null || quantity === undefined) {
    return { valid: false, message: 'Stock quantity is required' };
  }
  
  if (isNaN(quantity)) {
    return { valid: false, message: 'Please enter a valid quantity' };
  }
  
  if (quantity < 0) {
    return { valid: false, message: 'Stock quantity cannot be negative' };
  }
  
  if (!Number.isInteger(quantity)) {
    return { valid: false, message: 'Stock quantity must be a whole number' };
  }
  
  return { valid: true, message: '' };
}

// Get password strength
export function getPasswordStrength(password: string): { strength: 'weak' | 'medium' | 'strong' | 'very-strong'; score: number } {
  let score = 0;
  
  if (!password) {
    return { strength: 'weak', score: 0 };
  }
  
  // Length checks
  if (password.length >= 8) score += 1;
  if (password.length >= 12) score += 1;
  if (password.length >= 16) score += 1;
  
  // Character variety checks
  if (/[a-z]/.test(password)) score += 1;
  if (/[A-Z]/.test(password)) score += 1;
  if (/[0-9]/.test(password)) score += 1;
  if (/[^a-zA-Z0-9]/.test(password)) score += 2;
  
  // Determine strength
  if (score <= 2) return { strength: 'weak', score };
  if (score <= 4) return { strength: 'medium', score };
  if (score <= 6) return { strength: 'strong', score };
  return { strength: 'very-strong', score };
}
