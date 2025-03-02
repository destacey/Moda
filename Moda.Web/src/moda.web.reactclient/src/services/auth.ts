import { msalConfig, tokenRequest } from '@/authConfig'
import {
  AuthenticationResult,
  EventMessageUtils,
  EventType, // ✅ Import EventType
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
    if (!msalWrapper.eventCallbackRegistered) {
      // Listen for authentication changes across tabs using correct EventType enum
      msalInstance.addEventCallback((event) => {
        if (
          event.eventType === EventType.ACCOUNT_ADDED ||
          event.eventType === EventType.ACCOUNT_REMOVED
        ) {
          console.info(`[MSAL] Account change detected: ${event.eventType}`)
          handleAccountChange()
        }

        msalState.interactionStatus =
          EventMessageUtils.getInteractionStatusFromEvent(
            event,
            msalState.interactionStatus,
          ) ?? InteractionStatus.None
      })

      msalWrapper.eventCallbackRegistered = true
    }

    msalState.initializing = msalInstance.initialize().then(() => {
      const activeAccount =
        msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0]

      if (activeAccount) {
        msalInstance.setActiveAccount(activeAccount)
      }

      msalWrapper.isInitialized = true
      msalState.initializing = null
    })
  }

  await msalState.initializing
}

// Handle account changes (for multi-tab authentication)
const handleAccountChange = () => {
  const activeAccount = msalInstance.getActiveAccount()

  if (!activeAccount) {
    console.warn('[MSAL] No active account found, user might be logged out.')
  } else {
    console.info(`[MSAL] Active account updated: ${activeAccount.username}`)
  }
}

const msalWrapper = {
  isInitialized: false,
  eventCallbackRegistered: false,
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
      console.warn(
        'Silent token acquisition failed, trying interactive login',
        error,
      )

      if (error instanceof InteractionRequiredAuthError) {
        const activeAccount = msalInstance.getActiveAccount()

        if (!activeAccount) {
          console.error(
            '[MSAL] No active account found, redirecting to login...',
          )
          await msalInstance.loginRedirect()
        } else {
          try {
            tokenResponse = await msalInstance.acquireTokenPopup(acquireRequest)
          } catch (popupError) {
            console.error('[MSAL] Popup login failed', popupError)
          }
        }
      }
    }

    return {
      token: tokenResponse?.accessToken,
      expiresAt: tokenResponse?.expiresOn?.getTime(),
    }
  },
  hasClaim: (claimType: string, claimValue: string) => {
    const activeAccount = msalInstance.getActiveAccount()
    if (!activeAccount || !activeAccount.idTokenClaims) {
      return false
    }
    return activeAccount.idTokenClaims[claimType] === claimValue
  },
}

export default auth
