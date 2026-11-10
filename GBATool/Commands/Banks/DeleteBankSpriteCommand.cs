using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands.Banks;

public class DeleteBankSpriteCommand : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;

        return values[0] is SpriteModel;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        SpriteModel sprite = (SpriteModel)values[0];
        BankModel model = (BankModel)values[1];
        bool isForced2D = (bool)values[2];

        _ = BankModel.MetaDataCache.TryRemove((model.GUID, isForced2D), out _);
        _ = BankModel.MetaDataCache.TryRemove((model.GUID, !isForced2D), out _);

        SignalManager.Get<BankSpriteDeletedSignal>().Dispatch(sprite);
    }
}
