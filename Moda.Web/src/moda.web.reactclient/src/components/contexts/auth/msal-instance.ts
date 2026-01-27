// msalInstance.ts
import { msalConfig } from '@/auth-config'
import {
  PublicClientApplication,
  IPublicClientApplication,
} from '@azure/msal-browser'

// Only create MSAL instance on client-side to avoid SSR "window is not defined" errors
export const msalInstance: IPublicClientApplication | null =
  typeof window !== 'undefined'
    ? new PublicClientApplication(msalConfig)
    : null
