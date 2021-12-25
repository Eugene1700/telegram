using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Models
{
    public class TelegramCommandExecutionResult : ITelegramCommandExecutionResult
    {
        public object Data { get; private set; }
        public ITelegramCommandDescriptor NextCommandDescriptor { get; private set; }
        public ExecuteResult Result { get; private set; }
        
        public long? WaitFromChatId { get; private set; }
        public uint? SessionDurationInSec { get; private set; }

        private TelegramCommandExecutionResult()
        {
            
        }
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, TData>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery>(uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, TData>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Freeze()
        {
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Freeze,
                NextCommandDescriptor = null
            };
        }
        
        public static TelegramCommandExecutionResult Break()
        {
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Break,
                NextCommandDescriptor = null
            };
        }
    }
}