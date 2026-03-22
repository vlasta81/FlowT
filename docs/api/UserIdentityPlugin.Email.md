## UserIdentityPlugin\.Email Property

Gets the authenticated user's e\-mail address parsed from [System\.Security\.Claims\.ClaimTypes\.Email](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.email 'System\.Security\.Claims\.ClaimTypes\.Email'),
or `null` when the claim is absent or in a non\-HTTP scenario\.

```csharp
public string? Email { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')