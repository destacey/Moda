import { msalConfig, tokenRequest } from '@/auth-config'
import {
  AuthenticationResult,
  EventMessageUtils,
  InteractionRequiredAuthError,
  InteractionStatus,
  PublicClientApplication,
} from '@azure/msal-browser'

const msalInstance = new PublicClientApplication(msalConfig)
const msalState = {
  initializing: null as Promise<void> | null,
  interactionStatus: InteractionStatus.None as InteractionStatus,
}
const initialize = async () => {
  if (msalWrapper.isInitialized) return
  if (msalState.initializing === null) {
    msalInstance.addEventCallback((event) => {
      msalState.interactionStatus =
        EventMessageUtils.getInteractionStatusFromEvent(
          event,
          msalState.interactionStatus,
        ) ?? InteractionStatus.None
    })
    msalState.initializing = msalInstance.initialize().then(() => {
      if (
        msalInstance.getAllAccounts().length > 0 &&
        !msalInstance.getActiveAccount()
      ) {
        msalInstance.setActiveAccount(msalInstance.getAllAccounts()[0])
      }
      msalWrapper.isInitialized = true
      msalState.initializing = null
    })
  }
  await msalState.initializing
}

const msalWrapper = {
  isInitialized: false,
  interactionStatus: () => msalState.interactionStatus,
  initialize,
  getInstance: async () => {
    await initialize()
    return msalInstance
  },
  /// Do not use this directly, use getInstance() instead to ensure initialization
  instance: msalInstance,
}

const auth = {
  msalWrapper,
  acquireToken: async (request?: any) => {
    const acquireRequest = {
      ...tokenRequest,
      ...request,
    }
    let tokenResponse: AuthenticationResult | null = null
    try {
      tokenResponse = await msalInstance.acquireTokenSilent(acquireRequest)
    } catch (error) {
      console.warn(error)
      if (error instanceof InteractionRequiredAuthError) {
        try {
          tokenResponse = await msalInstance.acquireTokenPopup(acquireRequest)
        } catch (err) {
          console.error(err)
        }
      }
    }
    return {
      token: tokenResponse?.accessToken,
      expiresAt: tokenResponse?.expiresOn.getTime(),
    }
  },
  hasClaim: (claimType: string, claimValue: string) => {
    const claims = msalInstance.getAllAccounts()[0].idTokenClaims
    return claims && claims[claimType] === claimValue
  },
}

export default auth
