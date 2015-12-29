# Dasher Schema

[![Build status](https://ci.appveyor.com/api/projects/status/km8g7viqsq0lg2rx?svg=true)](https://ci.appveyor.com/project/andysturrock/dasher-schema)

[Dasher](https://github.com/drewnoakes/dasher) provides a way to deal at runtime with messages that mismatch in structure.  This project provides a build-time mechanism for generating explicit externalised schemas for the messages and also checking message compatibility.

## Dasher.Schema Annotations
[![MetadataExtractor NuGet version](https://img.shields.io/nuget/v/Dasher.Schema.svg)](https://www.nuget.org/packages/Dasher.Schema)
[![MetadataExtractor download stats](https://img.shields.io/nuget/dt/Dasher.Schema.svg)](https://www.nuget.org/packages/Dasher.Schema)
This project provides attributes/annotations for classes that you want to send or receive using [Dasher](https://github.com/drewnoakes/dasher).
The schema generation project below can then use those annotations to generate an explicit schema for your messages.

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

## Dasher.Schema.Generation
[![MetadataExtractor NuGet version](https://img.shields.io/nuget/v/Dasher.Schema.Generation.svg)](https://www.nuget.org/packages/Dasher.Schema.Generation)
[![MetadataExtractor download stats](https://img.shields.io/nuget/dt/Dasher.Schema.Generation.svg)](https://www.nuget.org/packages/Dasher.Schema.Generation)
This project uses the annotations above to generate an explicit schema file.

On a rebuild of the project a file called App.manifest will be created in the project directory and also the output directory.

Add the following incantation to the Post-build event command line (right-click the project->Properties->Build Events tab):
`PATH TO EXE\Dasher.Schema.Generation --targetDir=$(TargetDir) --targetPath=$(TargetPath) --projectDir=$(ProjectDir)`

If you have installed the [latest version](https://www.nuget.org/packages/Dasher.Schema.Generation/1.0.2) from Nuget Gallery the incantation will be:
`$(SolutionDir)\packages\Dasher.Schema.Generation.1.0.2.0\tools\Dasher.Schema.Generation --targetDir=$(TargetDir) --targetPath=$(TargetPath) --projectDir=$(ProjectDir)`

The App.manifest will look something like this:
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

## Dasher.Schema.Comparison
[![MetadataExtractor NuGet version](https://img.shields.io/nuget/v/Dasher.Schema.Generation.svg)](https://www.nuget.org/packages/Dasher.Schema.Generation)
[![MetadataExtractor download stats](https://img.shields.io/nuget/dt/Dasher.Schema.Generation.svg)](https://www.nuget.org/packages/Dasher.Schema.Generation)
This project compares Dasher.Schema.Generation files and determines message compatibility.

Add the following incantation to the Post-build event command line (right-click the project->Properties->Build Events tab):
`PATH TO EXE\Dasher.Schema.Generation --targetDir=$(TargetDir) --targetPath=$(TargetPath) --projectDir=$(ProjectDir)`

If you have installed the [latest version](https://www.nuget.org/packages/Dasher.Schema.Comparison) from Nuget Gallery the incantation will be:
`$(SolutionDir)\packages\Dasher.Schema.Comparison.1.0.3.0\tools\Dasher.Schema.Comparison --manifestPath=$(ProjectDir)\App.manifest --otherManifestsDir=OTHERMANIFESTSPATH --manifestFileGlob=MANIFESTFILEGLOB`

Set OTHERMANIFESTSPATH to where all your other manifests are stored.  This directory is searched recursively.
Set MANIFESTFILEGLOB to a glob that will match the files, eg *.* to match everything, App.manifest to only match files called App.manifest.


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

