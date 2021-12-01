# How to Contribute

## Bug Reports

Our project isn't always perfect, but we strive to always improve on that work. Please report any bugs by [filing an issue](https://github.com/factset/enterprise-sdk-utils-dotnet/issues/new).

## Feature Requests

We're always looking for suggestions to improve this project. If you have a suggestion for improving an existing feature, or would like to suggest a completely new feature, please [file an issue](https://github.com/factset/enterprise-sdk-utils-dotnet/issues/new).

## Pull Requests

Along with our desire to hear your feedback and suggestions, we're also interested in accepting direct assistance in the form of new code or documentation.

We ask that you please file a [bug report](#bug-reports) or [feature request](#feature-requests) first to make sure your change isn't already being worked on, then [open a pull request](https://github.com/factset/enterprise-sdk-utils-dotnet/compare) with the code change.

## Testing

Tests will be run automatically with each pull request, or you can run them locally with `dotnet test`.

```bash
$ dotnet test
```

Coverage reports are generated automatically on SonarQube. They can also be generated locally using `coverlet`. First change to the test project directory:

```bash
$ cd tests/FactSet.SDK.Utils.Tests
```

Then run the data collector:

```bash
$ dotnet test --collect:"XPlat Code Coverage"
```

This will create a `TestResults` directory containing a folder named with a GUID and inside that folder is the `coverage.cobertura.xml` file containing data about the tests. Then you can run the `reportgenerator`:

```bash
$ reportgenerator "-reports:TestResults/{GUID}/coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"
```

This will generate a `coveragereport` folder which has the coverage results in the `index.html` file. For more information about generating reports, [see the docs.](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=windows)
