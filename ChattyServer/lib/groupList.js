/**
* The GroupList class is used to manage the groups on the Chatty server
* The structure of a group object may look as follows:
*   {groupName: 'My special group', groupHash: 'sadfsfefgdfgf', members: [{userName: 'Piet', publicKey: 'safrtfgdsgddsdfsd'},...]}
*
*/
function GroupList()
{
  this.innerList = []
}

/**
* Adds a group to the GroupList
* @param {object} group - The group object to add to the GroupList
*/
GroupList.prototype.addGroup = function(group)
{
  this.innerList.push(group)
}

/**
* Removes a group from the GroupList
* @param {object} group - The group to remove
*/
GroupList.prototype.removeGroup = function(group)
{
  var index = this.innerList.indexOf(group)
  if(index > -1)
  {
    this.innerList.splice(index, 1)
  }
}

/**
* Gets a group by its unique groupHash
* @param {string} groupHash - The hash of the group
* @return {object|boolean} The group with the given groupHash or false if no group was found
*/
GroupList.prototype.getGroupByHash = function(groupHash)
{
  returnGroup = false
  this.innerList.forEach(function(group)
  {
    if(group.groupHash === groupHash)
    {
      returnGroup = group
    }
  })
  return returnGroup
}

/**
* Get all the groups where a client belongs to.
* @param {object} client - The client where you want all the groups from
* @return {array} An array with all the groups where the client is a member off
*/
GroupList.prototype.getGroupsOfClient = function(client)
{
  groupsOfClient = []
  this.innerList.forEach(function(group)
  {
    group.members.forEach(function(member)
    {
      if(member.publicKey === client.publicKey)
      {
        groupsOfClient.push(group)
      }
    })
  })
  return groupsOfClient
}

/**
* Checks if a group exists.
* @param {string} groupHash - The hash of the group
* @return {boolean} True if a group exists and false if it does not exists
*
*/
GroupList.prototype.groupExists = function(groupHash)
{
  var groupExists = this.innerList.some(function(group)
  {
    return group.groupHash === groupHash
  })
  return groupExists
}

module.exports = GroupList
