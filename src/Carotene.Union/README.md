# Carotene.Union

```csharp
[Union<Data>]
[Union<Response>]
public readonly partial struct DataEvent { }

public readonly struct Data { }
public readonly struct Response { }

DataEvent dataEvent = new Data { };
var result = dataEvent.Match(
    (in response) => "It's `Response`",
    (in data) => "It's `Data`"
);
```

## Supported types

`class` `record class` `struct` and `record struct`
