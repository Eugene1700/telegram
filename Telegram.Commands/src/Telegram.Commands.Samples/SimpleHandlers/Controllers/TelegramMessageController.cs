using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;

namespace SimpleHandlers.Controllers
{
    [ApiController]
    public class TelegramMessageController: ControllerBase
    {
        private readonly ILogger<TelegramMessageController> _logger;
        private readonly IBotCommandResolver<Update> _botCommandService;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageController(ILogger<TelegramMessageController> logger, 
            IBotCommandResolver<Update> botCommandService, ITelegramBotClient telegramBotClient)
        {
            _logger = logger;
            _botCommandService = botCommandService;
            _telegramBotClient = telegramBotClient;
        }
        
        [Route("ping")]
        [AllowAnonymous]
        public string Ping()
        {
            return "Hello, its SampleHandlers App";
        }
        
        [HttpPost]
        [Route(Settings.RouteToUpdate)]
        [AllowAnonymous]
        public async Task<TelegramResult> Update([FromBody] Update update)
        {
            try
            {
                await _botCommandService.Handle(update);
            }
            catch (TelegramExtractionCommandException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(TelegramExtractionCommandException)}");
            }
            catch (TelegramCommandsPermissionException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(TelegramCommandsPermissionException)}");
            }
            catch (TelegramCommandsChatAreaException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(TelegramCommandsChatAreaException)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return new TelegramResult(true);
        }
    }
}