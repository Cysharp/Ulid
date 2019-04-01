Ulid
===
[![CircleCI](https://circleci.com/gh/Cysharp/Ulid.svg?style=svg)](https://circleci.com/gh/Cysharp/Ulid)

Fast .NET Standard(C#) Implementation of [ULID](https://github.com/ulid/spec). Ulid is sortable, random id generator. This project aims performance by fastest binary serializer([MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp/)) technology. It achives faster generate than Guid.NewGuid.

![image](https://user-images.githubusercontent.com/46207/55129636-266c0d00-515b-11e9-85ab-3437de539451.png)

Documantation is not yet but core library is completed.

NuGet: [Ulid](https://www.nuget.org/packages/Ulid)

```
Install-Package Ulid
```

How to use
---
TODO:
* `Ulid.NewUlid()`
* `Ulid.Parse()`
* `new Ulid()`
* `.ToString()`
* `.ToByteArray()`
* `.TryWriteBytes()`
* `.ToBase64()`
* `.Time`
* `.Random`

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

case of get the string representation immediately, `Ulid` is twice faster than Guid.

* GetHashCode

| Method |       Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |-----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  0.9706 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  1.0329 ns |    NA |  1.06 |           - |           - |           - |                   - |
| NUlid_ | 20.6175 ns |    NA | 21.24 |      0.0063 |           - |           - |                40 B |

GetHashCode is called when use dictionary's key. `Ulid` is fast and zero allocation.

* Equals

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  1.819 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  2.023 ns |    NA |  1.11 |           - |           - |           - |                   - |
| NUlid_ | 29.875 ns |    NA | 16.43 |      0.0126 |           - |           - |                80 B |

Equals is called when use dictionary's key. `Ulid` is fast and zero allocation.

* CompareTo

| Method |      Mean | Error | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------- |----------:|------:|------:|------------:|------------:|------------:|--------------------:|
|  Guid_ |  5.409 ns |    NA |  1.00 |           - |           - |           - |                   - |
|  Ulid_ |  3.838 ns |    NA |  0.71 |           - |           - |           - |                   - |
| NUlid_ | 17.126 ns |    NA |  3.17 |      0.0063 |           - |           - |                40 B |

CompareTo is called when use sort. `Ulid` is fastest and zero allocation.

Cli
---
You can install command-line to generate ulid string by  .NET Core Global Tool.

`dotnet tool install --global ulid-cli`

after installed, you can call like here.

```
$ dotnet 
```

```
argument list:
-b, -base64: [default=False]output as base64 format, or output base32 if false
-t, -timestamp: [default=null]timestamp(converted to UTC, ISO8601 format recommended)
-r, -randomness: [default=null]randomness bytes(formatted as Base32, must be 16 characters, case insensitive)
-min, -minRandomness: [default=False]min-randomness(use 000...)
-max, -maxRandomness: [default=False]max-randomness(use ZZZ...)
```

Integrate
---
TODO: extension packages

Direct serialization: for JSON.NET, Utf8Json, MessagePack-CSharp

Direct mapping: for Dapper

Author Info
---
This library is mainly developed by Yoshifumi Kawai(a.k.a. neuecc).  
He is the CEO/CTO of Cysharp which is a subsidiary of [Cygames](https://www.cygames.co.jp/en/).  
He is awarding Microsoft MVP for Developer Technologies(C#) since 2011.  
He is known as the creator of [UniRx](https://github.com/neuecc/UniRx/) and [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp/).

License
---
This library is under the MIT License.
