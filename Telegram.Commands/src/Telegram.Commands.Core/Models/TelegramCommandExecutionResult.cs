﻿using System;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent;

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
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TData>(TData data, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<TData>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
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
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand>(uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
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
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TData>(TData data, long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<TData>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, TData>();
            return new TelegramCommandExecutionResult
            {
                Data = data,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand, TQuery>( long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : ISessionTelegramCommand<TQuery, EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult Ahead<TNextCommand>( long moveToChatId, uint? sessionDurationInSec = 600)
            where TNextCommand : IBehaviorTelegramCommand<EmptyObject>
        {
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TNextCommand, EmptyObject>();
            return new TelegramCommandExecutionResult
            {
                Data = new EmptyObject(),
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = moveToChatId,
                SessionDurationInSec = sessionDurationInSec
            };
        }
        
        public static TelegramCommandExecutionResult AheadFluent<TNextCommand, TObject, TStates>(TObject obj, bool fire, uint? sessionDurationInSec = 600)
            where TNextCommand : FluentCommand<TObject, TStates>
        {
            var commandInfo = TelegramCommandExtensions.GetFluentCommandInfo<TNextCommand, TObject, TStates>();
            return new TelegramCommandExecutionResult
            {
                Data = new FluentObject<TObject, TStates>(obj, fire ? FireType.Entry : FireType.HandleCurrent),
                Result = fire ? ExecuteResult.Fire : ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
                SessionDurationInSec = sessionDurationInSec,
            };
        }
        
        internal static TelegramCommandExecutionResult AheadFluent<TObj, TStates>(FluentCommand<TObj, TStates> command,
            FluentObject<TObj, TStates> sessionObject, uint? sessionDurationInSec)
        {
            var commandInfo = command.GetCommandInfo();
            return new TelegramCommandExecutionResult
            {
                Data = sessionObject,
                Result = ExecuteResult.Ahead,
                NextCommandDescriptor = commandInfo,
                WaitFromChatId = null,
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