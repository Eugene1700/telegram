using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EntityStorage;
using EntityStorage.Extensions;
using SimpleHandlers.Domain;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;

namespace SimpleHandlers.Services
{

    public class CourseSessionDescriptorBase : ISessionInfoWithData
    {
        public string CommandQuery { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public object Data { get; set; }
    }
}
