# MagellanGPT

## Context
Project to perfom an AI assistant for enterprise with RAG functionality
- Users must be able to to converse with the AI
- Users must be able to load PDF files and get AI completion from this data source
- The conversation history must be saved, and the user must be able to access it and resume the conversation where he left off.

## Architecture
CleanArchiteture based on Jayson Taylor Project  
[![Clean Architecture with ASP.NET Core 3.0 • Jason Taylor • GOTO 2019](https://img.youtube.com/vi/dK4Yb6-LxAk/0.jpg)](https://www.youtube.com/watch?v=dK4Yb6-LxAk)


- Domain Layer

This layer encompasses all entities, enums, exceptions, interfaces, types, and the domain-specific logic. It serves as the core foundation of the domain layer, housing the essential components that define the business model and its rules.

- Application Layer

This layer is responsible for the application's logic. It relies on the Domain layer but is independent of any other layers or external projects. The Application layer specifies interfaces that external layers implement. For instance, should the application need to utilize a notification service, a new interface will be introduced within this layer, and its implementation will be carried out in the Infrastructure layer.

- Infrastructure Layer

This layer includes classes that facilitate access to external resources, such as file systems, web services, SMTP, etc. These classes are designed based on the interfaces outlined in the Application layer, ensuring a decoupled architecture that promotes flexibility and maintainability.

- API Layer

The API Layer acts as the gateway for external communications to the application. It translates requests from the outside world into actions that can be processed by the Application layer, and then maps the results back to responses. This layer handles all HTTP request routing. By isolating these responsibilities in the API Layer, the application's internal architecture remains decoupled from the external interface.

- Shared

The Shared Layer consists of common utilities, helper functions, and shared services that can be used across all other layers. This includes cross-cutting concerns such as logging, configuration, and custom libraries for operations like encryption and error handling. The Shared Layer promotes code reuse and helps maintain consistency throughout the application.
## Dependencies
* [MediatR](https://github.com/jbogard/MediatR)
* [Entity Framework]()
* [Azure OpenAI]()
* [Semantic Kernel]()
* [Azure Bing Search]()
* [Azure AI Search]()

## Setup
- ASP.NET Core 8.0  
- Visual Studio or VS Code & IIS Express

## Code quality analysis
Use SonarQube image with docker - sonar:lts-community
```bash
docker run -d --name sonarqube -p 9000:9000 -p 9092:9092 sonarqube
docker run -ti -v $(pwd):/root/src --link sonarqube newtmitch/sonar-scanner
```

Go to http://localhost:9000
User: admin
Password: admin

Create project and follow differents steps

sqp_7fcaa6fc043ce04f1a6c7a5154e8006823e89060

dotnet sonarscanner begin /k:"MagellantGPT_MagellantGPT_b7c99bb8-25a3-4b1d-9725-bb2e1fec4047" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_7fcaa6fc043ce04f1a6c7a5154e8006823e89060"
dotnet sonarscanner end /d:sonar.token="sqp_b811a5803bd5f6ed68f11da77a2a657207f6a7d4"
