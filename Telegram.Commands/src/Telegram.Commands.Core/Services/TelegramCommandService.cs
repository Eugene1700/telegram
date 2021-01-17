using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Services
{
    public class TelegramCommandService
    {
        private readonly TelegramClient _telegramClient;
        private readonly ITelegramCommandFactory _commandFactory;
        private readonly IAuthProvider _authProvider;

        public TelegramCommandService(TelegramClient telegramClient,
            ITelegramCommandFactory commandFactory, IAuthProvider authProvider)
        {
            _telegramClient = telegramClient;
            _commandFactory = commandFactory;
            _authProvider = authProvider;
        }

        public async Task<TelegramResult> Handle(Update update)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await QueryHandler(update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        await QueryHandler(update.CallbackQuery);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new TelegramResult(true);
            }
            catch (NotImplementedException ex)
            {
                await _telegramClient.SendTextMessageAsync(update.GetChatId(), ex.Message);
                return new TelegramResult(true);
            }
        }

        private async Task QueryHandler<T>(T query)
        {
            try
            {
                var command = await GetCommand(query);
                if (command != null)
                {
                    await command.Execute(query);
                }
            }
            catch (TelegramException ex)
            {
                var message = query.AsMessage();
                await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                    replyToMessageId: message.MessageId);
            }
        }

        private async Task<ITelegramCommand<T>> GetCommand<T>(T query)
        {
            var comType = typeof(ITelegramCommand<T>);
            var commandType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .SingleOrDefault(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    return p.IsClass && !p.IsAbstract &&
                           comType.IsAssignableFrom(p) && attrLoc != null && attrLoc.MatchCommand(query.GetData());
                });

            var attr = commandType?.GetCustomAttribute<CommandAttribute>();
            if (commandType == null || attr == null)
                throw new TelegramException("Command not found");

            var user = await _authProvider.AuthUser(query.GetId());
            if (user == null)
                throw new TelegramException("User not found");

            if (user.Permission < attr.Permission)
                throw new TelegramException("У вас недостаточно прав для выполнения этой команды");

            return await _commandFactory.GetCommand(query, commandType);
        }
    }

    public class TelegramResult
    {
        public bool Ok { get; }

        public TelegramResult(bool ok)
        {
            Ok = ok;
        }
    }
}