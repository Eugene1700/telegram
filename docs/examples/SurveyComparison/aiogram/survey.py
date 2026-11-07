import re
from aiogram import F, Router
from aiogram.filters import Command, StateFilter
from aiogram.fsm.context import FSMContext
from aiogram.fsm.state import State, StatesGroup
from aiogram.types import CallbackQuery, Message

router = Router()

class Survey(StatesGroup):
    name = State()
    age = State()
    confirm = State()

async def prompt(state: FSMContext, output, error=None):
    step = await state.get_state()
    data = await state.get_data()
    if step == Survey.name.state:
        text, buttons = "Имя (2–50 символов):", [("Отмена", "cancel")]
    elif step == Survey.age.state:
        text, buttons = "Возраст (18–120):", [("Назад", "back"), ("Отмена", "cancel")]
    else:
        text = f"Проверьте: {data['name']}, {data['age']}"
        buttons = [("Назад", "back"), ("Отмена", "cancel"), ("Подтвердить", "confirm")]
    output.show((error + " " if error else "") + text, buttons)

@router.message(Command("survey"), StateFilter(None))
async def start(message: Message, state: FSMContext, output):
    await state.set_data({})
    await state.set_state(Survey.name)
    await prompt(state, output)

@router.callback_query(StateFilter(Survey.name, Survey.age, Survey.confirm), F.data == "cancel")
async def cancel(query: CallbackQuery, state: FSMContext, output):
    await state.clear()
    output.show("Анкета отменена.")

@router.callback_query(StateFilter(Survey.age, Survey.confirm), F.data == "back")
async def back(query: CallbackQuery, state: FSMContext, output):
    await state.set_state(Survey.name if await state.get_state() == Survey.age.state else Survey.age)
    await prompt(state, output)

@router.callback_query(Survey.confirm, F.data == "confirm")
async def confirm(query: CallbackQuery, state: FSMContext, output):
    data = await state.get_data()
    output.show(f"Готово: {data['name']}, {data['age']}")
    await state.clear()

@router.message(Survey.name)
async def name(message: Message, state: FSMContext, output):
    value = (message.text or "").strip()
    if not re.fullmatch(r"[A-Za-zА-Яа-яЁё][A-Za-zА-Яа-яЁё '-]{1,49}", value):
        await prompt(state, output, "Некорректное имя.")
        return
    await state.update_data(name=value)
    await state.set_state(Survey.age)
    await prompt(state, output)

@router.message(Survey.age)
async def age(message: Message, state: FSMContext, output):
    value = (message.text or "").strip()
    if not re.fullmatch(r"[0-9]{1,3}", value) or not 18 <= int(value) <= 120:
        await prompt(state, output, "Некорректный возраст.")
        return
    await state.update_data(age=int(value))
    await state.set_state(Survey.confirm)
    await prompt(state, output)

@router.message(Survey.confirm)
async def review(message: Message, state: FSMContext, output):
    await prompt(state, output)

@router.message()
@router.callback_query()
async def idle(event, output):
    output.show("Начните /survey.")
