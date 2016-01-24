# Chatty
Chat application project for the Secure Programmer Minor at Stenden University in the Netherlands

## Features
Chatty offers the following features:

* SSL encryption between client and server
* RSA encryption between clients. The server cannot read the messages.
* Support for groups
* Messages are saved in a database if the recipient of the messages is offline

## Installation
You can download the latest stable release from the [releases](https://github.com/Valentijn1995/Chatty/releases) page. The download contains both the client and server application.

### Installing the server
Steps for getting the server running:

* Install Nodejs from https://nodejs.org/en/
* Download the latest release
* Move ChattyServer directory to a preferred location on your hard drive
* Open a terminal (command prompt in Windows) in the ChattyServer directory and run:
```
 npm install
```
This will install all the dependencies of the ChattyServer

* Open the **options.js** file in the ChattyServer directory and change the options if needed. Pay close attention to the SSL key and certificate instructions.
* Run  ``` node index ``` in the previous opened terminal to launch the server

### Installing the client application
You can run the Chatty client by opening the **Chatty.sln** file with Microsoft Visual Studio. From within Visual Studio you will be able to build and run the Chatty client.

## Known bugs
We experienced communication delays between clients and server durning the development of the program.

## License
[MIT](LICENSE)
