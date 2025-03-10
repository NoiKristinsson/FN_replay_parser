FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy built Blazor files
COPY ./publish/web/wwwroot .

# Copy the correct Nginx site configuration
COPY default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]