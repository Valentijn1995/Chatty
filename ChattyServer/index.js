var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var crypto = require('crypto')

var ClientList = require('./lib/clientList')
var SqliteManager = require('./lib/sqliteManager')

var listenAddress = '192.168.2.167'
var listenPort = 3000
var clientList = new ClientList()
var dbManager = new SqliteManager('messages.db')

app.get('/', function(req, res)
{
  res.sendFile(__dirname + '/index.html')
});

io.on('connection', function(socket)
{
  console.log('New user connected!')

  socket.on('register', function(regData)
  {
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

    client = clientList.getClientByPublicKey(regData.publicKey)

    if (client !== false)
    {
      if(client.online == false)
      {
        client.online == true
        client.socket = socket
        socket.emit('register-accepted')
        console.log('Client ' + client.userName + ' just came online!')
        dbManager.getSavedMessages(client.publicKeyHash, function(savedMessages)
        {
          savedMessages.forEach(function(message)
          {
            socket.emit('message', message)
          })
        })
        /*
          We could improve the registration by introducing a handshake meganism.
          With the current situation, a client can register with a public key
          of another user. This evil client will not be able to read messages
          of the hijaced user because the evil client is not able to decrypt
          the received messages (the evil client needs the private key to do this).
          The evil client can however prevent the real client from registering.

          The solution to this problem is to create a handshake meganism.
          The handshake meganism consists of the following steps:
          - Client registers with his public key.
          - Server genarates a random message and encrypts this message with the
            given public key of the client.
          - The server send the genarated message back to the Client
          - The client decrypts the message
          - The client sends the message back to the server
          - The server checks if the message from the client is the same as the
            previous genarated message
          - The server registers the client when the genarated and received messages
            are the same. The client has proven that he is the owner of the public key.
        */
      }
    }
    else
    {
        var shaHash = crypto.createHash('sha1')
        shaHash.update(regData.publicKey)
        var publicKeyHash = shaHash.digest('hex')
        regData.publicKeyHash = publicKeyHash
        regData.socket = socket
        regData.online = true
        clientList.addClient(regData)
        socket.emit('register-accepted')
        console.log('New client registered! Name: ' + regData.userName )
    }
  })

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

  socket.on('message', function(msgData)
  {
    console.log('Message received')
    var client = clientList.getClientBySocket(socket)
    if(client !== false)
    {
      var receiver = clientList.getClientByHash(msgData.receiver)
      var msgTimeStamp = new Date.getTime()
      var emitMessage = { sender: client.publicKeyHash, message: msgData.message, timestamp: msgTimeStamp }
      if('groupHash' in msgData)
      {
        emitMessage.groupHash = msgData.groupHash
      }

      if(receiver !== false)
      {
          if(receiver.online === true)
          {
            receiver.socket.emit('message', emitMessage)
          }
          else
          {
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
        console.log("Message received from unregisted client")
    }
  })

  socket.on('user-search', function(searchName)
  {
    var results = []
    clientList.innerList.forEach(function(client)
    {
      if(client.userName.indexof(searchName) != -1)
      {
        results.push({ userName: client.userName, publicKeyHash: client.publicKeyHash })
      }
    })
    socket.emit('user-search', results)
  })

  socket.on('user-confirm', function(hashString)
  {
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

  socket.on('create-group', function(groupData)
  {
    var memberList = []
    var memberMessage = []

    groupData.members.forEach(function(clientHash)
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

    memberList.forEach(function(member)
    {
      member.socket.emit('joined-group', { groupName: groupData.groupName, members: memberMessage })
    })
    console.log('New group created with the name ' + groupData.groupName)
  })
})

dbManager.createTableIfNeeded()

http.listen(listenPort, listenAddress, function()
{
  console.log('listening on ' + listenAddress + ':' + listenPort)
})
