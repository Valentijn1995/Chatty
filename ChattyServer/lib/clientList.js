function ClientList()
{
  this.innerList = []
}

ClientList.prototype.addClient = function (newClient)
{
  this.innerList.push(newClient)
};

ClientList.prototype.removeClient = function (clientToRemove)
{
  index = this.innerList.indexOf(clientToRemove)
  if(index > -1)
  {
    this.innerList.splice(index, 1)
  }
};

ClientList.prototype.getClientByHash = function(hashString)
{
  var returnClient = false
  this.innerList.forEach(function(client)
  {
    if(client.publicKeyHash == hashString)
    {
      returnClient = client
    }
  })
  return returnClient
}

ClientList.prototype.getClientsByHash = function(hashArray)
{
  var results = []
  this.innerList.forEach(function(client)
  {
    if(client.publicKeyHash in hashArray)
    {
      results.push(client)
    }
  })
  return results
}

ClientList.prototype.getClientBySocket = function(clientSocket)
{
  var returnClient = false
  this.innerList.forEach(function(client)
  {
    if(client.socket == clientSocket)
    {
      returnClient = client
    }
  })
  return returnClient
}

ClientList.prototype.getClientByPublicKey = function(clientPublicKey)
{
  var returnClient = false
  this.innerList.forEach(function(client)
  {
    if(client.publicKey == clientPublicKey)
    {
      returnClient = client
    }
  })
  return returnClient
}

module.exports = ClientList
