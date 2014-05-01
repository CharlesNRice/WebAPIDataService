Moving from WCF Data Services to Web API 2
==============

Current Supported features
--------------

- Querying entity sets
- Drilling into Navigation Properties
- Selecting Single Entity
- Selecting Property on the Entity


Missing or incomplete
--------------

- Don't handle complex types
- Don't handle batch queries
- Only handle querying 
- ToDo in the code where I know I need to clean up or haven't tested yet
- Haven't tested without entity framework
- No unit test for not code first (tested by hand but didn't port into unit test project yet)


The unit test are written using MS Test and use LocalDb for the code first support of entity framework.  Should be able to project and select whatever Web API supports.   Like to thank ProcessPro (www.processproerp.com) because I work for them and we use this code internal but they allowed me to share with everyone.

There is more information at www.nhail.com (my personal blog). 

Thanks

Charles Rice
