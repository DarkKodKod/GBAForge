using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands.Menu;

public class DeleteElementCommand : ItemSelectedCommand
{
    public override bool CanExecute(object? parameter)
    {
        if (ItemSelected == null)
        {
            return false;
        }

        if (ItemSelected.IsRoot)
        {
            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (ItemSelected == null)
        {
            return;
        }

        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this element?", "Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new DeleteProjectItemHistoryAction(ItemSelected));

            ProjectItemFileSystem.DeteElement(ItemSelected);

            SignalManager.Get<DeleteElementSignal>().Dispatch(ItemSelected);
        }
    }
}
