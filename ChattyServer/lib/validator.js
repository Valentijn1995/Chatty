/**
* The validator module contains functions which can be used to check if
* the data of the Chatty clients is valid and contains the right properties.
*
*/

/*
  List of Socket.IO events and their parameters.
*/
var validatorList = {
  'register':['userName', 'publicKey'],
  'message':['receiver', 'message'], //Grouphash is optional
  //Events with only one data type (usualy strings) don't have named
  //properties so their property array is empty
  'user-search':[],
  'user-confirm': [],
  'create-group': ['groupName', 'members']
}

/**
* Checks if the given data set a valid for a certain property.
* @example
*   isValidEventData('register', {userName:'Henk', publicKey:'sdfdrgdfgdsfgdgdfg'}) -> true
*   isValidEventData('register', {userName:'Henk'}) -> false (missing the publicKey property)
*
* @param {string} eventName - The name of the socket.IO event
* @param {object} data - The data retrieved from the client
* @return True wen the given data contains the right properties or false then the data is incomplete
*/
function isValidEventData(eventName, data)
{
  var eventDataProps = validatorList[eventName]
  if(eventDataProps.length === 0)
  {
    return data !== null
  }
  else
  {
    var dataIsValid = eventDataProps.every(function(eventProp)
    {
      return eventProp in data
    })
    return dataIsValid
  }
}

/**
* Creates a string with all the property names of a given event.
* Handy for printing error messages when a client is missing data properties
* @param {string} eventName - The name of the event where you want the properties from
* @return property string is the form: property<space>property<space>...
*/
function getEventDataString(eventName)
{
  var propsString = ''
  validatorList[eventName].forEach(function(eventProp)
  {
    propsString += (eventProp + ' ')
  })
}

module.exports = {isValidEventData, getEventDataString}
