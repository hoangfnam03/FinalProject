FROM nginx:stable-alpine

# Copy toàn bộ file FE vào thư mục html
COPY . /usr/share/nginx/html/

# Copy file nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
