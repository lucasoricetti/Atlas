import axios from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { clearReturnTo, storeReturnTo } from "../auth/authState";
import { apiRequest } from "../auth/msalConfig";
import { msalInstance, syncActiveAccount } from "../auth/msalInstance";
import { getAppConfig } from "../config/runtimeConfig";

const rawApiBaseUrl = getAppConfig("VITE_API_BASE_URL");
const normalizedApiBaseUrl = rawApiBaseUrl
  .replace(/\/+$/, "")
  .replace(/\/api$/i, "");

const api = axios.create({
  baseURL: normalizedApiBaseUrl || undefined,
  headers: { "Content-Type": "application/json" }
});

function resolveFriendlyMessage(error: unknown): string {
  const axiosError = error as {
    message?: string;
    response?: {
      status?: number;
      data?: {
        detail?: string;
        title?: string;
      };
    };
  };

  const status = axiosError?.response?.status;
  const problemDetails = axiosError?.response?.data;

  if (problemDetails?.detail || problemDetails?.title) {
    return problemDetails.detail ?? problemDetails.title ?? "Error during API request";
  }

  if (status === 400) {
    return "The request is invalid. Please check the entered data.";
  }

  if (status === 401) {
    return "Your session has expired. Please sign in again.";
  }

  if (status === 403) {
    return "You do not have permission to perform this action.";
  }

  if (status === 404) {
    return "The requested resource was not found.";
  }

  if (status === 409) {
    return "This action conflicts with existing data.";
  }

  if (status === 422) {
    return "Some fields are invalid. Please review and try again.";
  }

  if (typeof status === "number" && status >= 500) {
    return "A server error occurred. Please try again later.";
  }

  if (axiosError?.message === "Network Error") {
    return "Unable to reach the server. Check your connection and try again.";
  }

  return axiosError?.message || "Error during API request";
}

api.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  const account = syncActiveAccount();

  if (!account) {
    return config;
  }

  try {
    const tokenResult = await msalInstance.acquireTokenSilent({
      ...apiRequest,
      account
    });

    config.headers.Authorization = `Bearer ${tokenResult.accessToken}`;
  } catch (error) {
    if (!(error instanceof InteractionRequiredAuthError)) {
      console.warn("Unable to acquire API token silently.", error);
    }
  }

  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error?.response?.status === 401 && window.location.pathname !== "/login") {
      const account = syncActiveAccount();

      if (!account) {
        clearReturnTo();
        storeReturnTo(`${window.location.pathname}${window.location.search}${window.location.hash}`);
        window.location.assign("/login");
      }
    }

    const pd = error?.response?.data;
    const message = resolveFriendlyMessage(error);

    return Promise.reject({
      ...error,
      friendlyMessage: message,
      problemDetails: pd
    });
  }
);

export default api;