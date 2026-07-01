import type { Configuration } from "@azure/msal-browser";
import { getAppConfig } from "../config/runtimeConfig";

const aadClientId = getAppConfig("VITE_AAD_CLIENT_ID");
const aadAuthority = getAppConfig("VITE_AAD_AUTHORITY");
const apiScope = getAppConfig("VITE_API_SCOPE");
const fallbackOrigin = getAppConfig("VITE_APP_ORIGIN");

if (!aadClientId) {
  throw new Error("Missing VITE_AAD_CLIENT_ID environment variable.");
}

if (!aadAuthority) {
  throw new Error("Missing VITE_AAD_AUTHORITY environment variable.");
}

if (!apiScope) {
  throw new Error("Missing VITE_API_SCOPE environment variable.");
}

const defaultRedirectUri =
  typeof window !== "undefined" ? window.location.origin : fallbackOrigin;
const redirectUriFromEnv = getAppConfig("VITE_AAD_REDIRECT_URI");
const redirectUri = redirectUriFromEnv || defaultRedirectUri;

const postLogoutRedirectUriFromEnv = getAppConfig("VITE_AAD_POST_LOGOUT_REDIRECT_URI");
const postLogoutRedirectUri =
  postLogoutRedirectUriFromEnv || `${redirectUri}/login`;

if (!redirectUri) {
  throw new Error("Missing redirect URI. Check window.location or VITE_AAD_REDIRECT_URI.");
}

export const API_SCOPES = [apiScope];

export const msalConfig: Configuration = {
  auth: {
    clientId: aadClientId,
    authority: aadAuthority,
    redirectUri,
    postLogoutRedirectUri
  },
  cache: {
    cacheLocation: "localStorage"
  }
};

export const loginRequest = {
  scopes: API_SCOPES
};

export const apiRequest = {
  scopes: API_SCOPES
};