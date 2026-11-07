import json
from pathlib import Path
from aiogram.fsm.storage.base import BaseStorage, StorageKey

class JsonStorage(BaseStorage):
    """Sequential demo only; no cross-process transaction or locking."""
    def __init__(self, path):
        self.path = Path(path)

    def _load(self):
        return json.loads(self.path.read_text(encoding="utf-8")) if self.path.exists() else {}

    @staticmethod
    def _key(key: StorageKey):
        return f"{key.bot_id}|{key.chat_id}|{key.user_id}|{key.thread_id}|{key.destiny}"

    def _write(self, key, field, value):
        all_data = self._load()
        name = self._key(key)
        record = all_data.setdefault(name, {"state": None, "data": {}})
        record[field] = value
        if record["state"] is None and not record["data"]:
            del all_data[name]
        temp = self.path.with_suffix(self.path.suffix + ".tmp")
        temp.write_text(json.dumps(all_data, ensure_ascii=False), encoding="utf-8")
        temp.replace(self.path)

    async def set_state(self, key, state=None):
        self._write(key, "state", state.state if hasattr(state, "state") else state)

    async def get_state(self, key):
        return self._load().get(self._key(key), {}).get("state")

    async def set_data(self, key, data):
        self._write(key, "data", dict(data))

    async def get_data(self, key):
        return self._load().get(self._key(key), {}).get("data", {})

    async def close(self):
        pass
