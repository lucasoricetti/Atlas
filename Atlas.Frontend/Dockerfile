# Stage di build: compila l'app React/Vite e produce i file statici in /app/dist.
FROM node:22-alpine AS build
WORKDIR /app

# Copia manifest dipendenze separatamente per sfruttare meglio la cache Docker.
COPY package*.json ./
RUN npm ci

COPY . .

# Build args per valorizzare import.meta.env durante la build Vite.
ARG VITE_API_BASE_URL
ARG VITE_AAD_CLIENT_ID
ARG VITE_AAD_AUTHORITY
ARG VITE_API_SCOPE
ARG VITE_APP_ORIGIN
ARG VITE_AAD_REDIRECT_URI
ARG VITE_AAD_POST_LOGOUT_REDIRECT_URI

# Espone i valori al processo di build.
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
ENV VITE_AAD_CLIENT_ID=${VITE_AAD_CLIENT_ID}
ENV VITE_AAD_AUTHORITY=${VITE_AAD_AUTHORITY}
ENV VITE_API_SCOPE=${VITE_API_SCOPE}
ENV VITE_APP_ORIGIN=${VITE_APP_ORIGIN}
ENV VITE_AAD_REDIRECT_URI=${VITE_AAD_REDIRECT_URI}
ENV VITE_AAD_POST_LOGOUT_REDIRECT_URI=${VITE_AAD_POST_LOGOUT_REDIRECT_URI}

RUN npm run build

# Stage runtime: serve i file statici tramite Nginx.
FROM nginx:1.27-alpine AS runtime
RUN rm -rf /usr/share/nginx/html/*
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
# Script eseguito all'avvio container per generare runtime-config.js da env runtime.
COPY docker-entrypoint.d/40-runtime-config.sh /docker-entrypoint.d/40-runtime-config.sh
RUN sed -i 's/\r$//' /docker-entrypoint.d/40-runtime-config.sh \
	&& chmod +x /docker-entrypoint.d/40-runtime-config.sh

#Questa è solo informativa, la vera esposizione della porta avviene tramite docker-compose.yml
EXPOSE 80
