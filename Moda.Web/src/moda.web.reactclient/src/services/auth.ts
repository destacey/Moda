import { msalConfig, tokenRequest } from '@/authConfig'
import {
  AuthenticationResult,
  InteractionRequiredAuthError,
  PublicClientApplication,
} from '@azure/msal-browser'

const msalInstance = new PublicClientApplication(msalConfig)

const auth = {
  msalInstance,
  acquireToken: async (request?: any) => {
    const acquireRequest = {
      ...tokenRequest,
      ...request,
    }
    let tokenResponse: AuthenticationResult | null = null
    try {
      tokenResponse = (await msalInstance.acquireTokenSilent(acquireRequest))
    } catch (error) {
      console.warn(error)
      if (error instanceof InteractionRequiredAuthError) {
        try {
          tokenResponse = (await msalInstance.acquireTokenPopup(acquireRequest))
        } catch (err) {
          console.error(err)
        }
      }
    }
    return {token: tokenResponse?.accessToken, expiresAt: tokenResponse?.expiresOn.getTime()}
  },
  hasClaim: (claimType: string, claimValue: string) => {
    const claims = msalInstance.getAllAccounts()[0].idTokenClaims
    return claims && claims[claimType] === claimValue
  },
}

export default auth
