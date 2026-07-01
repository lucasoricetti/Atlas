import { EventType, PublicClientApplication, type AccountInfo, type AuthenticationResult } from "@azure/msal-browser";
import { msalConfig } from "./msalConfig";

export const msalInstance = new PublicClientApplication(msalConfig);

export const syncActiveAccount = (): AccountInfo | null => {
  const currentActiveAccount = msalInstance.getActiveAccount();
  const activeAccount = currentActiveAccount ?? msalInstance.getAllAccounts()[0] ?? null;

  if (activeAccount && currentActiveAccount?.homeAccountId !== activeAccount.homeAccountId) {
    msalInstance.setActiveAccount(activeAccount);
  }

  return activeAccount;
};

msalInstance.addEventCallback(event => {
  if (
    event.eventType === EventType.LOGIN_SUCCESS ||
    event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS
  ) {
    const payload = event.payload as AuthenticationResult | null;

    if (payload?.account) {
      msalInstance.setActiveAccount(payload.account);
    }
  }
});