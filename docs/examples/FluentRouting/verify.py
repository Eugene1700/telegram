"""Offline nested routing checks with actual process restarts."""
import json
from pathlib import Path
import subprocess
import tempfile

DLL = Path(__file__).resolve().parent / "bin/Debug/net10.0/FluentRouting.dll"
STEPS = [
    ("/order", "Как вас зовут?"),
    ("Ada", "Улица и дом:"),
    ("Main 1", "Город:"),
    ("Moscow", "Адрес: Main 1; город: Moscow"),
    (("Сохранить адрес",), "Заказ: Ada; адрес: Main 1, Moscow"),
    (("Адрес",), "Улица и дом:"),
    ("New 2", "Город:"),
    (("Отмена города",), "Адрес: New 2; город: —"),
    (("Сохранить адрес",), "Город:"),
    ("Berlin", "Адрес: New 2; город: Berlin"),
    (("Отмена адреса",), "Заказ: Ada; адрес: Main 1, Moscow"),
    (("Готово",), "Готово: Ada; Main 1, Moscow"),
    ("/order", "Как вас зовут?"),
    ("Grace", "Улица и дом:"),
    (("Отмена адреса",), "Заказ: Grace; адрес: —"),
    (("Отмена",), "Заказ отменён."),
    ("hello", "Начните /order."),
]

def event(action, last):
    if isinstance(action, str):
        return {"text": action}
    return {"callback": next(b["data"] for b in last["buttons"] if b["title"] == action[0])}

def invoke(store, payload):
    p = subprocess.run(["dotnet", str(DLL), str(store)], input=json.dumps(payload)+"\n",
                       text=True, encoding="utf-8", capture_output=True)
    if p.returncode:
        raise RuntimeError(p.stderr)
    return json.loads(p.stdout.strip().splitlines()[-1])

with tempfile.TemporaryDirectory(prefix="fluent-routing-") as tmp:
    store = Path(tmp) / "sessions.json"
    last = None
    for action, expected in STEPS:
        last = invoke(store, event(action, last))
        assert last["text"] == expected, (action, last, expected)
        for session in json.loads(store.read_text(encoding="utf-8")).values():
            data = session["Data"]["Object"]
            routes = data["Routes"]
            assert data["RouteId"] is None or data["RouteId"] in routes
            for route_id, route in routes.items():
                assert route["RouteId"] == route_id
                assert "Routes" not in route["Data"]
                assert route["ParentRouteId"] is None or route["ParentRouteId"] in routes
                assert route["IsRealized"] == (route["Result"] is not None)
    assert json.loads(store.read_text(encoding="utf-8")) == {}
    print(f"PASS: {len(STEPS)} updates, a fresh process per update; nested return and cancellation")

with subprocess.Popen(["dotnet", str(DLL), "--memory"], stdin=subprocess.PIPE,
                      stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, encoding="utf-8") as p:
    last = None
    for action, expected in STEPS:
        p.stdin.write(json.dumps(event(action, last))+"\n")
        p.stdin.flush()
        last = json.loads(p.stdout.readline())
        assert last["text"] == expected, (action, last, expected)
    p.stdin.close()
    assert p.wait(timeout=30) == 0, p.stderr.read()
assert invoke("--memory", {"text": "Ada"})["text"] == "Начните /order."
print("PASS: same nested flows with UseInMemoryStorage; restart loses in-memory state")
