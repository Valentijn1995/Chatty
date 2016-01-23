var sqlite = require('sqlite3')

/**
* The SqliteManager is responsible for saving, deleting and retrieving messages.
* Chatty saves the messages of offline users in a sqlite database. The saved
* messages of a user will be retrieved when the user comes online.
*
*/
function SqliteManager(filePath)
{
  this.messageDB = new sqlite.Database(filePath);
  this.messageDBTableName = 'CHATTY_MESSAGES'
}

/**
* Creates a messages tables in the sqlite database. This function does nothing
* when a table already exists in the sqlite database.
*
*/
SqliteManager.prototype.createTableIfNeeded = function()
{
  this.messageDB.run(
    'CREATE TABLE IF NOT EXISTS ' + this.messageDBTableName + ' (sender TEXT, receiver TEXT, message TEXT,\
   grouphash TEXT, timestamp NUMERIC)'
  )
}

/**
* Saves a message in the database.
* @param {string} receiverHash - The publicKeyHash of the receiver of the message
* @param {string} message - The message
*/
SqliteManager.prototype.saveMessage = function(receiverHash, message)
{
  if(!('groupHash' in message))
  {
    message.groupHash = ''
  }

  this.messageDB.run('INSERT INTO ' + this.messageDBTableName + ' (sender, receiver, message, grouphash, timestamp) VALUES(?,?,?,?,?)',
  [message.sender,
  receiverHash,
  message.message,
  message.groupHash,
  message.timestamp])
}

SqliteManager.prototype.getSavedMessages = function(publicKeyHash, completeCallback)
{
  savedMessages = []
  this.messageDB.each('SELECT sender, message, grouphash, timestamp FROM ' + this.messageDBTableName +
  ' WHERE receiver == (?)', publicKeyHash, function(error, message)
  {
    if(message.grouphash === "")
    {
      delete message.groupHash
    }
    savedMessages.push(message)
  }, function()
  {
    completeCallback(savedMessages)
  })
}

/**
* Delete the messages of a specific client. This function gets usually call after the getSavedMessages function.
* @param {string} publicKeyHash - The publicKeyHash of the client
*/
SqliteManager.prototype.deleteSavedMessages = function(publicKeyHash)
{
  this.messageDB.run('DELETE FROM ' + this.messageDBTableName + ' WHERE receiver == (?)', publicKeyHash)
}

module.exports = SqliteManager
