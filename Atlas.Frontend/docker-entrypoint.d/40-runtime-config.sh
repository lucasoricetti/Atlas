#!/bin/sh
set -eu

CONFIG_FILE="/usr/share/nginx/html/runtime-config.js"

escape_js() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

write_value() {
  key="$1"
  value="${2:-}"
  escaped_value="$(escape_js "$value")"
  printf '  %s: "%s",\n' "$key" "$escaped_value" >> "$CONFIG_FILE"
}

cat > "$CONFIG_FILE" <<'EOF'
window.__APP_CONFIG__ = {
EOF

write_value "VITE_API_BASE_URL" "${VITE_API_BASE_URL:-}"
write_value "VITE_AAD_CLIENT_ID" "${VITE_AAD_CLIENT_ID:-}"
write_value "VITE_AAD_AUTHORITY" "${VITE_AAD_AUTHORITY:-}"
write_value "VITE_API_SCOPE" "${VITE_API_SCOPE:-}"
write_value "VITE_APP_ORIGIN" "${VITE_APP_ORIGIN:-}"
write_value "VITE_AAD_REDIRECT_URI" "${VITE_AAD_REDIRECT_URI:-}"
write_value "VITE_AAD_POST_LOGOUT_REDIRECT_URI" "${VITE_AAD_POST_LOGOUT_REDIRECT_URI:-}"

cat >> "$CONFIG_FILE" <<'EOF'
};
EOF
