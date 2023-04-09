using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent;

namespace Telegram.Commands.Core.Models
{
    /// <see cref="ITelegramCommandExecutionResult"/>
    public class TelegramCommandExecutionResult : ITelegramCommandExecutionResult
    {
        /// <see cref="ITelegramCommandExecutionResult"/>
        public object Data { get; private init; }
        
        /// <see cref="ITelegramCommandExecutionResult"/>
        public ITelegramCommandDescriptor NextCommandDescriptor { get; private init; }
        
        /// <see cref="ITelegramCommandExecutionResult"/>
        public Moving Moving { get; private init; }
        
        /// <see cref="ITelegramCommandExecutionResult"/>
        public long? WaitFromChatId { get; private init; }
        
        /// <see cref="ITelegramCommandExecutionResult"/>
        public uint? SessionDurationInSec { get; private init; }

        private TelegramCommandExecutionResult()
        {
            
        }
        
        /// <summary>
        /// Construct moving descriptor for the ISessionTelegramCommand next command without user's data. Waiting in current chat.
        /// </summary>
        /// <param name="data">User's data, that will sent to the next command</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command</typeparam>
        /// <typeparam name="TQuery">Type of user's action</typeparam>
        /// <typeparam name="TData">Type of user's data</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, TData>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        ///   Construct moving descriptor for the IBehaviorTelegramCommand next command with user's data. Waiting in current chat.
        /// </summary>
        /// <param name="data">User's data, that will sent to the next command</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command</typeparam>
        /// <typeparam name="TData">Type of user's data</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<TData>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the ISessionTelegramCommand next command without user's data. Waiting in current chat.
        /// </summary>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command</typeparam>
        /// <typeparam name="TQuery">Type of user's action</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery>(uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the IBehaviorTelegramCommand next command without user's data. Waiting in current chat.
        /// </summary>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand>(uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the ISessionTelegramCommand next command with user's data. Waiting in moveToChatId chat.
        /// </summary>
        /// <param name="data">User's data, that will sent to the next command.</param>
        /// <param name="moveToChatId">Chat ID, where session will wait some user actions</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command.</typeparam>
        /// <typeparam name="TQuery">Type of user's action</typeparam>
        /// <typeparam name="TData">Type of user's data.</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, TData>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the IBehaviorTelegramCommand next command with user's data. Waiting in moveToChatId chat.
        /// </summary>
        /// <param name="data">User's data, that will sent to the next command.</param>
        /// <param name="moveToChatId">Chat ID, where session will wait some user actions</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command.</typeparam>
        /// <typeparam name="TData">Type of user's data.</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<TData>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        ///  Construct moving descriptor for the ISessionTelegramCommand next command without user's data. Waiting in moveToChatId chat.
        /// </summary>
        /// <param name="moveToChatId">Chat ID, where session will wait some user actions</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command.</typeparam>
        /// <typeparam name="TData">Type of user's data.</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery>( long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the IBehaviorTelegramCommand next command without user's data. Waiting in moveToChatId chat.
        /// </summary>
        /// <param name="moveToChatId">Chat ID, where session will wait some user actions</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command.</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Ahead<TNextCommand>(long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Construct moving descriptor for the FluentCommand next command with user's data. Waiting in current chat.
        /// </summary>
        /// <param name="data">User's data, that will sent to the next command.</param>
        /// <param name="sessionDurationInSec">Duration of waiting user's actions in seconds. Default is 600 seconds. Null is permanent waiting.</param>
        /// <typeparam name="TNextCommand">Type of next command.</typeparam>
        /// <typeparam name="TData">Type of user's data.</typeparam>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult AheadFluent<TNextCommand, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : FluentCommand<TData>
        {
            var commandInfo = TelegramCommandExtensions.GetFluentCommandInfo<TNextCommand, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = new FluentObject<TData>(data)
                {
                    CurrentStateId = null
                },
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Internal method for moving state inside FluentCommand state machine
        /// </summary>
        /// <param name="command">fluent command</param>
        /// <param name="sessionObject">fluent command session object</param>
        /// <param name="sessionDurationInSec">duration of waiting uses's action</param>
        /// <typeparam name="TObj">Type user's data</typeparam>
        /// <returns>Moving descriptor</returns>
        internal static TelegramCommandExecutionResult AheadFluent<TObj>(FluentCommand<TObj> command,
            FluentObject<TObj> sessionObject, uint? sessionDurationInSec)
        {
            var commandInfo = command.GetCommandInfo();
            return new TelegramCommandExecutionResult
            {
                Data = sessionObject,
                Moving = Moving.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        /// <summary>
        /// Stay in current state 
        /// </summary>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Freeze()
        {
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Moving = Moving.Freeze,
                NextCommandDescriptor = null
            };
        }
        
        /// <summary>
        /// Interrupt chain of commands
        /// </summary>
        /// <returns>Moving descriptor</returns>
        public static TelegramCommandExecutionResult Break()
        {
            return new TelegramCommandExecutionResult
            {
                Data = null,
                Moving = Moving.Break,
                NextCommandDescriptor = null
            };
        }
    }
}