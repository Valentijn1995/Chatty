function GroupList()
{
  this.innerList = []
}

GroupList.prototype.addGroup = function(group)
{
  this.innerList.push(group)
}

GroupList.prototype.removeGroup = function(group)
{
  var index = this.innerList.indexOf(group)
  if(index > -1)
  {
    this.innerList.splice(index, 1)
  }
}

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

GroupList.prototype.getGroupsOfClient = function(client)
{
  groupsOfClient = []
  this.innerList.forEach(function(group)
  {
    group.member.forEach(function(member)
    {
      if(member.publicKey === client.publicKey)
      {
        groupsOfClient.push(group)
      }
    })
  })
  return groupsOfClient
}

GroupList.prototype.groupExists = function(groupHash)
{
  var groupExists = this.innerList.some(function(group)
  {
    return group.groupHash === groupHash
  })
  return groupExists
}

module.exports = GroupList
