# Dasher Schema

[![Build status](https://ci.appveyor.com/api/projects/status/km8g7viqsq0lg2rx?svg=true)](https://ci.appveyor.com/project/andysturrock/dasher-schema)

[Dasher](https://github.com/drewnoakes/dasher) provides a way to deal at runtime with messages that mismatch in structure.  This project provides a build-time mechanism for generating explicit externalised schemas for the messages and also checking message compatibility.

Annotate your classes that you want to generate a schema for like this:
```csharp
using Dasher.Schema;

[ReceiveMessage]
[SendMessage]
public sealed class UserScoreWithDefaultScore
{
    public UserScoreWithDefaultScore(string name, int score = 100)
    {
        Name = name;
        Score = score;
    }

    public string Name { get; }
    public int Score { get; }
}
```

Add the following incantation to the Post-build event command line (right-click the project->Properties->Build Events tab):
`PATH TO EXE\Dasher.Schema.Generation --targetDir=$(TargetDir) --targetPath=$(TargetPath) --projectDir=$(ProjectDir)`

On a rebuild of the project a file called App.manifest will be created in the project directory and also the output directory.
This file will look something like this:
```xml
<?xml version="1.0" encoding="utf-8"?>
<App>
  <SendsMessages>
    <Message name="UserScoreWithDefaultScore">
      <Field name="name" type="System.String" />
      <Field name="score" type="System.Int32" default="100" />
    </Message>
  <SendsMessages>
  <ReceivesMessages>
    <Message name="UserScoreWithDefaultScore">
      <Field name="name" type="System.String" />
      <Field name="score" type="System.Int32" default="100" />
    </Message>
  <ReceivesMessages>
</App>
```

## License

Copyright 2015 Andy Sturrock

> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
> You may obtain a copy of the License at
>
>     http://www.apache.org/licenses/LICENSE-2.0
>
> Unless required by applicable law or agreed to in writing, software
> distributed under the License is distributed on an "AS IS" BASIS,
> WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
> See the License for the specific language governing permissions and
> limitations under the License.

