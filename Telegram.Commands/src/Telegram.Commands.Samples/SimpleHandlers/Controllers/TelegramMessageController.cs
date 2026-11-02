using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;

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
        public async Task<BotResult> Update([FromBody] Update update)
        {
            try
            {
                await _botCommandService.Handle(update);
            }
            catch (CommandExtractionException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(CommandExtractionException)}");
            }
            catch (CommandPermissionException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(CommandPermissionException)}");
            }
            catch (CommandChatAreaException e)
            {
                await _telegramBotClient.SendMessage(e.ChatId,
                    $"This is {nameof(CommandChatAreaException)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return new BotResult(true);
        }
    }
}