QueueStat
=========

Generate statistics from messages in NServieBus MSMQ queue.

For command line help use:
````
QueueStat.exe -?
````

Sample usage:
````
QueueStat.exe Error
QueueStat.exe Error Error2 Error3
````

Sample output
````
QueueStat.exe error@myserver103.myapp.domain
Queue FormatName:DIRECT=OS:myserver103.myapp.domain\private$\error contains 47 messages

Top 10 grouped by Uri
29      http://mydomain.net/MyNamespace.CommonDomainService.Messages.Commands
17      http://mydomain.net/MyNamespace.ItemComponent.Messages.Commands
1       http://mydomain.net/MyNamespace.DocumentService.Messages.Commands

Top 10 grouped by MessageType
29      http://mydomain.net/MyNamespace.CommonDomainService.Messages.Commands.SendEmailCommand
17      http://mydomain.net/MyNamespace.ItemComponent.Messages.Commands.CreateItemLocalWithInventoryAdjustmentCommand
1       http://mydomain.net/MyNamespace.DocumentService.Messages.Commands.SendAndSubmitExternalTransferCommand

Top 10 grouped by ExceptionType
29      System.Net.Mail.SmtpFailedRecipientException
17      System.NotImplementedException
1       System.InvalidOperationException

Top 10 grouped by ExceptionSource
29      System
17      MyNamespace.myservice
1       MyNamespace.DocumentService

Top 10 grouped by ExceptionMessage
29      Mailbox unavailable. The server response was: Relaying denied to <someuser@somedomain.net>
17      CreateItemLocalWithInventoryAdjustmentCommand not implemented
1       document with id 3550282c-9132-4063-af7c-a36b00ba7287 has no lines

Top 10 grouped by StackTrace
...

Top 10 grouped by ResponseQueue
17      FORMATNAME:DIRECT=OS:myserver103\private$\itemadjustmentservice
17      FORMATNAME:DIRECT=OS:dalbvine012149\private$\myapp.admin.web
8       FORMATNAME:DIRECT=OS:dalbvine012148\private$\myapp.admin.web
4       FORMATNAME:DIRECT=OS:dalbvine012147\private$\myapp.admin.web
1       FORMATNAME:DIRECT=OS:dalbvine012149\private$\myapp.web
````

Tip: Use it to analyze the workers in NServiceBus storage or control queue:
````
QueueStat.exe myservice.distributor.storage@myserver103.myapp.domain
Queue FormatName:DIRECT=OS:myserver103.myapp.domain\private$\myservice.distributor.storage contains 192 messages

Top 10 grouped by ResponseQueue
33      FORMATNAME:DIRECT=OS:myserver112\private$\myservice
27      FORMATNAME:DIRECT=OS:myserver105\private$\myservice
22      FORMATNAME:DIRECT=OS:myserver110\private$\myservice
22      FORMATNAME:DIRECT=OS:myserver104\private$\myservice
22      FORMATNAME:DIRECT=OS:myserver106\private$\myservice
22      FORMATNAME:DIRECT=OS:myserver107\private$\myservice
22      FORMATNAME:DIRECT=OS:myserver111\private$\myservice
11      FORMATNAME:DIRECT=OS:myserver109\private$\myservice
11      FORMATNAME:DIRECT=OS:myserver108\private$\myservice
````