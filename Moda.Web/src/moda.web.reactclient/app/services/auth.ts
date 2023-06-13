import { msalConfig, tokenRequest } from "@/authConfig";
import { AuthenticationResult, EventType, InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";

export const msalInstance = new PublicClientApplication(msalConfig);
msalInstance.addEventCallback((event) => {
  if(event.eventType === EventType.LOGIN_SUCCESS && (event.payload as any)?.account) {
    msalInstance.setActiveAccount((event.payload as any)?.account);
    console.log('login success');
  }
});

export async function acquireToken(request?: any) {
  const acquireRequest = {
    ...tokenRequest,
    ...request
  };

  let tokenResponse: string | null = null;
  try {
    tokenResponse = (await msalInstance.acquireTokenSilent(acquireRequest)).accessToken;
  }
  catch (error) {
    console.warn(error);
    if(error instanceof InteractionRequiredAuthError) {
      try {
        tokenResponse = (await msalInstance.acquireTokenPopup(acquireRequest)).accessToken;
      }
      catch (err) {
        console.error(err);
      }
    }
  }
  return tokenResponse;
}

