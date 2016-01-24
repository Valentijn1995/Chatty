# Chatty
Chat application project for the Secure Programmer Minor at Stenden University in the Netherlands

## Features
Chatty offers the following features:

* SSL encryption between client and server
* RSA encryption between clients. The server cannot read the messages.
* Support for groups
* Messages are saved in a database if the recipient of the messages is offline

## Installation
You can download the latest stable release from the master branch. The client and server branch are development branches.

### Installing the server
Steps for getting the server running:

* Install Nodejs from https://nodejs.org/en/
* Download the ChattyServer directory from the master branch and move it to a preferred location on your hard drive
* Open a terminal (command prompt in Windows) in the ChattyServer directory and run:
```
 npm install
```
This will install all the dependencies of the ChattyServer

* Open the **options.js** file in the ChattyServer directory and change the options if needed. Pay close attention to the SSL key and certificate instructions.
* Run  ``` node index ``` in the previous opened terminal to launch the server

### Installing the client application
You can install the Chatty client by downloading the Chatty directory and Chatty.sln file from the master branch. Open de **Chatty.sln** file with Microsoft Visual Studio so you can build and run the client.

## Known bugs
We experienced communication delays between clients and server durning the development of the program. 

## License
[MIT](LICENCE)
