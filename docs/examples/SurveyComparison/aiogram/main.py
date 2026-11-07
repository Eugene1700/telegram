import asyncio
import json
import sys
from aiogram import Bot, Dispatcher
from aiogram.fsm.storage.memory import MemoryStorage, SimpleEventIsolation
from aiogram.types import Update, InlineKeyboardButton, InlineKeyboardMarkup
from storage import JsonStorage
from survey import router

class Output:
    def __init__(self):
        self.last = None

    def show(self, text, buttons=()):
        markup = InlineKeyboardMarkup(inline_keyboard=[
            [InlineKeyboardButton(text=title, callback_data=data)] for title, data in buttons
        ])
        self.last = {"text": text, "buttons": [
            {"title": button.text, "data": button.callback_data}
            for row in markup.inline_keyboard for button in row
        ]}

async def main():
    storage = MemoryStorage() if sys.argv[1] == "--memory" else JsonStorage(sys.argv[1])
    dp = Dispatcher(storage=storage, events_isolation=SimpleEventIsolation())
    dp.include_router(router)
    bot = Bot("123456:ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghi")
    output = Output()
    # Synthetic updates only: the transport is a recorder, never a Telegram request.
    try:
        for line in sys.stdin:
            item = json.loads(line)
            user = {"id": 2, "is_bot": False, "first_name": "Reader"}
            message = {"message_id": 1, "date": 1, "chat": {"id": 2, "type": "private"}, "from": user}
            if "text" in item:
                message["text"] = item["text"]
            raw = {"update_id": 1}
            if "callback" in item:
                raw["callback_query"] = {"id": "callback", "from": user, "chat_instance": "demo",
                                         "message": message, "data": item["callback"]}
            else:
                raw["message"] = message
            await dp.feed_update(bot, Update.model_validate(raw), output=output)
            print(json.dumps(output.last, ensure_ascii=False), flush=True)
    finally:
        await storage.close()
        await bot.session.close()

if __name__ == "__main__":
    sys.stdin.reconfigure(encoding="utf-8")
    sys.stdout.reconfigure(encoding="utf-8")
    asyncio.run(main())
