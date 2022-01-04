using EntityStorage;

namespace SimpleHandlers.Domain
{
    public class TranslatorModeService: IMode

    {
        public TranslatorMode TranslatorMode => TranslatorMode.EFOnly;
    }
}