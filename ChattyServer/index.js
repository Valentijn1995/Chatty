/*
  This this the main file of the Chatty server application.
*/

// node_module imports
var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var crypto = require('crypto')

// lib imports
var ClientList = require('./lib/clientList')
var GroupList = require('./lib/groupList')
var SqliteManager = require('./lib/sqliteManager')

// Global variable definitions
var listenAddress = 'localhost'
var listenPort = 3000
var clientList = new ClientList()
var groupList = new GroupList()
var dbManager = new SqliteManager('messages.db')

/**
 * Creates a sha1 hash of a given string.
 * @param {string} stringToHash - The string to hash
 * @return {string} The hashed version of the string
 */
function createHash(stringToHash)
{
  var shaHash = crypto.createHash('sha1')
  shaHash.update(stringToHash)
  var hash = shaHash.digest('hex')
  return hash
}

/**
 * Creates a timestamp of the current time.
 * @return {number} The current time in milliseconds
 */
function createTimestamp()
{
  return new Date().getTime()
}

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
    // Property check
    if(!('userName' in regData))
    {
      socket.emit('register-failed', 'Register data is missing a userName property')
      return
    }
    else if (!('publicKey' in regData))
    {
      socket.emit('register-failed', 'Register data is missing a publicKey property')
      return
    }

    var client = clientList.getClientByPublicKey(regData.publicKey)

    if (client !== false) // Do we know this client?
    {
      if(client.online == false) //Is the client registered as offline?
      {
        client.online = true
        client.socket = socket
        socket.emit('register-accepted')
        console.log('Client ' + client.userName + ' just came online!')

        //Notify the client of his joined group. Groupdata gets lost when you
        //shutdown the client program so we have to send the group data again
        //when the client reconnects.
        var joinedGroups = groupList.getGroupsOfClient(client)
        joinedGroups.forEach(function(group)
        {
          socket.emit('joined-group', group)
        })

        // Check if the connected client received any messages while he was offline.
        dbManager.getSavedMessages(client.publicKeyHash, function(savedMessages)
        {
          savedMessages.forEach(function(message)
          {
            var isGroupMessage = 'groupHash' in message
            var groupDoesExist = groupList.groupExists(message.groupHash)

            //Only send a the message if it is a group message and the group still exists or
            // when it then the message is a individual message.
            if((isGroupMessage && groupDoesExist) || !isGroupMessage)
            {
              socket.emit('message', message)
            }
          })

          //Delete the saved messages from the messages database
          if(savedMessages.length > 0)
          {
            dbManager.deleteSavedMessages(client.publicKeyHash)
          }
        })
        /*
          We could improve the registration by introducing a handshake mechanism.
          With the current situation, a client can register with a public key
          of another user. This evil client will not be able to read messages
          of the hijacked user because the evil client is not able to decrypt
          the received messages (the evil client needs the private key to do this).
          The evil client can however prevent the real client from registering.

          The solution to this problem is to create a handshake mechanism.
          The handshake mechanism consists of the following steps:
          - Client registers with his public key.
          - Server generates a random message and encrypts this message with the
            given public key of the client.
          - The server sends the generated message back to the Client
          - The client decrypts the message
          - The client sends the message back to the server
          - The server checks if the message from the client is the same as the
            previous generated message
          - The server registers the client when the generated and received messages
            are the same. The client has proven that he is the owner of the public key.

            The identity of the server could best be verified by using SSL.
            The sockets need to make use of SSL so the handshake can not be
            easily observed by another party.

        */
      }
    }
    else //New client
    {
        var publicKeyHash = createHash(regData.publicKey)
        regData.publicKeyHash = publicKeyHash
        regData.socket = socket
        regData.online = true
        clientList.addClient(regData)
        socket.emit('register-accepted')
        console.log('New client registered! Name: ' + regData.userName )
    }
  })

  /*
    This event gets called when a client disconnects from the server
  */
  socket.on('disconnect', function()
  {
    var client  = clientList.getClientBySocket(socket)
    if(client !== false)
    {
      console.log('User ' + client.userName + ' disconnected and is now offline')
      client.online = false
      client.socket = null
    }
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
    var client = clientList.getClientBySocket(socket)
    if(client !== false)
    {
      var receiver = clientList.getClientByHash(msgData.receiver)
      var msgTimeStamp = createTimestamp()
      var emitMessage = { sender: client.publicKeyHash, message: msgData.message, timestamp: msgTimeStamp }
      if('groupHash' in msgData)
      {
        emitMessage.groupHash = msgData.groupHash
      }

      if(receiver !== false)
      {
          if(receiver.online === true) //Send message directly to the receiver if he/she is online.
          {
            console.log("Sending message to " + receiver.userName)
            receiver.socket.emit('message', emitMessage)
          }
          else //Save the message and deliver it when the receiver comes back online.
          {
            console.log("Saving message for offline user " + receiver.userName)
            dbManager.saveMessage(receiver.publicKeyHash, emitMessage)
          }
      }
      else
      {
          console.log("Message received from " + client.userName + " but the receiver is not known")
      }
    }
    else
    {
        console.log("Message received from unregistered client")
    }
  })

  /*
    This event is trigged when the client requests a user-search
  */
  socket.on('user-search', function(searchName)
  {
    // TODO add check if client is a registered user.
    var results = []
    clientList.innerList.forEach(function(client)
    {
      if(client.userName.indexOf(searchName) != -1)
      {
        results.push({ userName: client.userName, publicKeyHash: client.publicKeyHash })
      }
    })
    socket.emit('user-search', results)
  })

  /*
    This event is trigged when a client likes to obtain a public key
    from another client so they can set-up a communication channel
  */
  socket.on('user-confirm', function(hashString)
  {
    // TODO add check if client is a registered user.
    var client = clientList.getClientByHash(hashString)
    if(client !== false)
    {
      socket.emit('user-confirm',  { publicKey: client.publicKey, userName: client.userName })
    }
    else
    {
        console.log("Could not find user with hash:" + hashString)
        socket.emit('user-confirm')
    }
  })

  /*
    This event gets trigged when a client likes to create a new group
  */
  socket.on('create-group', function(groupData)
  {
    var groupHost = clientList.getClientBySocket(socket)
    if(groupHost === false)
    {
      console.log('Non existing client tried to create a group!')
      return
    }
    // Create a unique hash to identify this group.
    var createdGroupHash = createHash(groupHost.publicKeyHash + createTimestamp())
    var memberHashList = groupData.members
    var groupName = groupData.groupName
    var memberList = []
    var memberMessage = []

    // Add the host as a member
    memberList.push(groupHost)
    memberMessage.push({ userName: groupHost.userName, publicKey: groupHost.publicKey })

    //Build-up the list of members.
    memberHashList.forEach(function(clientHash)
    {
      var client = clientList.getClientByHash(clientHash)
      if(client !== false)
      {
        memberList.push(client)
        memberMessage.push({ userName: client.userName, publicKey: client.publicKey })
      }
      else
      {
          console.log("Client with hash '" + clientHash + "' could not be added to group " + groupData.groupName)
      }
    })

    //Create the message which will be send to the groupMembers
    var joinedGroupMessage = { groupName: groupName, groupHash: createdGroupHash, members: memberMessage }
    //Nofity the members of the group
    memberList.forEach(function(member)
    {
      // Notify members who are online. Members who are offline will receive the 'joined-group' event
      // as soon as they come online (and register with the server).
      if(member.online === true)
      {
        member.socket.emit('joined-group', joinedGroupMessage)
      }
    })
    groupList.addGroup(joinedGroupMessage) //Add the group info to the groupList so the group can be found later.
    console.log('New group created with the name ' + groupName)
  })
})

// Let the dbManager create the message table if it does not exists.
// This table is used to store the messages of the offline users.
dbManager.createTableIfNeeded()

//Setup the http server.
http.listen(listenPort, listenAddress, function()
{
  console.log('listening on ' + listenAddress + ':' + listenPort)
})
