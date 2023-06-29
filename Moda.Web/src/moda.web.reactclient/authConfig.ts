import { Configuration, LogLevel } from '@azure/msal-browser'

const microsoftLogonAuthority =
  process.env.NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY ?? ''
const azureAdClientId = process.env.NEXT_PUBLIC_AZURE_AD_CLIENT_ID ?? ''

export const msalConfig: Configuration = {
  auth: {
    authority: microsoftLogonAuthority,
    clientId: azureAdClientId,
    redirectUri: '/',
    postLogoutRedirectUri: '/',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    allowNativeBroker: false, // Disables WAM Broker
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        switch (level) {
          case LogLevel.Error:
            console.error(message)
            return
          case LogLevel.Info:
            console.info(message)
            return
          case LogLevel.Verbose:
            console.debug(message)
            return
          case LogLevel.Warning:
            console.warn(message)
            return
        }
      },
    },
  },
}

export const tokenRequest = {
  scopes: ['api://fdca5e6f-46a2-455c-b2f3-06a9a6877190/access_as_user'],
  forceRefresh: false,
}

export const graphConfig = {
  graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
}
