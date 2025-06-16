openssl genrsa -out mykey.pem 2048
openssl req -new -key mykey.pem -out myrequest.csr
openssl x509 -req -days 365 -in myrequest.csr -signkey mykey.pem -out mycert.pem