echo Generate certificate for tms-auth-service

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-auth-service.key" -out "C:\docker-certs\tms\tms-auth-service.crt" -subj "/CN=tms-auth-service" -addext "subjectAltName=DNS:tms-auth-service"

echo Generate certificate for tms-file-storage-service

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-file-storage-service.key" -out "C:\docker-certs\tms\tms-file-storage-service.crt" -subj "/CN=tms-file-storage-service" -addext "subjectAltName=DNS:tms-file-storage-service"

echo Generate certificate for tms-gateway

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-gateway.key" -out "C:\docker-certs\tms\tms-gateway.crt" -subj "/CN=tms-gateway" -addext "subjectAltName=DNS:tms-gateway"

echo Generate certificate for tms-notification-service

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-notification-service.key" -out "C:\docker-certs\tms\tms-notification-service.crt" -subj "/CN=tms-notification-service" -addext "subjectAltName=DNS:tms-notification-service"

echo Generate certificate for tms-notification-service

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-task-service.key" -out "C:\docker-certs\tms\tms-task-service.crt" -subj "/CN=tms-task-service" -addext "subjectAltName=DNS:tms-task-service"

echo Generate certificate for tms-webapp

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "C:\docker-certs\tms\tms-webapp.key" -out "C:\docker-certs\tms\tms-webapp.crt" -subj "/CN=tms-webapp" -addext "subjectAltName=DNS:tms-webapp"

pause