FROM node:20-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG API_URL=
ENV API_URL=$API_URL

RUN node -e "const fs=require('node:fs'); const path='src/environments/environment.ts'; const apiUrl=process.env.API_URL ?? ''; const content=fs.readFileSync(path,'utf8'); const next=content.replace(/api_url:\s*'.*?'/, 'api_url: ' + JSON.stringify(apiUrl)); if (content === next) { throw new Error('Failed to inject API_URL into src/environments/environment.ts'); } fs.writeFileSync(path, next);" \
    && npm run build

FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/infra-flow-sculptor-front/browser /usr/share/nginx/html

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]