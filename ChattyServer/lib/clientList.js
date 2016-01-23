/**
 * The ClientList class is responsible for managing the clients of the ChattyServer.
 * A client object may look like this:
 *  {userName:'Henk', socket:<Socket_Object>, online: true, publicKeyHash: 'sdfhdrdfgesth', publicKey: 'skfwehrikffbvbdflnfxbvsdfgnkjbdsf'}
 */
function ClientList()
{
  this.innerList = []
}

/**
* Add a client to the clientList.
* @param {object} newClient - The new client
*/
ClientList.prototype.addClient = function (newClient)
{
  this.innerList.push(newClient)
}

/**
 * Remove a Client from the clientList.
 * @param {object} clientToRemove - The client to remove
 */
ClientList.prototype.removeClient = function (clientToRemove)
{
  index = this.innerList.indexOf(clientToRemove)
  if(index > -1)
  {
    this.innerList.splice(index, 1)
  }
}

/**
 * Checks if a client exists.
 * @param {Socket} socket - The socket of the client
 * @return {boolean} True if the client exists and false if not
 */
ClientList.prototype.clientExists = function(socket)
{
  return this.innerList.some(function(client)
  {
    return client.socket === socket
  })
}

/**
 * Get a client by its publicKeyHash.
 * @param {string} hashString - The publicKeyHash of the client
 * @return {object|boolean} The client object or false if the client was not found
 */
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

/**
 * Get multiple clients by their publicKeyHashes.
 * @param {array} hashArray - Array with publicKeyHashes
 * @return {array} Array with found clients
 */
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

/**
 * Get a client by its socket.
 * @param {Socket} socket - Socket object of the client
 * @return {object|boolean}
 */
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

/**
 * Get a client by its publicKey.
 * @param {string} clientPublicKey - The publicKey of the client
 * @return {object|boolean} A client object or false if no client was found
 */
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
