# Stage 1: Build React app
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files and install dependencies
COPY package.json package-lock.json* ./ 
RUN npm install

# Copy all source files
COPY . .

# Build the React app for production
RUN npm run build

# Stage 2: Serve the built app with nginx
FROM nginx:stable-alpine

# Copy the build output to nginx's default public folder
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx configuration file
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
