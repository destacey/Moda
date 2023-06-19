import { msalConfig, tokenRequest } from "@/authConfig";
import { InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";

const msalInstance = new PublicClientApplication(msalConfig)

const auth = {
  msalInstance,
  acquireToken: async (request?: any) => {
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
  },
  hasClaim: (claimType: string, claimValue: string) => {
    const claims = msalInstance.getAllAccounts()[0].idTokenClaims;
    return claims && claims[claimType] === claimValue;
  }
}

export default auth
