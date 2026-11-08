using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;

namespace Bot.Commands.Core.Models
{
    public class CommandExecutionResult : ICommandExecutionResult
    {
        public object Data { get; private init; }
        public ICommandDescriptor NextCommandDescriptor { get; private init; }
        public ExecuteResult Result { get; private init; }
        public long? WaitFromChatId { get; private init; }
        public uint? SessionDurationInSec { get; private init; }

        private CommandExecutionResult()
        {
            
        }
        public static CommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionCommand<TQuery, TData>
        {
            var commandInfo = BotCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new CommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorCommand<TData>
        {
            var commandInfo = BotCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
            return new CommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand, TQuery>(uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionCommand<TQuery, EmptyObject>
        {
            var commandInfo = BotCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new CommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand>(uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorCommand<EmptyObject>
        {
            var commandInfo = BotCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new CommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionCommand<TQuery, TData>
        {
            var commandInfo = BotCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new CommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorCommand<TData>
        {
            var commandInfo = BotCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
            return new CommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand, TQuery>( long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionCommand<TQuery, EmptyObject>
        {
            var commandInfo = BotCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new CommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult Ahead<TNextCommand>(long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorCommand<EmptyObject>
        {
            var commandInfo = BotCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new CommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static CommandExecutionResult AheadFluent<TNextCommand, TObject, TStates>(TObject obj, bool fire, uint? sessionDurationInSec = 600)
            where TNextCommand : FluentCommand<TObject, TStates>
        {
            var commandInfo = BotCommandExtensions.GetFluentCommandInfo<TNextCommand, TObject, TStates>();
            return new CommandExecutionResult
            {
                Data = new FluentObject<TObject, TStates>(obj, fire ? FireType.Entry : FireType.HandleCurrent),
                Result = fire ? ExecuteResult.Fire : ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec,
            };
        }
        
        internal static CommandExecutionResult AheadFluent<TObj, TStates>(FluentCommand<TObj, TStates> command,
            FluentObject<TObj, TStates> sessionObject, uint? sessionDurationInSec)
        {
            var commandInfo = command.GetCommandInfo();
            return new CommandExecutionResult
            {
                Data = sessionObject,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        internal static CommandExecutionResult ResumeRoute(Fluent.Routing.FluentRouteResume resume)
            => new()
            {
                Result = ExecuteResult.Fire,
                NextCommandDescriptor = new Bot.Commands.Abstract.Attributes.CommandAttribute { Name = resume.Frame.CallerCommand },
                Data = resume,
                SessionDurationInSec = null
            };

        public static CommandExecutionResult Freeze()
        {
            return new CommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Freeze,
                NextCommandDescriptor = null
            };
        }
        
        public static CommandExecutionResult Break()
        {
            return new CommandExecutionResult
            {
                Data = null,
                Result = ExecuteResult.Break,
                NextCommandDescriptor = null
            };
        }
    }
}
