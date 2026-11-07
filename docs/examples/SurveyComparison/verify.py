"""Cross-framework behavioral comparison. Every file-backed event runs in a NEW process."""
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parent
DLL = ROOT / "dotnet/bin/Debug/net10.0/SurveyComparison.dll"
ENV = dict(os.environ, PYTHONUTF8="1", DOTNET_NOLOGO="1")

def command(framework, store):
    return (["dotnet", str(DLL), str(store)] if framework == "dotnet"
            else [sys.executable, str(ROOT / "aiogram/main.py"), str(store)])

def invoke(framework, store, event):
    result = subprocess.run(command(framework, store), input=json.dumps(event) + "\n",
                            text=True, encoding="utf-8", capture_output=True, env=ENV, check=True)
    return json.loads(result.stdout.strip().splitlines()[-1])

def visible(result):
    return result["text"], [b["title"] for b in result["buttons"]]

STEPS = [
    ("/survey", "Имя (2–50 символов):"),
    ("A", "Некорректное имя. Имя (2–50 символов):"),
    ("Ada", "Возраст (18–120):"),
    ("seventeen", "Некорректный возраст. Возраст (18–120):"),
    ({"click": "Назад"}, "Имя (2–50 символов):"),
    ("Grace", "Возраст (18–120):"),
    ("17", "Некорректный возраст. Возраст (18–120):"),
    ("121", "Некорректный возраст. Возраст (18–120):"),
    ("36", "Проверьте: Grace, 36"),
    ({"click": "Назад"}, "Возраст (18–120):"),
    ("37", "Проверьте: Grace, 37"),
    ({"click": "Подтвердить"}, "Готово: Grace, 37"),
    ("/survey", "Имя (2–50 символов):"),
    ({"click": "Отмена"}, "Анкета отменена."),
    ("Ada", "Начните /survey."),
]

def event_for(action, last):
    if isinstance(action, str):
        return {"text": action}
    return {"callback": next(b["data"] for b in last["buttons"] if b["title"] == action["click"])}

def check():
    with tempfile.TemporaryDirectory(prefix="survey-comparison-") as tmp:
        transcripts = {}
        for framework in ("dotnet", "aiogram"):
            store = Path(tmp) / (framework + ".json")
            last, transcript = None, []
            for action, expected in STEPS:
                last = invoke(framework, store, event_for(action, last))
                assert last["text"] == expected, (framework, action, last)
                transcript.append(visible(last))
            assert json.loads(store.read_text(encoding="utf-8")) == {}, "Cancelled draft retained"
            transcripts[framework] = transcript
            print(f"{framework}: {len(STEPS)} file-backed updates passed, fresh process per update")
            # One process retains in-memory state.
            result = subprocess.run(command(framework, "--memory"), input='{"text":"/survey"}\n{"text":"Ada"}\n',
                                    text=True, encoding="utf-8", capture_output=True, env=ENV, check=True)
            rows = [json.loads(line) for line in result.stdout.splitlines()]
            assert rows[-1]["text"] == "Возраст (18–120):", rows
            # A new process cannot resume an in-memory session.
            assert invoke(framework, "--memory", {"text": "36"})["text"] == "Начните /survey."
            print(f"{framework}: in-memory retention and restart loss passed")
        assert transcripts["dotnet"] == transcripts["aiogram"], "Visible behavior differs"
        print("PASS: identical prompts/buttons; validation, back, cancel and actual process restart verified")

if __name__ == "__main__":
    check()
