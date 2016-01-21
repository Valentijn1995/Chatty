var sqlite = require('sqlite3')


function SqliteManager(filePath)
{
  this.messageDB = new sqlite.Database(filePath);
  this.messageDBTableName = 'CHATTY_MESSAGES'
}

SqliteManager.prototype.createTableIfNeeded = function()
{
  this.messageDB.run(
    'CREATE TABLE IF NOT EXISTS ' + this.messageDBTableName + ' (sender TEXT, receiver TEXT, message TEXT,\
   grouphash TEXT, timestamp NUMERIC)'
  )
}

SqliteManager.prototype.saveMessage = function(receiverHash, message)
{
  if(!('groupHash' in message))
  {
    message.groupHash = ''
  }

  this.messageDB.run('INSERT INTO ' + this.messageDBTableName + ' (sender, receiver, message, grouphash, timestamp) VALUES(?,?,?,?)',
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

SqliteManager.prototype.deleteSavedMessages = function(publicKeyHash)
{
  this.messageDB.run('DELETE FROM ' + this.messageDBTableName + ' WHERE receiver == (?)', publicKeyHash)
}

module.exports = SqliteManager
