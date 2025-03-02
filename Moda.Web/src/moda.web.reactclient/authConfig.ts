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
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return // Ensures sensitive data isn't logged

        switch (level) {
          case LogLevel.Error:
            console.error(message)
            return
          case LogLevel.Warning:
            console.warn(message)
            return
          // Uncomment if you want more logging
          // case LogLevel.Info:
          //   console.info(message);
          //   return;
          // case LogLevel.Verbose:
          //   console.debug(message);
          //   return;
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
