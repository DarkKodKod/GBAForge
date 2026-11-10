using GBATool.Models;
using GBATool.Utils;
using GBATool.VOs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBATool.FileSystem;

public static class ProjectFiles
{
    public static ConcurrentDictionary<string, FileHandler> Handlers { get; private set; } = new();

    public static List<FileModelVO> GetModels<T>() where T : AFileModel
    {
        List<FileModelVO> models = [];

        int index = 0;

        foreach (KeyValuePair<string, FileHandler> pair in Handlers)
        {
            if (pair.Value.FileModel is T model)
            {
                models.Add(new()
                {
                    Index = index++,
                    Name = pair.Value.Name,
                    Path = pair.Value.Path,
                    Model = model,
                    Type = Util.GetItemType(pair.Value.FileModel?.FileExtension)
                });
            }
        }

        return models;
    }

    public static async Task SaveModelAsync<T>(string guid, T model) where T : AFileModel
    {
        if (string.IsNullOrEmpty(guid))
        {
            return;
        }

        foreach (KeyValuePair<string, FileHandler> pair in Handlers)
        {
            if (pair.Key == guid)
            {
                pair.Value.FileModel = model;

                await pair.Value.SaveAsync().ConfigureAwait(false);

                break;
            }
        }
    }

    public static FileModelVO? GetFileModel(string guid)
    {
        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }

        foreach (KeyValuePair<string, FileHandler> pair in Handlers)
        {
            if (pair.Key == guid)
            {
                return new FileModelVO()
                {
                    Index = 0,
                    Name = pair.Value.Name,
                    Path = pair.Value.Path,
                    Model = pair.Value.FileModel,
                    Type = Util.GetItemType(pair.Value.FileModel?.FileExtension)
                };
            }
        }

        return null;
    }

    public static T? GetModel<T>(string guid) where T : AFileModel
    {
        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }

        foreach (KeyValuePair<string, FileHandler> pair in Handlers)
        {
            if (pair.Key == guid &&
                pair.Value.FileModel is T model)
            {
                return model;
            }
        }

        return null;
    }
}
