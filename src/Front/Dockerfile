# syntax=docker/dockerfile:1.7

# --- Build stage --------------------------------------------------------------
FROM node:22-alpine AS build

WORKDIR /app

# Install deps in their own layer for better caching.
COPY package.json package-lock.json angular.json tsconfig.json tsconfig.app.json tsconfig.spec.json tailwind.config.js proxy.conf.js ./
RUN --mount=type=cache,target=/root/.npm npm ci --ignore-scripts --no-audit --no-fund

COPY public ./public
COPY src ./src

ARG API_URL=
ENV API_URL=$API_URL

RUN node -e "const fs=require('node:fs'); const path='src/environments/environment.ts'; const apiUrl=process.env.API_URL ?? ''; const content=fs.readFileSync(path,'utf8'); const next=content.replace(/api_url:\s*'.*?'/, 'api_url: ' + JSON.stringify(apiUrl)); if (content === next) { throw new Error('Failed to inject API_URL into src/environments/environment.ts'); } fs.writeFileSync(path, next);" \
    && npm run build -- --configuration=production

# --- Runtime stage ------------------------------------------------------------
FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/infra-flow-sculptor-front/browser /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1:8080/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
