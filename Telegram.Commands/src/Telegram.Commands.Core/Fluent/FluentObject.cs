﻿namespace Telegram.Commands.Core.Fluent;

public class FluentObject<TObject>
{
    public FluentObject(TObject o)
    {
        Object = o;
        CurrentStateId = null;
    }

    public string CurrentStateId { get; set; }
    public TObject Object { get; set; }
}