// msalInstance.ts
import { msalConfig } from '@/auth-config'
import { PublicClientApplication } from '@azure/msal-browser'

export const msalInstance = new PublicClientApplication(msalConfig)
