const RETURN_TO_KEY = "atlas:auth:return-to";

export const storeReturnTo = (value: string) => {
  window.sessionStorage.setItem(RETURN_TO_KEY, value);
};

export const readReturnTo = (): string => {
  return window.sessionStorage.getItem(RETURN_TO_KEY) ?? "/";
};

export const clearReturnTo = () => {
  window.sessionStorage.removeItem(RETURN_TO_KEY);
};