using System.Threading.Tasks;

namespace GBATool.Models;

public class FileHandler
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public AFileModel? FileModel { get; set; }

    public void Save() => FileModel?.Save(Path, Name);
    public async Task SaveAsync()
    {
        if (FileModel == null)
        {
            return;
        }

        await FileModel.SaveAsync(Path, Name).ConfigureAwait(false);
    }

}
