# Survey comparison: Bot.Commands and aiogram

Two executable, offline implementations of the same Telegram survey:
name → age → confirmation, with validation, Back and Cancel buttons.
The visible conversation is Russian; this guide is English.

## Contract

- Start with /survey when no survey is active.
- Name: 2–50 characters; Latin/Cyrillic letters, spaces, apostrophes and hyphens,
  starting with a letter. Input is trimmed.
- Age: 1–3 ASCII digits representing an integer from 18 through 120.
- Invalid input stays on the same step and displays an error and the prompt.
- Back returns age → name or confirmation → age. Previously entered values remain
  in the draft; advancing requires entering the field again.
- Cancel is available at every input step and removes the draft.
- Confirmation displays the result and removes the draft. Saving the completed
  form to a business database is outside this comparison.
- Both examples retain sessions indefinitely until completion/cancellation.
- A restart between completed updates must preserve the current step and data
  with file storage. In-memory storage must lose them on restart.

## Run

Requires .NET 10 and Python 3.10+ (verified here with Python 3.12 and aiogram 3.31.0).
From the repository root, on Windows:

```powershell
dotnet build docs/examples/SurveyComparison/dotnet/SurveyComparison.csproj
python -m venv docs/examples/SurveyComparison/.venv
docs/examples/SurveyComparison/.venv/Scripts/python -m pip install -r docs/examples/SurveyComparison/aiogram/requirements.txt
docs/examples/SurveyComparison/.venv/Scripts/python -u docs/examples/SurveyComparison/verify.py
```

Use an actual Python 3 executable if your system's python command resolves to Python 2.
On Unix use .venv/bin/python. The verification script locates itself and can run
from any directory after building the .NET executable.

The runners accept newline-delimited JSON on stdin:
`{"text":"/survey"}`, followed by text messages or
`{"callback":"<data from the previous output button>"}`.
They print the last response as JSON. Pass a storage file path or --memory:

```sh
dotnet docs/examples/SurveyComparison/dotnet/bin/Debug/net10.0/SurveyComparison.dll --memory
python docs/examples/SurveyComparison/aiogram/main.py --memory
```

Use the venv Python for aiogram. No real token is required and no Telegram API
requests are made. Output is a recording transport: a live host must send messages,
acknowledge callbacks and implement update delivery.

## What is actually tested

verify.py runs both libraries' real routers with native Telegram updates.
Each of 15 file-backed events is processed by a new OS process, not just a new
DI scope. It checks invalid names/ages, back from both later steps, edited answers,
confirmation, cancellation, another message after cancellation, and identical
visible prompts/button labels. Native callback payloads intentionally differ.

For each implementation it also verifies that two updates in one process preserve
in-memory state and that a fresh process cannot resume it. Verification uses a
unique temporary directory; existing application databases are never opened.

This verifies orderly restart between events. It does not simulate termination
halfway through a handler, parallel updates, a server crash while writing,
webhook retries or real Telegram delivery.

## Comparison

Counts below are nonblank physical lines, including imports and braces, not an
abstract complexity metric. C# flow/data includes SurveyFlow, SurveyCommand,
SurveyData and SurveyStep; Python includes survey.py. Hosts, recording transport,
storage and verification are excluded from this business-logic count.

| Aspect | Bot.Commands | aiogram 3.31.0 |
| --- | --- | --- |
| Flow and data | 92 nonblank lines | 65 nonblank lines |
| File storage adapter | 48 nonblank lines | 32 nonblank lines |
| Conversation structure | One Fluent chain describes states, prompts and buttons | Router handlers select the state and update it explicitly |
| Validation | Typed transition functions return the next state | Handlers validate, update data and call set_state |
| Back | NextFromCallback declares the target state beside the button | Filtered callback handler chooses the target state |
| Cancel | Terminal Fluent state returns Break; the engine closes the session | Callback handler calls FSMContext.clear |
| Data | SurveyData class; typed context and state enum | Standard FSM dictionary with string keys in this implementation |
| In memory | services.UseInMemoryStorage() | Dispatcher(storage=MemoryStorage()) |
| Process restart | Passed with the demo file store | Passed with the demo file store |
| Multi-messenger seam | Generic SurveyFlow&lt;TMessage&gt;; concrete Telegram descendant | This router uses Telegram Message/CallbackQuery directly |
| Event serialization | Host must serialize a conversation's updates | This host configures SimpleEventIsolation for one process |

aiogram can also use typed application models; a dictionary is its standard FSM
data interface, not a prohibition on typing. Bot.Commands can use platform-specific
handlers; a generic flow does not make all keyboards and media interchangeable.

The file stores are deliberately small sequential teaching adapters. C# saves the
session envelope/state/data in one replacement; aiogram's set_data and set_state
are separate writes. Neither example provides an atomic transaction covering
incoming events, state and outgoing messages. Do not use these adapters as a
multi-process production database.

For production, aiogram provides built-in storage alternatives such as RedisStorage;
this comparison does not make a custom JSON store look like its only option.
Bot.Commands supplies a storage contract, the new in-memory implementation and the
repository's application-owned EntityStorage example. This test does not run that
sample's startup or its database initialization.

## In-memory configuration

```csharp
services.UseTelegramCommandsServices();
services.AddScoped<IBotProfile, YourBotProfile>();
services.AddSingleton<IClock, YourClock>();
services.UseInMemoryStorage();
```

UseInMemoryStorage replaces the session-store registration. The backing store is
singleton per service provider, while ISessionsStore is scoped and uses the active
IBotProfile.BotName. Register it after another storage registration when choosing
the memory implementation. Repeating the call does not replace the backing store.

Session data is snapshotted through System.Text.Json, so it must be serializable.
Reads do not share mutable data with stored values. Expired entries are removed
when read and swept on writes; completion removes the entry. Store operations are
locked, but the whole read/execute/write conversation is not transactional.
Do not mistake this for per-conversation event isolation or durable storage.

## Interpretation

This example does not support the claim that aiogram is less capable: both satisfy
the tested contract. aiogram needs fewer lines here. Bot.Commands makes the
transition graph more locally visible and provides a statically typed model and a
messenger-independent flow boundary. Whether this is easier to maintain should be
tested by changing a real scenario, not inferred from line counts alone.

Sources:
- [aiogram FSM](https://docs.aiogram.dev/en/v3.31.0/dispatcher/finite_state_machine/index.html)
- [aiogram storages](https://docs.aiogram.dev/en/v3.31.0/dispatcher/finite_state_machine/storages.html)
- [C# flow](dotnet/SurveyFlow.cs)
- [Python flow](aiogram/survey.py)
