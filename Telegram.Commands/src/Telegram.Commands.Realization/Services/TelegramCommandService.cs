using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Realization.Services
{
    internal sealed class TelegramCommandService : CommandServiceBase, ITelegramCommandResolver
    {
        private readonly ITelegramBotClient _telegramClient;

        public TelegramCommandService(ITelegramBotClient telegramClient,
            IAuthProvider authProvider,
            SessionManager sessionManager,
            ITelegramBotProfile telegramBotProfile,
            CommandExecutionService commandExecutionService): base(commandExecutionService,sessionManager, authProvider, telegramBotProfile)
        {
            _telegramClient = telegramClient;
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
                    case UpdateType.PreCheckoutQuery:
                        await QueryHandler(update.PreCheckoutQuery);
                        break;
                    case UpdateType.MyChatMember:
                        await QueryHandler(update.MyChatMember);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new TelegramResult(true);
            }
            catch (NotImplementedException ex)
            {
                await _telegramClient.SendTextMessageAsync(update.GetChatId(), ex.Message);
                return new TelegramResult(false);
            }
        }
        
        protected override async Task SendDomainException<T>(T query, TelegramDomainException ex)
        {
            var message = query.AsMessage();
            await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                replyToMessageId: message.MessageId);
        }

        protected override CommandDescriptorComposition GetEvent<T>(T query)
        {
            if (!IsEvent(query))
                return null;

            Type type = null;
            if (query is ChatMemberUpdated)
            {
                type = FindEventByType(EventType.BotMemberAddedToChat);
            }

            if (query is Message mes)
            {
                switch (mes.Type)
                {
                    case MessageType.MigratedToSupergroup:
                    case MessageType.MigratedFromGroup:
                        type = FindEventByType(EventType.MigrateToSuperGroup);
                        break;
                    case MessageType.ChatMembersAdded:
                        type = FindEventByType(EventType.ChatMemberAdded);
                        break;
                    default:
                        return null;
                }
            }

            return CommandDescriptorComposition.CreateQueryResult(new FullCommandDescriptor(type));
        }

        protected override bool IsEvent<T>(T query)
        {
            switch (query)
            {
                case ChatMemberUpdated _:
                    return true;
                case Message mes:
                    switch (mes.Type)
                    {
                        case MessageType.MigratedToSupergroup:
                        case MessageType.MigratedFromGroup:
                        case MessageType.ChatMembersAdded:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        protected override void AssertChatTypeConcrete<T>(T query, ITelegramCommandDescriptor commandInfo)
        {
            if (commandInfo == null)
                throw new ArgumentNullException(nameof(commandInfo));
            var chatType = query.GetChatType();
            var chatId = query.GetChatId();
            switch (chatType)
            {
                case ChatType.Private:
                    if ((commandInfo.Area & ChatArea.Private) != ChatArea.Private)
                        throw new TelegramCommandsChatAreaException("This command is not for private chat", chatId);
                    return;
                case ChatType.Group:
                    if ((commandInfo.Area & ChatArea.Group) != ChatArea.Group)
                        throw new TelegramCommandsChatAreaException("This command is not for group", chatId);
                    return;
                case ChatType.Channel:
                    if ((commandInfo.Area & ChatArea.Channel) != ChatArea.Channel)
                        throw new TelegramCommandsChatAreaException("This command is not for channel", chatId);
                    return;
                case ChatType.Supergroup:
                    if ((commandInfo.Area & ChatArea.SuperGroup) != ChatArea.SuperGroup)
                        throw new TelegramCommandsChatAreaException("This command is not for supergroup", chatId);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}