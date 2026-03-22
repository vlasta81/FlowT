## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
FlowT001 | FlowT.ThreadSafety | Warning | Mutable instance field in singleton flow component
FlowT002 | FlowT.ThreadSafety | Error | Non-thread-safe collection in singleton flow component
FlowT003 | FlowT.DependencyInjection | Error | Captive scoped dependency in singleton flow component
FlowT004 | FlowT.ThreadSafety | Error | Static mutable state in flow component
FlowT005 | FlowT.AsyncPatterns | Warning | Async void method in flow component
FlowT006 | FlowT.ThreadSafety | Error | FlowContext stored in field
FlowT007 | FlowT.ThreadSafety | Error | Request or Response object stored in field
FlowT008 | FlowT.ThreadSafety | Warning | Lock on this or typeof(T)
FlowT009 | FlowT.AsyncPatterns | Info | Missing CancellationToken propagation
FlowT010 | FlowT.AsyncPatterns | Warning | Synchronous blocking call or Thread.Sleep in async method
FlowT011 | FlowT.FlowConfiguration | Error | Missing Handle call in FlowDefinition.Configure
FlowT012 | FlowT.DependencyInjection | Error | IServiceProvider stored in field
FlowT013 | Threading | Error | CancellationTokenSource stored in field
FlowT014 | FlowT.ErrorHandling | Info | Empty catch block
FlowT015 | Threading | Error | Mutable public or internal property in flow component
FlowT016 | Threading | Warning | Task or ValueTask stored in field
FlowT017 | Threading | Warning | Manual Thread creation in flow component
FlowT018 | Threading | Error | Lazy without thread-safety mode
FlowT019 | FlowT.ThreadSafety | Error | State leak type stored in field
FlowT020 | FlowT.AsyncAwait | Warning | ConfigureAwait false loses HttpContext or FlowContext
FlowT021 | FlowT.ThreadSafety | Error | FlowPlugin stored in singleton field
FlowT022 | FlowT.FlowConfiguration | Error | Multiple Handle calls in FlowDefinition.Configure
FlowT023 | FlowT.BestPractices | Warning | Direct HttpClient instantiation
FlowT024 | FlowT.AsyncPatterns | Warning | Synchronous file I/O in async flow method
FlowT025 | FlowT.BestPractices | Info | Direct IServiceProvider access
