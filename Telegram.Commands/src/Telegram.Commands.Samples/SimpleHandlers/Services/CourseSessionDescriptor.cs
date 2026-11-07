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

    public class CourseSessionDescriptor<TData> : CourseSessionDescriptorBase, ISessionInfoWithData<TData>
    {
        public new TData Data
        {
            get => base.Data == null ? default : (TData)base.Data;
            set => base.Data = value;
        }
    }
}
