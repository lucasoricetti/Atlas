import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";

import { MsalProvider } from "@azure/msal-react";
import { msalInstance, syncActiveAccount } from "./auth/msalInstance";

import App from "./App";
import "./styles.css";
import { AuthGuard } from "./auth/AuthGuard";

const bootstrap = async () => {
  await msalInstance.initialize();
  await msalInstance.handleRedirectPromise();
  syncActiveAccount();

  ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <BrowserRouter>
          <AuthGuard />
          <App />
        </BrowserRouter>
      </MsalProvider>
    </React.StrictMode>
  );
};

void bootstrap();