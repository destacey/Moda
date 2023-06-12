import { Configuration } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    authority: "https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef",
    clientId: "4d566fb3-7966-4c77-9864-113020fd646f",
    redirectUri: "/",
    postLogoutRedirectUri: "/",
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
}

export const tokenRequest = {
  scopes: ["api://fdca5e6f-46a2-455c-b2f3-06a9a6877190/access_as_user"],
  forceRefresh: false
}

export const graphConfig = {
  graphMeEndpoint: "https://graph.microsoft.com/v1.0/me"
};
