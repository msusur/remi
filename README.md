ReMi - Release Management Interface

Application was created to help organize, approve, sign off and close releases. It Also helps with gathering all the release related metrics and information, such as site down time, release content, release manual tasks, participants and so on. 

It is Single Page Application with reusable API. System has completely separated frontend, written in AngularJS, Bootstrap, HTML 5, CSS 3, and backend REST webservice, which uses Web API 2.0 and JSON, .NET 4.5, Entity Framework 6.0 with Code First. API architecture is based on CQS pattern, everything in the system is driven by queries, asynchronous commands and events. Backend also communicates with frontend clients using SignalR, to notify about events triggered by other user actions. Thanks to Bootstrap application can be used on mobile devices. Both frontend and backend logic is covered with tests, Jasmine and NUnit. System can communicate with many other APIs like JIRA, ZenDesk, LDAP, Gerrit, Email service and couple more using independent plugins.

[Getting Started with ReMi](https://github.com/wongatech/remi/wiki/Getting-Started)

[Basic User Guide](https://github.com/wongatech/remi/wiki/Basic-User-Guide)
