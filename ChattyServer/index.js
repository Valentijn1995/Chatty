/*
  This this the main file of the Chatty server application.

  Possible improvements:
    - Better handshake between server and client (see handleRegistration function in ./lib/eventsHandler.js)
    - Performance improvements, The GroupList and ClientList use the forEach method to iterate over the list.
          The problem of the forEach approach is that you cannot easily stop the loop when you found what you searched for.
          Some of the forEach methods where replaced by the 'some' and 'every' methods to stop the loop when needed.
          Performance could be improved further by replacing the forEach methods with normal for loops.
          Performance could also be improved by keeping a reference to the client object in the 'io.on('connection')'' scope
          so each events do not have to use the clientList.getClientBySocket() function to get the current client.
    - The Socket.IO event functions all contain the same if-else statement. Maybe this could be refactored to a more cleaner solution.

  Known problems:
    - We experienced some delay problems in het Socket.IO communication. The time difference between the connection and register events sometimes took about 10 seconds.
      This problem could be caused by the Socket.IO version differences between client and server.
*/

var chattyOptions = require('./options')
// node_module imports
var app = require('express')()
var webServer
if(chattyOptions.enableSSL === true)
{
  var fs = require('fs')
  var httpsServerOptions = {
    key: fs.readFileSync(chattyOptions.SSLKeyPath),
    cert: fs.readFileSync(chattyOptions.SSLCrtPath)
  };
  webServer = require('https').createServer(httpsServerOptions, app);
}
else
{
  http = require('http').Server(app)
}
var io = require('socket.io')(webServer)

//Lib imports
var EventsHandler = require('./lib/eventsHandler')
var ClientList = require('./lib/clientList')
var GroupList = require('./lib/groupList')
var SqliteManager = require('./lib/sqliteManager')
var validator = require('./lib/validator')

// Global variable definitions
var listenAddress = chattyOptions.listenAddress
var listenPort = chattyOptions.listenPort
var clientList = new ClientList()
var groupList = new GroupList()
var dbManager = new SqliteManager(chattyOptions.msgDBName)
var eventsHandler = new EventsHandler(clientList, groupList, dbManager)
/*
  Supplies the index.html file when a user browses to http://<listenAddress>:<listenPort>
*/
app.get('/', function(req, res)
{
  res.sendFile(__dirname + '/index.html')
});

/*
  Definitions for Socket.IO
*/
io.on('connection', function(socket)
{
  console.log('New user connected!')

  /*
    The register event gets called short after the connection event.
    This event is used to register the connected Chatty client.
  */
  socket.on('register', function(regData)
  {
    if(validator.isValidEventData('register', regData))
    {
      eventsHandler.handleRegistration(socket, regData)
    }
    else
    {
        console.log('A client tried to register with invalid register data. Properties need to be ' +
                  validator.getEventDataString('register') + '. Saw ' + regData)
    }
  })

  /*
    This event gets called when a client disconnects from the server
  */
  socket.on('disconnect', function()
  {
    eventsHandler.handleDisconnect(socket)
  })

  /*
    The event gets trigged when a client sends a chat message to the server.
    The server will read the message properties and send the message the the
    recipient of temporary saves the messages if the recipient is offline.
    The recipient will receive the message then he/she gets back online.
  */
  socket.on('message', function(msgData)
  {
    console.log('Message received')
    if(clientList.clientExists(socket) === false)
    {
      console.log('Unregistered user tried to send a message')
    }
    else if(validator.isValidEventData('message', msgData))
    {
      eventsHandler.handleMessage(socket, msgData)
    }
    else
    {
      console.log('A client tried to send a message with invalid message data. Properties need to be ' +
                validator.getEventDataString('message') + '. Saw ' + msgData)
    }
  })

  /*
    This event is trigged when the client requests a user-search
  */
  socket.on('user-search', function(searchName)
  {
    if(clientList.clientExists(socket) === false)
    {
      console.log('Unregistered client tried to do a user search')
    }
    else if (validator.isValidEventData('user-search', searchName))
    {
      eventsHandler.handleUserSearch(socket, searchName)
    }
    else
    {
      console.log('A client tried to do a user search without providing a search name')
    }
  })

  /*
    This event is trigged when a client likes to obtain a public key
    from another client so they can set-up a communication channel
  */
  socket.on('user-confirm', function(hashString)
  {
    if(clientList.clientExists(socket) === false)
    {
      console.log('Unregistered client tried to get the public key of a user')
    }
    else if (validator.isValidEventData('user-confirm', hashString))
    {
      eventsHandler.handleUserConfirm(socket, hashString)
    }
    else
    {
      console.log('A client tried to do a get the public key of a user without providing a userHash')
    }
  })

  /*
    This event gets trigged when a client likes to create a new group
  */
  socket.on('create-group', function(groupData)
  {
    if(clientList.clientExists(socket) === false)
    {
      console.log('Unregistered client tried create a group')
    }
    else if (validator.isValidEventData('create-group', groupData))
    {
      eventsHandler.handleCreateGroup(socket, groupData)
    }
    else
    {
      console.log('A client tried to create a group with invalid group data. Properties need to be ' +
                validator.getEventDataString('create-group') + '. Saw ' + groupData)
    }
  })
})

// Let the dbManager create the message table if it does not exists.
// This table is used to store the messages of the offline users.
dbManager.createTableIfNeeded()

//Setup the http server.
webServer.listen(listenPort, listenAddress, function()
{
  console.log('Options: ')
  console.log()
  console.log(chattyOptions)
  console.log()
  console.log('Chatty server is active')
})
