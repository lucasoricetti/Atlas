export type RuntimeConfigKey =
  | "VITE_API_BASE_URL"
  | "VITE_AAD_CLIENT_ID"
  | "VITE_AAD_AUTHORITY"
  | "VITE_API_SCOPE"
  | "VITE_APP_ORIGIN"
  | "VITE_AAD_REDIRECT_URI"
  | "VITE_AAD_POST_LOGOUT_REDIRECT_URI";

type RuntimeConfig = Partial<Record<RuntimeConfigKey, string>>;

declare global {
  interface Window {
    __APP_CONFIG__?: RuntimeConfig;
  }
}

const readRuntimeValue = (key: RuntimeConfigKey): string => {
  if (typeof window === "undefined") {
    return "";
  }

  return String(window.__APP_CONFIG__?.[key] ?? "").trim();
};

const readBuildValue = (key: RuntimeConfigKey): string =>
  String(import.meta.env[key] ?? "").trim();

export const getAppConfig = (key: RuntimeConfigKey): string => {
  const runtimeValue = readRuntimeValue(key);
  if (runtimeValue) {
    return runtimeValue;
  }

  return readBuildValue(key);
};
