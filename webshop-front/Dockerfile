FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
ARG VITE_API_BASE
ARG VITE_MERCHANT_ID
RUN echo "VITE_API_BASE=$VITE_API_BASE" > .env
RUN echo "VITE_MERCHANT_ID=$VITE_MERCHANT_ID" >> .env
RUN npm run build

FROM nginx:stable-alpine AS runtime
COPY --from=builder /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
