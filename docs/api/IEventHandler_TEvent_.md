## IEventHandler\<TEvent\> Interface

Represents a handler for domain events published via [PublishAsync&lt;TEvent&gt;\(TEvent, CancellationToken\)](FlowContext.PublishAsync.58535BP5AHRSH68ZVN80J8OS4.md 'FlowT\.FlowContext\.PublishAsync\<TEvent\>\(TEvent, System\.Threading\.CancellationToken\)')\.
Multiple handlers can be registered for the same event type\.

```csharp
public interface IEventHandler<in TEvent>
```
#### Type parameters

<a name='FlowT.Contracts.IEventHandler_TEvent_.TEvent'></a>

`TEvent`

The type of event this handler processes\.

| Methods | |
| :--- | :--- |
| [HandleAsync\(TEvent, CancellationToken\)](IEventHandler_TEvent_.HandleAsync.QGU5J0YMBY6L5QAEL753CRBJ4.md 'FlowT\.Contracts\.IEventHandler\<TEvent\>\.HandleAsync\(TEvent, System\.Threading\.CancellationToken\)') | Handles the event asynchronously\. |
