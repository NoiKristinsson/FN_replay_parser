# Use Nginx to serve the pre-built Blazor WebAssembly app
FROM nginx:latest
WORKDIR /usr/share/nginx/html

# Copy pre-built WebAssembly files from `Publish/web`
COPY publish/web .

# Remove default Nginx configuration and add custom config
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
