# ============================================================
# FinancialPortfolio.Client - Multi-stage Dockerfile (Angular 22)
# ============================================================

# ---------- Build stage ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci --prefer-offline --no-audit

COPY . .
RUN npm run build -- --configuration=production

# ---------- Production stage ----------
FROM nginx:1.27-alpine AS final

RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Correct path (confirmed)
COPY --from=build /app/dist/FinancialPortfolio.Client/browser /usr/share/nginx/html

EXPOSE 80

# Softer healthcheck (nginx alpine friendly)
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget -q --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]