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
    cacheLocation: 'localStorage',
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return

        switch (level) {
          case LogLevel.Error:
            console.error(message)
            break
          case LogLevel.Warning:
            console.warn(message)
            break
        }
      },
    },
  },
}

export const tokenRequest = {
  scopes: [process.env.NEXT_PUBLIC_API_SCOPE],
  forceRefresh: false,
}

export const graphConfig = {
  graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
}
