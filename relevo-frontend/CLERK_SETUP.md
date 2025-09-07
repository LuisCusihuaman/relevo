# Clerk Authentication Setup

## Environment Variables

Create a `.env` file in the root of the frontend project with the following variables:

```bash
# Clerk Authentication - Get this from your Clerk dashboard
VITE_CLERK_PUBLISHABLE_KEY=pk_test_your_publishable_key_here

# API Configuration - Points to your backend API
VITE_API_URL=https://localhost:57679
```

## Clerk Dashboard Setup

1. Go to [Clerk Dashboard](https://dashboard.clerk.dev/)
2. Create a new application or use an existing one
3. Copy the **Publishable Key** from the API Keys section
4. Add it to your `.env` file as `VITE_CLERK_PUBLISHABLE_KEY`

## Backend Integration

The frontend is now configured to work with the Clerk authentication system implemented in the backend:

- **Token Format**: Uses `x-clerk-user-token` header (primary) and `Authorization: Bearer <token>` (fallback)
- **Authentication Flow**: Clerk handles user authentication, frontend gets tokens, backend validates them
- **User Context**: Backend extracts user information from JWT claims
- **Protected Routes**: Frontend routes are protected using Clerk's `useAuth` hook

## Features Implemented

### ✅ Authentication
- Clerk sign-in/sign-up components
- JWT token management
- Protected routes
- Loading states during authentication

### ✅ API Integration
- Automatic token injection in API calls
- Support for both header formats
- Error handling for authentication failures
- Token refresh handling

### ✅ User Experience
- Seamless authentication flow
- Redirect to login for unauthenticated users
- Loading states during token retrieval
- Proper error messages

## Testing

To test the authentication:

1. Start the backend API server
2. Start the frontend development server
3. Navigate to protected routes - should redirect to login
4. Sign in with Clerk - should access protected content
5. API calls should include authentication headers

## Security Notes

- Never commit `.env` files to version control
- Use different Clerk applications for development/staging/production
- Regularly rotate API keys
- Monitor authentication logs for suspicious activity
