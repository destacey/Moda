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

// Resolves once MSAL has fully initialized and processed any pending redirect.
// Axios interceptors must await this before calling acquireTokenSilent to avoid
// block_iframe_reload errors during the startup window.
// Both initialize() and handleRedirectPromise() return cached promises on
// subsequent calls, so this is safe alongside MsalProvider's own initialization.
export const msalReady: Promise<void> =
  msalInstance
    ? msalInstance
        .initialize()
        .then(() => msalInstance.handleRedirectPromise())
        .then(() => {})
    : Promise.resolve()
