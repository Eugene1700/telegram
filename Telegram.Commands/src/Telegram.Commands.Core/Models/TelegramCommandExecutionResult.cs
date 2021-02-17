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

        private TelegramCommandExecutionResult()
        {
            
        }
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data)
            where TNextCommand : ITelegramCommand<TQuery>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, long moveToChatId)
            where TNextCommand : ITelegramCommand<TQuery>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId
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
        
        public static TelegramCommandExecutionResult GoOut()
        {
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = null
            };
        }
    }
}