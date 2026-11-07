# Contributing

Thank you for helping improve Bot.Commands. Keep the shared command engine
independent of messenger SDKs, and place platform-specific behavior in an adapter.

## Development setup

Install the .NET 10 SDK, clone the repository, and run from its root:

```sh
dotnet restore Telegram.Commands/src/Telegram.Commands.sln
dotnet build Telegram.Commands/src/Telegram.Commands.sln --no-restore
dotnet test Telegram.Commands/src/Telegram.Command.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests.csproj
```

For documentation examples:

```sh
dotnet run --project docs/examples/QuickStart -- telegram
dotnet run --project docs/examples/QuickStart -- max
dotnet run --project docs/examples/QuickStart -- vk
```

The quick start is outside the main solution so the documentation example does
not become a runtime dependency. It uses project references and must be built or
run separately. It performs no external bot API calls.

## Making a change

1. Identify whether the behavior belongs in Abstract, Core, UI, an adapter, or the host.
2. Keep shared contracts messenger-independent; do not introduce legacy Telegram aliases into Core or Abstract.
3. Add focused tests for changed execution or routing behavior, including affected adapters.
4. Update the relevant guide and runnable example if the public API changes.
5. Include the problem, resulting behavior, validation, and remaining limitations in the change description.

Avoid embedding SDK types in Core or UI. Treat command names, session data, and
Fluent state values as persistence contracts. A change that compiles can still
break already stored conversations.

Do not put real tokens, user data, or webhook secrets in examples. Tests should
use synthetic updates unless a live integration test is explicitly documented.

## Reporting issues

Include the .NET version, adapter and SDK version, a minimal command/flow, the
relevant exception, and a sanitized update shape. For session problems, include
store lifetime, expiry behavior, and whether multiple bots/workers share storage.

## Documentation conventions

- Write public documentation in English.
- Prefer complete runnable examples; clearly label fragments and application-provided services.
- Link to repository-relative files so documentation works on GitHub and in a checkout.
- Distinguish implemented behavior from planned features and SDK capabilities.
- Update the supported-update tables when adding adapter cases.
- Do not advertise a package version until that release has been published and verified.

## Preparing a public release

The current source tree is not by itself a complete distribution plan. Before a release:

- Confirm the intended license and add its full text. Metadata currently references MIT,
  but this checkout has no standalone license file.
- Choose package IDs and coordinated versions for the shared projects and adapters.
  Core now uses the package ID `Bot.Commands.Core`; do not assume every project
  name is already available as a published package.
- Review package metadata, repository links, license metadata, and inclusion of a package README.
- Build and test the release configuration, run all documentation example platforms, and
  validate the actual packages in a clean consumer project.
- Exercise live Telegram, Max, and VK integrations for the features claimed by the release.
- Record breaking API/session-format changes and update the installation instructions
  to point to the verified published versions.

Resolve or explicitly assess build/dependency warnings as part of release review.
Passing local unit tests is evidence about the tested code paths, not a claim that
every platform feature or deployment mode has been validated.
