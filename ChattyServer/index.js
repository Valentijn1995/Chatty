var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var crypto = require('crypto')
var shaHash = crypto.createHash('sha1')

clientArray = []

function getClientByHash(hashString)
{
  return_client = false
  clientArray.forEach(function(client)
  {
    if(client.publicKeyHash == hashString)
    {
      return_client = client
    }
  })
  return return_client
}

function getClientBySocket(client_socket)
{
  return_client = false
  clientArray.forEach(function(client)
  {
    if(client.socket == client_socket)
    {
      return_client = client
    }
  })
  return return_client
}

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
    }
    else if (!('publicKey' in regData))
    {
      socket.emit('register-failed', 'Register data is missing a publicKey property')
    }
    else
    {
        shaHash.update(regData.publicKey)
        publicKeyHash = shaHash.digest('hex')
        regData.publicKeyHash = publicKeyHash
        regData.socket = socket
        clientArray.push(regData)
        socket.emit('register-accepted')
        console.log('New client registered! Name: ' + regData.userName )
    }
  })

  socket.on('disconnect', function()
  {
    client  = getClientBySocket(socket)
    console.log('User ' + client.userName + ' disconnected')
    clientIndex = clientArray.indexof(client)
    if(clientIndex > -1)
    {
      clientArray.splice(clientIndex, 1)
    }
  })

  socket.on('message', function(msgData)
  {
    console.log('Message received')
    client = getClientBySocket(socket)
    if(client !== false)
    {
      receiver = getClientByHash(msgData.receiver)
      if(receiver !== false)
      {
        msgTimeStamp = new Date.getTime()
        emitMessage = { sender: client.publicKeyHash, message: msgData.message, timestamp: msgTimeStamp }
        receiver.emit('message', emitMessage)
      }
      else
      {
          console.log("Message received form " + client.userName + " but the receiver is not known")
      }
    }
    else
    {
        console.log("Message received from unregisted client")
    }
  })

  socket.on('user-search', function(searchName)
  {
    results = []
    clientArray.forEach(function(client)
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
    client = getClientByHash(hashString)
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
    memberList = []
    memberMessage = []

    groupData.members.forEach(function(clientHash)
    {
      client = getClientByHash(clientHash)
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

http.listen(3000,'localhost', function()
{
  console.log('listening on localhost:3000')
})
