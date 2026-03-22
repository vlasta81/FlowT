## IUserIdentityPlugin\.UserId Property

Gets the authenticated user's ID parsed from [System\.Security\.Claims\.ClaimTypes\.NameIdentifier](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.nameidentifier 'System\.Security\.Claims\.ClaimTypes\.NameIdentifier'),
or `null` when unauthenticated or in a non\-HTTP scenario\.

```csharp
System.Nullable<System.Guid> UserId { get; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Guid](https://learn.microsoft.com/en-us/dotnet/api/system.guid 'System\.Guid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')