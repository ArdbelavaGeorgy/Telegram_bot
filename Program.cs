using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Создание нового клиента бота
var botClient = new TelegramBotClient("AAFwsbtHlWK_QqgR3SmRMD_Dxcqf8ViJYHg");

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // получать все типы обновлений, кроме связанных с членами чата
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Начинаем прослушивание @{me.Username}");
Console.ReadLine();
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Проверка на тип обновления и обработка соответственно
    if (update.Type == UpdateType.Message)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;
        if (message.Text is not null)
        {
            Console.WriteLine($"Получено текстовое сообщение '{message.Text}' в чате {chatId}.");
            await ProcessMessageAsync(botClient, message.Text, chatId, cancellationToken);
        }
    }
    else if (update.Type == UpdateType.CallbackQuery)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery.Message.Chat.Id;
        Console.WriteLine($"Получен колбэк запрос '{callbackQuery.Data}' в чате {chatId}.");
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
        await ProcessMessageAsync(botClient, callbackQuery.Data, chatId, cancellationToken);
    }
}

async Task ProcessMessageAsync(ITelegramBotClient botClient, string messageText, long chatId, CancellationToken cancellationToken)
{
    switch (messageText)
    {
        case "Проверка":
            await botClient.SendTextMessageAsync(chatId, "Проверка бота: работа корректна", cancellationToken: cancellationToken);
            break;
        case "Привет":
            await botClient.SendTextMessageAsync(chatId, "Здравствуй, Георгий", cancellationToken: cancellationToken);
            break;
        case "Картинка":
            await botClient.SendPhotoAsync(chatId, InputFile.FromUri("https://w.forfun.com/fetch/cc/cc1c54f2e03870add8505ecebdfc356a.jpeg"), cancellationToken: cancellationToken);
            break;
        case "Стикер":
            await botClient.SendStickerAsync(chatId, InputFile.FromUri("https://chpic.su/_data/stickers/n/narutostilpacks/narutostilpacks_001.webp"), cancellationToken: cancellationToken);
            break;
        case "Видео":
            await botClient.SendVideoAsync(chatId, InputFile.FromUri("https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-countdown.mp4"), cancellationToken: cancellationToken);
            break;
        case "Аниме топ?":
            await botClient.SendPollAsync(chatId, "Аниме топ?", new[] { "Да", "Нет" }, cancellationToken: cancellationToken);
            break;
        default:
            await SendInlineKeyboard(botClient, chatId, cancellationToken);
            break;
    }
}

async Task SendInlineKeyboard(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
{
    var inlineKeyboard = new InlineKeyboardMarkup(new[]
    {
        new [] { InlineKeyboardButton.WithCallbackData("Проверка") },
        new [] { InlineKeyboardButton.WithCallbackData("Привет") },
        new [] { InlineKeyboardButton.WithCallbackData("Картинка") },
        new [] { InlineKeyboardButton.WithCallbackData("Стикер") },
        new [] { InlineKeyboardButton.WithCallbackData("Видео") },
        // Добавлена новая кнопка для опроса
        new [] { InlineKeyboardButton.WithCallbackData("Аниме топ?") },
    });

    await botClient.SendTextMessageAsync(chatId, "Выберите опцию:", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка API Telegram:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
