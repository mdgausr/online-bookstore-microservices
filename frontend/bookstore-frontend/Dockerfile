FROM node:18-alpine as build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm install --no-audit --no-fund
COPY . .
RUN npm run build --prod

FROM nginx:stable-alpine
COPY --from=build /app/dist/bookstore-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
