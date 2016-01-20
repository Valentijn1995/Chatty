var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var crypto = require('crypto')
var shaHash = crypto.createHash('sha1')

clientArray = []

function getClientByHash(hashString)
{
  return_client = false
  array.foreach(function(client)
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
  array.foreach(function(client)
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
    if(!('nickName' in regData))
    {
      socket.emit('register-failed', 'Register data is missing a nickName property')
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
    clientArray.remove(client)
  })

  socket.on('message', function(msgData)
  {
    console.log('Message received')
    client = getClientBySocket(socket)
    receiver = getClientByHash(msgData.receiver)
    msgTimeStamp = new Date.getTime()
    emitMessage = { sender: client.publicKeyHash, message: msgData.message, timestamp: msgTimeStamp }
    receiver.emit('message', emitMessage)
  })

  socket.on('user-search', function(searchName)
  {
    results = []
    clientArray.foreach(function(client)
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
    socket.emit('user-confirm',  { publicKey: client.publicKey, userName: client.userName })
  })

  socket.on('create-group', function(groupData)
  {
    memberList = []
    memberMessage = []

    groupData.members.foreach(function(clientHash)
    {
      client = getClientByHash(clientHash)
      memberList.push(client)
      memberMessage.push({ userName: client.userName, publicKey: client.publicKey })
    })

    memberList.foreach(function(member)
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
