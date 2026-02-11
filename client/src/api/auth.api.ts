import apiClient from './client';
import type {
  SigninRequest,
  SigninResponse,
  SignupRequest,
  UserOutput,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  VerifyEmailRequest,
} from '../types';

export const authApi = {
  /**
   * Sign in user with email and password
   * Server returns SessionId cookie automatically
   */
  signIn: async (data: SigninRequest): Promise<SigninResponse> => {
    const response = await apiClient.post<SigninResponse>('/identity/sign-in', data);
    return response.data;
  },

  /**
   * Sign up new user (public registration)
   */
  signUp: async (data: SignupRequest): Promise<UserOutput> => {
    const response = await apiClient.post<UserOutput>('/identity/sign-up', data);
    return response.data;
  },

  /**
   * Verify email with token and set password
   */
  verifyEmail: async (data: VerifyEmailRequest): Promise<UserOutput> => {
    const response = await apiClient.post<UserOutput>('/identity/verify-email', data);
    return response.data;
  },

  /**
   * Resend verification code to email
   */
  resendVerificationCode: async (email: string): Promise<string> => {
    const response = await apiClient.post<string>('/identity/resend-verification-code', null, {
      params: { email },
    });
    return response.data;
  },

  /**
   * Request password reset email
   */
  forgotPassword: async (data: ForgotPasswordRequest): Promise<string> => {
    const response = await apiClient.post<string>('/identity/forgot-password', data);
    return response.data;
  },

  /**
   * Reset password with token
   */
  resetPassword: async (data: ResetPasswordRequest): Promise<string> => {
    const response = await apiClient.post<string>('/identity/reset-password', data);
    return response.data;
  },

  /**
   * Refresh authentication token
   * Uses SessionId cookie automatically
   */
  refreshToken: async (): Promise<SigninResponse> => {
    const response = await apiClient.post<SigninResponse>('/identity/refresh-token');
    return response.data;
  },

  /**
   * Logout user and invalidate session
   */
  logout: async (): Promise<void> => {
    await apiClient.post('/identity/logout');
  },
};

export default authApi;
