var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var crypto = require('crypto')
var sqlite = require('sqlite3')

var clientArray = []

function getClientByHash(hashString)
{
  var returnClient = false
  clientArray.forEach(function(client)
  {
    if(client.publicKeyHash == hashString)
    {
      returnClient = client
    }
  })
  return returnClient
}

function getClientBySocket(clientSocket)
{
  var returnClient = false
  clientArray.forEach(function(client)
  {
    if(client.socket == clientSocket)
    {
      returnClient = client
    }
  })
  return returnClient
}

function getClientByPublicKey(clientPublicKey)
{
  var returnClient = false
  clientArray.forEach(function(client)
  {
    if(client.publicKey == clientPublicKey)
    {
      returnClient = client
    }
  })
  return returnClient
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
    else if (getClientByPublicKey(regData.publickey) !== false)
    {
      socket.emit('register-failed', 'There is already a user registered with this publickey')
    }
    else
    {
        var shaHash = crypto.createHash('sha1')
        shaHash.update(regData.publicKey)
        var publicKeyHash = shaHash.digest('hex')
        regData.publicKeyHash = publicKeyHash
        regData.socket = socket
        regData.online = true
        clientArray.push(regData)
        socket.emit('register-accepted')
        console.log('New client registered! Name: ' + regData.userName )
    }
  })

  socket.on('disconnect', function()
  {
    var client  = getClientBySocket(socket)
    if(client !== false)
    {
      console.log('User ' + client.userName + ' disconnected')
      var clientIndex = clientArray.indexOf(client)
      if(clientIndex > -1)
      {
        clientArray.splice(clientIndex, 1)
      }
    }
  })

  socket.on('message', function(msgData)
  {
    console.log('Message received')
    var client = getClientBySocket(socket)
    if(client !== false)
    {
      var receiver = getClientByHash(msgData.receiver)
      var msgTimeStamp = new Date.getTime()
      var emitMessage = { sender: client.publicKeyHash, message: msgData.message, timestamp: msgTimeStamp }
      if(receiver !== false)
      {
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
    var results = []
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
    var client = getClientByHash(hashString)
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
      var client = getClientByHash(clientHash)
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
