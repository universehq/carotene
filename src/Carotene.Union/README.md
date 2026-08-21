# Carotene.Union

```csharp
[Union<Data>]
[Union<Response>]
public readonly partial struct DataEvent { }

public readonly struct Data { }
public readonly struct Response { }

DataEvent dataEvent = new Data { };
var result = dataEvent.Match(
    data => data with { },
    response => response with { }
);
```

## Supported types

`class` `record class` `struct` and `record struct`
