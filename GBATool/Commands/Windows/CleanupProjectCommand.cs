using ArchitectureLibrary.Commands;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.VOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GBATool.Commands.Windows;

public class CleanupProjectCommand : Command
{
    public override async Task ExecuteAsync(object? parameter)
    {
        List<Task> tasks = [];

        #region BankModel
        List<FileModelVO> bankModelVOs = ProjectFiles.GetModels<BankModel>();

        foreach (FileModelVO vo in bankModelVOs)
        {
            if (vo.Model is BankModel bank)
            {
                bool somethingRemoved = bank.CleanUpDeletedSprites();

                if (somethingRemoved)
                {
                    tasks.Add(ProjectFiles.SaveModelAsync(bank.GUID, bank));
                }
            }
        }
        #endregion

        #region CharacterModel
        List<FileModelVO> characterModelVOs = ProjectFiles.GetModels<CharacterModel>();

        foreach (FileModelVO vo in characterModelVOs)
        {
            if (vo.Model is CharacterModel character)
            {
                bool somethingRemoved = character.CleanUp();

                if (somethingRemoved)
                {
                    tasks.Add(ProjectFiles.SaveModelAsync(character.GUID, character));
                }
            }
        }
        #endregion

        await Task.WhenAll(tasks);

        MessageBox.Show("All missing object were removed", "Info", MessageBoxButton.OK);
    }
}