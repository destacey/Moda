import { msalConfig, tokenRequest } from "@/authConfig";
import { InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";

export const msalInstance = new PublicClientApplication(msalConfig);

export async function acquireToken(request?: any) {
  const acquireRequest = {
    ...tokenRequest,
    ...request
  };
  if (!msalInstance.getActiveAccount()) {
    msalInstance.setActiveAccount(msalInstance.getAllAccounts()[0]);
  }
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

