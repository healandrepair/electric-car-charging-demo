This is a electric car charging demo app. 

This simulates the battery charging and depletion, with the ability to schedule, stop and start battery charging of your electric car.

Stack:
* AngularJS for frontend
* Azure Functions for exposing REST APIs
* IoT Messaging: Azure IoT Hub for device connectivity and message ingestion
* C# console application using the Azure IoT Device SDK to simulate car battery level and charging state
* Azure Table Storage for creating, and scheduling car charging schedule data


<img width="1370" height="801" alt="image" src="https://github.com/user-attachments/assets/ca562d92-c3f7-42b5-bae5-6f6205af1987" />
<img width="1184" height="436" alt="image" src="https://github.com/user-attachments/assets/33edc009-56fa-4f66-a05a-de4714a02fa6" />

To run:
* Run frontend eg ng serve --configuration production
* Run CarConsoleApp
* Use connectionString for device for ConsoleApp.
* Setup Azure environment  


*Dev Notes and thoughts:*

CORS currently allows local access, but for production you’ll need to configure it properly to only allow requests from the deployed frontend URL.

Authentication is currently set to Anonymous, but the Azure Function endpoints should be secured to prevent unauthorized access to car battery information.
- In the future, you’ll also want to establish a user-to-car relationship, where a user can own multiple cars, ensuring that only authorised users can view or update a car’s charging state.

Use KeyVault to store and retrieve secrets, there should be NO CODE that is committed with secrets. 
CI/CD pipeline needs to be added, this ensures consistent delivery for the application. Useful stuff may include: 
- Deploy upon merge to master
- Run unit and integration tests and build before deploying, this helps with regression testing and ensures that our app is bug free and performs as expected.
- Automated rollback mechanisms
- Environment specific configs, eg dev,sand, prod environments

May need a CRON job or process to clear out completed or stale schedules.
- This prevents the database from increasing in size, just general cleanup.

The charging schedule execution could run on a shorter polling interval, or be redesigned using a Pub/Sub model for better scalability

ConsoleApp should not be using Console.WriteLine, should be using ILogger. This way the logs persist and not get nuked as once as the app is shutdown, but for sake of time and demo, we will use Console.WriteLine.

If want to change device_id, will need to create a new device in IoT Hub, and configure it with the ConsoleApp.

We also probably also don’t want to be pinging the GET API’s that frequently, we can use WebSockets for real time updates.
