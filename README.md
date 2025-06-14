# StreamFlow - Microservices
This is the repository of a microservices architecture for the fictitious application "StreamFlow" from the subject "Arquitectura de Sistemas" at the Universidad Católica del Norte. All the necessary tools and commands to run the project are described below.

## Pre-requisites
- [.NET SDK](https://dotnet.microsoft.com/es-es/download) (version 9.0.4)
- [MySQL](https://www.mysql.com/) (version 8.0.29) 
- [PostgreSQL](https://www.postgresql.org/) (version 15.6.0)
- [MariaDB](https://mariadb.org/) (version 10.7.4)
- [MongoDB](https://www.mongodb.com/) (version 5.0.3)
- [Git](https://git-scm.com/) (version 2.49.0)
- [RabbitMQ image](https://hub.docker.com/_/rabbitmq) (version 4.1.1)
- [Docker Desktop](https://www.docker.com/) (version 4.42.0)

## Installation and configuration

1. **Clone the repository**
```bash
git clone https://github.com/NachoXx25/Microservicios-StreamFlow.git
```

2. **Navigate to the project directory**
```bash
cd Microservicios-StreamFlow
``` 

3. **Run the services and the API Gateway**
Microservices have a specific order of execution, mainly because of the connections that exist between them and their data. Thus, the order and steps to be executed in each one are described:

Execution order of the microservices and the API Gateway

| Execution Order |  Project folder                | Database    |
|:----------------|:------------------------------:|------------:|
| 1               | MonitoringMicroservice         | MongoDB     |
| 2               | UserMicroservice               | MySQL       |
| 3               | AuthMicroservice               | PostgreSQL  |
| 4               | BillMicroservice               | MariaDB     |
| 5               | VideoMicroservice              | MongoDB     |
| 6               | SocialInteractionsMicroservice | MongoDB     |
| 7               | PlaylistMicroservice           | PostgreSQL  |
| 8               | EmailMicroservice              | No database |
| 9               | ApiGateway                     | No database |

Microservices and the API Gateway share 4 common steps to execute them:

3.1 **Navigate to the service project folder**

```
cd MicroserviceProjectName
```

3.2 **Restore the dependencies**
```
cd dotnet restore
```

3.3 **Create a ```.env``` file on the root of the project and fill the environment variables**
```bash
cp .env.example .env
```

In the ```.env``` file:

#### For MySQL databases:
- On the ```MYSQL_CONNECTION``` replace:
    - ```your_port``` with your MySQL port.
    - ```your_database``` with your database name.
    - ```your_username``` with your MySQL username.
    - ```your_password``` with your MySQL password.
- On the ```RABBITMQ_EXCHANGE``` replace:
    - ```your_rabbitmq_exchange``` with your RabbitMQ exchange name.

Once you have replaced everything, save the changes and move on to the next step.

#### For PostgreSQL databases:

- On the ```JWT_SECRET``` replace (only in Authentication service):
    - ```your_jwt_secret_key``` with your JWT secret key.

- On the ```POSTGRES_CONNECTION_STRING``` replace:
    - ```your-port``` with your PostgreSQL port.
    - ```your-database-name``` with your database name.
    - ```your-username``` with your PostgreSQL username.
    - ```your-password``` with your PostgreSQL password.
- On the ```RABBITMQ_EXCHANGE``` replace:
    - ```your_rabbitmq_exchange``` with your RabbitMQ exchange name.

Once you have replaced everything, save the changes and move on to the next step.

#### For MariaDB databases:
- On the ```MARIADB_CONNECTION``` replace:
    - ```mariadb_port``` with your MySQL port.
    - ```your_db``` with your database name.
    - ```your_user``` with your MySQL username.
    - ```your_password``` with your MySQL password.
- On the ```RABBITMQ_EXCHANGE``` replace:
    - ```your_rabbitmq_exchange``` with your RabbitMQ exchange name.

Once you have replaced everything, save the changes and move on to the next step.

#### For MongoDB databases:
- On the ```MONGODB_CONNECTION```:
    - ```your_user``` with your MongoDB user.
    - ```your_password``` with your MongoDB password.
    - ```mongodb_port``` with your MongoDB Port.
- On the ```MONGODB_DATABASE_NAME```:
    - ```your_mongodb_db_name``` with your MongoDB database name.
- On the ```RABBITMQ_EXCHANGE``` replace:
    - ```your_rabbitmq_exchange``` with your RabbitMQ exchange name.

Once you have replaced everything, save the changes and move on to the next step.

#### For the Email service: 
- On the ```RABBITMQ_EXCHANGE``` replace:
    - ```your_rabbitmq_exchange``` with your RabbitMQ exchange name.
- On the ```FROM_EMAIL``` replace:
    - ```your_email@gmail.com"``` with your gmail.

- On the ```FROM_EMAIL_PASSWORD``` replace:
    - ```your_password``` with your gmail application password. This password **IS NOT** your gmail password, instead, is a dedicated password for an application. To obtain this password, go to the [Obtain an App Password](#obtain-an-app-password) section.

#### For the API Gateway:
- On the ```JWT_SECRET``` replace:
    - ```your_jwt_secret_key``` with your JWT secret key.

Once you have replaced everything, save the changes and move on to the next step.

3.4 **Run the service**
```
dotnet run
```

Repeat the steps 3.1 to 3.4 for each service until all are running. Make sure to run all the services in the order listed in [step 3](#run-the-services-and-the-api-gateway) before running the API Gateway.

## Table of contents
This section shows the steps to execute the application seeders and obtain the gmail application password for the EmailService.

### How to execute the seeders
1. **Run the RabbitMQ container on Docker**
```
docker run your_rabbitmq_container_name
```
Replace ```your_rabbitmq_container_name``` with your the name of your RabbitMQ docker container.

You can also run the container using the Docker Desktop Application.

2. **Make sure that all the services are running**
```
dotnet run
```
Use the above command on each service and this will load the seeders for each and also stream them through RabbitMQ as appropriate. 

**The services have to run in the order given in the [Installation and Configuration](#installation-and-configuration) section, otherwise the seeder's load can fail.**

### Seed data
There is seed data for the Video, Bill, User and Social Interactions services.

There are: 
- **152 Users**, including 2 specific users with each system role ("administrador" and "cliente") for testing purposes. The data for each are:
    - Administrador:
        - Id: 2
        - First name: Juana
        - Last name: Valencia
        - Email: juana@gmail.com
        - Password: Password123!
    - Cliente:
        - Id: 1 
        - First name: Juan
        - Last name: Perez
        - Email: juan@gmail.com
        - Password: Password123!
    User seeders are sent by RabbitMQ to the Authentication and Billing services, keeping the system users synchronized.

- **350 Bills**. As the Bill services is associated to the users, these seeders are executed only by having the seeders of the User service.
- **452 Videos**, including especific Videos to test on the Postman flows. These video details are:
    - Video 1:
        - Id: 507f1f77bcf86cd799439011
        - Title: Primer Video de Prueba
        - Description: Este es un video de prueba para verificar la funcionalidad del microservicio de videos.
        - Genre: Comedia
    - Video 2:
        - Id: 507f1f77bcf86cd799439012
        - Title: Segundo Video de Prueba
        - Description: Este es otro video de prueba para verificar la funcionalidad del microservicio de videos.
        - Genre: Acción
    Video seeders are sent by RabbitMQ to the Playlist and Social Interactions services, keeping the system videos synchronized.

- **75 Likes and 35 Comments**.
    These social interactions are only created when the video seeders of the Video service have been uploaded via RabbitMQ. Both likes and comments are randomly assigned, avoiding having a video that has all social interactions loaded.

### Obtain an App Password
To use Gmail services via SMTP messaging protocol, it is no longer allowed to use your email password. For security reasons, "Application Passwords" are now used, which are unique and dedicated passwords for use with certain services. In the case of this project, the email service uses this protocol, so it is necessary to obtain this password for it to be able to send emails.  To obtain your application password, follow these steps:

1. **Enable 2-Step Verification**
- Go to your Google Account settings by clicking on your profile picture and then "Manage your Google Account". 
- Navigate to the "Security" tab. 
- Unnder "Signing in to Google," select "2-Step Verification". 
Follow the instructions to turn on 2-Step Verification. 

2. **Generate the App Password**
- Once 2-Step Verification is enabled, go to the App passwords page (you might need to sign in again). 
- Click on "Select app" and choose the application you're using. 
- Click on "Select device" and choose the device you're using. 
- Click "Generate". 
- Copy the generated 16-digit app password carefully. You won't be able to see it again after closing the window. 

## Authors
- [@Sebastián Núñez](https://github.com/2kSebaNG)
- [@Ignacio Valenzuela](https://github.com/NachoXx25)