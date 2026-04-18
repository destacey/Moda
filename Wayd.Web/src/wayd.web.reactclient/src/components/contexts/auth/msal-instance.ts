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
// Lazy: only starts initialization on first await, so merely importing this
// module in tests won't trigger crypto.subtle calls that jsdom doesn't support.
let _msalReady: Promise<void> | null = null

export function getMsalReady(): Promise<void> {
  if (!msalInstance) return Promise.resolve()
  if (!_msalReady) {
    _msalReady = msalInstance
      .initialize()
      .then(() => msalInstance.handleRedirectPromise())
      .then(() => {})
  }
  return _msalReady
}
