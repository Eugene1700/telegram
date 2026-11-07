using Bot.Commands.Abstract.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.UI.Pagination;

namespace SimpleHandlers.Services.Commands
{

    public class MyObject : IFluentPagination, IFluentPaginationMenu
    {
        public string SecondName { get; set; }
        public string FirstName { get; set; }

        public long ChatId { get; set; }
        public ulong TotalCount { get; set; }
        public uint Limit { get; set; }
        public uint PageNumber { get; set; }
    }
}
