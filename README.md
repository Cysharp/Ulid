Ulid
===
[![GitHub Actions](https://github.com/Cysharp/Ulid/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/Ulid/actions) [![Releases](https://img.shields.io/github/release/Cysharp/Ulid.svg)](https://github.com/Cysharp/Ulid/releases)

Fast C# Implementation of [ULID](https://github.com/ulid/spec) for .NET Core and Unity. Ulid is sortable, random id generator. This project focuses on performance, using the fastest binary serializer([MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp/)) technology. It achieves faster generation than Guid.NewGuid.

![image](https://user-images.githubusercontent.com/46207/55129636-266c0d00-515b-11e9-85ab-3437de539451.png)

NuGet: [Ulid](https://www.nuget.org/packages/Ulid) or download .unitypackage from [Ulid/Releases](https://github.com/Cysharp/Ulid/releases) page.

```
Install-Package Ulid
```

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [How to use](#how-to-use)
- [Performance](#performance)
- [Cli](#cli)
- [Integrate](#integrate)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

How to use
---
Similar api to Guid.

* `Ulid.NewUlid()`
* `Ulid.Parse()`
* `Ulid.TryParse()`
* `new Ulid()`
* `.ToString()`
* `.ToByteArray()`
* `.TryWriteBytes()`
* `.TryWriteStringify()`
* `.ToBase64()`
* `.Time`
* `.Random`

Allows conversion to Guid.

* `.ToGuid()`
* `(Guid)ulid`

Performance
---
`Guid` is standard corelib guid. `Ulid` is this library. `NUlid` is competitor [RobThree/NUlid](https://github.com/RobThree/NUlid).

* New

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  73.13 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  65.41 ns |    NA |  0.89 |           - |           - |           - |                   - |
| NUlid_ | 209.89 ns |    NA |  2.87 |      0.0162 |           - |           - |               104 B |

`Ulid.NewUlid()` is faster than `Guid.NewGuid()` and zero allocation.

* Parse

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ | 197.98 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  28.67 ns |    NA |  0.14 |           - |           - |           - |                   - |
| NUlid_ | 161.03 ns |    NA |  0.81 |      0.0441 |           - |           - |               280 B |

from string(Base32) to ulid, `Ulid.Parse(string)` is fastest and zero allocation.

* ToString

| Method |     Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |---------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ | 57.73 ns |    NA |  1.00 |      0.0163 |           - |           - |               104 B |
|  Ulid_ | 38.77 ns |    NA |  0.67 |      0.0126 |           - |           - |                80 B |
| NUlid_ | 96.76 ns |    NA |  1.68 |      0.0583 |           - |           - |               368 B |

to string representation(Base32), `Ulid.ToString()` is fastest and less allocation.

* NewId().ToString()

| Method |     Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |---------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ | 205.7 ns |    NA |  1.00 |      0.0162 |           - |           - |               104 B |
|  Ulid_ | 110.2 ns |    NA |  0.54 |      0.0125 |           - |           - |                80 B |
| NUlid_ | 301.7 ns |    NA |  1.47 |      0.0744 |           - |           - |               472 B |

In case of get the string representation immediately, `Ulid` is twice faster than Guid.

* GetHashCode

| Method |       Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |-----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  0.9706 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  1.0329 ns |    NA |  1.06 |           - |           - |           - |                   - |
| NUlid_ | 20.6175 ns |    NA | 21.24 |      0.0063 |           - |           - |                40 B |

GetHashCode is called when using dictionary's key. `Ulid` is fast and zero allocation.

* Equals

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  1.819 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  2.023 ns |    NA |  1.11 |           - |           - |           - |                   - |
| NUlid_ | 29.875 ns |    NA | 16.43 |      0.0126 |           - |           - |                80 B |

Equals is called when using dictionary's key. `Ulid` is fast and zero allocation.

* CompareTo

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  5.409 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  3.838 ns |    NA |  0.71 |           - |           - |           - |                   - |
| NUlid_ | 17.126 ns |    NA |  3.17 |      0.0063 |           - |           - |                40 B |

CompareTo is called when sorting. `Ulid` is fastest and zero allocation.

Cli
---
You can install command-line to generate ulid string by  .NET Core Global Tool.

`dotnet tool install --global ulid-cli`

after installation, you can call it like this.

```
$ dotnet ulid
01D7CB31YQKCJPY9FDTN2WTAFF

$ dotnet ulid -t "2019/03/25" -min
01D6R3EBC00000000000000000

$ dotnet ulid -t "2019/03/25" -max
01D6R3EBC0ZZZZZZZZZZZZZZZZ

$ dotnet ulid -t "2019/03/25" -max -base64
AWmwNy2A/////////////w==
```

```
Arguments:
-t, -timestamp: [default=null]timestamp(converted to UTC, ISO8601 format recommended)
-r, -randomness: [default=null]randomness bytes(formatted as Base32, must be 16 characters, case insensitive)
-b, -base64: [default=False]output as base64 format, or output base32 if false
-min, -minRandomness: [default=False]min-randomness(use 000...)
-max, -maxRandomness: [default=False]max-randomness(use ZZZ...)
```

This CLI tool is powered by [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework/).

Integrate
---
**System.Text.Json**

NuGet: [Ulid.SystemTextJson](https://www.nuget.org/packages/Ulid.SystemTextJson)

You can use custom Ulid converter - `Cysharp.Serialization.Json.UlidJsonConverter`.

```csharp
var options = new JsonSerializerOptions()
{
    Converters =
    {
        new Cysharp.Serialization.Json.UlidJsonConverter()
    }
};

JsonSerializer.Serialize(Ulid.NewUlid(), options);
```

If the application target framework is `netcoreapp3.0`, converter is built-in, does not require to add `Ulid.SystemTextJson` package, and does not require custom options.

**MessagePack-CSharp**

NuGet: [Ulid.MessagePack](https://www.nuget.org/packages/Ulid.MessagePack)

You can use custom Ulid formatter - `Cysharp.Serialization.MessagePack.UlidMessagePackFormatter` and resolver - `Cysharp.Serialization.MessagePack.UlidMessagePackResolver`.

```csharp
var resolver = MessagePack.Resolvers.CompositeResolver.Create(
    Cysharp.Serialization.MessagePack.UlidMessagePackResolver.Instance,
    MessagePack.Resolvers.StandardResolver.Instance);
var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

MessagePackSerializer.Serialize(Ulid.NewUlid(), options);
```

If you want to use this custom formatter on Unity, download [UlidMessagePackFormatter.cs](https://github.com/Cysharp/Ulid/blob/master/src/Ulid.MessagePack/UlidMessagePackFormatter.cs).

**Dapper**

For [Dapper](https://github.com/StackExchange/Dapper) or other ADO.NET database mapper, register custom converter from Ulid to binary or Ulid to string.

```csharp
public class BinaryUlidHandler : TypeHandler<Ulid>
{
    public override Ulid Parse(object value)
    {
        return new Ulid((byte[])value);
    }

    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.Binary;
        parameter.Size = 16;
        parameter.Value = value.ToByteArray();
    }
}

public class StringUlidHandler : TypeHandler<Ulid>
{
    public override Ulid Parse(object value)
    {
        return Ulid.Parse((string)value);
    }

    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.StringFixedLength;
        parameter.Size = 26;
        parameter.Value = value.ToString();
    }
}

// setup handler
Dapper.SqlMapper.AddTypeHandler(new BinaryUlidHandler());
```

**Entity Framework Core**

To use it in EF, create a ValueConverter and bind it.

```csharp
public class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 16);

    public UlidToBytesConverter() : this(null)
    {
    }
    
    public UlidToBytesConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x.ToByteArray(),
                convertFromProviderExpression: x => new Ulid(x),
                mappingHints: DefaultHints.With(mappingHints))
    {
    }
}

public class UlidToStringConverter : ValueConverter<Ulid, string>
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 26);

    public UlidToStringConverter() : this(null)
    {
    }

    public UlidToStringConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x.ToString(),
                convertFromProviderExpression: x => Ulid.Parse(x),
                mappingHints: DefaultHints.With(mappingHints))
    {
    }
}
```

To use those converters, you can either specify individual properties of entities in `OnModelCreating` method of your context:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MyEntity>()
        .Property(e => e.MyProperty)
        .HasConversion<UlidToStringConverter>()
        .HasConversion<UlidToBytesConverter>();
}
```

or use model bulk configuration for all properties of type `Ulid`. To do this, overload `ConfigureConventions` method of your context:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder
        .Properties<Ulid>()
        .HaveConversion<UlidToStringConverter>()
        .HaveConversion<UlidToBytesConverter>();
}
```

License
---
This library is under the MIT License.
