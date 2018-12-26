# .NET Rehosted Workflow Designer #

![Alt text](https://github.com/tebjan/VL.WorkflowFoundation/blob/master/screenshot.png?raw=true "Rehosted Workflow Designer")

The solution contains:

## VL Nodes ##
* StateMachine instance as process node
* SendMessage to trigger statemachine transitions
* State to get info and notifications from a selected state

## Workflow Designer ##
* Workflow Designer - Rehosting in a Form with one tab per workflow
* ToolboxControl - Selected for working with state machines
* Workflow Execution - Retrieve real-time Execution Log (TrackData) and Execution Output(s)
* Workflow Management - Store to file / Run / Stop

## Custom Activities ##
* Wakeup - Waits for a message to trigger transitions

***

## Links ##
* (My presentation at Microsoft Summit 2015) [Introduction to Windows Workflow Foundation](http://www.slideshare.net/orosandrei/windows-workflow-foundation-54773529)
* Blog post about the WF Designer demo &amp; [Windows Workflow Foundation](http://andreioros.com/blog/windows-workflow-foundation-rehosted-designer/)
* Project Showcased at [Microsoft Summit 2015](http://andreioros.com/blog/workflow-foundation-microsoft-summit/#more-92) & [Timisoara .NET Meetup 2](http://www.meetup.com/Timisoara-NET-Meetup/events/186254642/)
* Twitter [@orosandrei](http://twitter.com/orosandrei)

***

* MSDN [Windows Workflow Foundation](http://msdn.microsoft.com/en-us/library/dd489441(v=vs.110).aspx)
* MSDN [What's new in WF 4.5](https://msdn.microsoft.com/en-us/library/hh305677.aspx)
* [Roslyn C# Expression Editor](https://github.com/dmetzgar/wf-rehost-roslyn)
* [Custom Expression Editor](https://blogs.msdn.microsoft.com/cathyk/2009/11/05/implementing-a-custom-expression-editor/)
* [Expression Editing Mechanics](https://blogs.msdn.microsoft.com/cathyk/2009/11/09/expression-editing-mechanics/)
* [Avalon Edit](https://github.com/icsharpcode/AvalonEdit)
