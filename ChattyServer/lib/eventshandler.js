var crypto = require('crypto')

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
/**
 * The EventsHandler is responsible for handling the Socket.IO events.
 * @param {ClientList} clientList - The clientList to use for managing clients
 * @param {GroupList} groupList - The groupList to use for managing groups
 * @param {SqliteManager} dbManager - The dbManager to use for saving messages
 */
function EventsHandler(clientList, groupList, dbManager)
{
  this.clientList = clientList
  this.groupList = groupList
  this.dbManager = dbManager
}

EventsHandler.prototype.handleRegistration = function(socket, regData)
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

  var client = this.clientList.getClientByPublicKey(regData.publicKey)

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
      var joinedGroups = this.groupList.getGroupsOfClient(client)
      joinedGroups.forEach(function(group)
      {
        socket.emit('joined-group', group)
      })

      // Create a reference to this eventshandler so we can access the handler inside the callback function.
      var handerRef = this
      // Check if the connected client received any messages while he was offline.
      this.dbManager.getSavedMessages(client.publicKeyHash, function(savedMessages)
      {
        savedMessages.forEach(function(message)
        {
          var isGroupMessage = 'groupHash' in message

          //Only send a the message if it is a group message and the group still exists or
          // when it then the message is a individual message.
          if((isGroupMessage && handerRef.groupList.groupExists(message.groupHash)) || !isGroupMessage)
          {
            socket.emit('message', message)
          }
        })

        //Delete the saved messages from the messages database
        if(savedMessages.length > 0)
        {
          handerRef.dbManager.deleteSavedMessages(client.publicKeyHash)
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
      this.clientList.addClient(regData)
      socket.emit('register-accepted')
      console.log('New client registered! Name: ' + regData.userName )
  }
}

EventsHandler.prototype.handleDisconnect = function(socket)
{
  var client  = this.clientList.getClientBySocket(socket)
  if(client !== false)
  {
    console.log('User ' + client.userName + ' disconnected and is now offline')
    client.online = false
    client.socket = null
  }
}

EventsHandler.prototype.handleMessage = function(socket, msgData)
{
  var client = this.clientList.getClientBySocket(socket)
  if(client !== false)
  {
    var receiver = this.clientList.getClientByHash(msgData.receiver)
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
          console.log(emitMessage)
          receiver.socket.emit('message', emitMessage)
        }
        else //Save the message and deliver it when the receiver comes back online.
        {
          console.log("Saving message for offline user " + receiver.userName)
          this.dbManager.saveMessage(receiver.publicKeyHash, emitMessage)
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
}

EventsHandler.prototype.handleUserSearch = function(socket, searchName)
{
  var results = []
  this.clientList.innerList.forEach(function(client)
  {
    if(client.userName.indexOf(searchName) != -1)
    {
      results.push({ userName: client.userName, publicKeyHash: client.publicKeyHash })
    }
  })
  socket.emit('user-search', results)
}

EventsHandler.prototype.handleUserConfirm = function(socket, hashString)
{
  var client = this.clientList.getClientByHash(hashString)
  if(client !== false)
  {
    socket.emit('user-confirm',  { publicKey: client.publicKey, userName: client.userName })
  }
  else
  {
      console.log("Could not find user with hash:" + hashString)
      socket.emit('user-confirm')
  }
}

EventsHandler.prototype.handleCreateGroup = function(socket, groupData)
{
  var groupHost = this.clientList.getClientBySocket(socket)
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

  var handerRef = this
  //Build-up the list of members.
  memberHashList.forEach(function(clientHash)
  {
    var client = handerRef.clientList.getClientByHash(clientHash)
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
  this.groupList.addGroup(joinedGroupMessage) //Add the group info to the groupList so the group can be found later.
  console.log('New group created with the name ' + groupName)
}

module.exports = EventsHandler
