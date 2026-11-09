using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;

namespace GBATool.Commands.Character;

public class MoveFrameBackwardCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;
        string animationID = (string)values[0];
        string frameID = (string)values[1];
        FileHandler fileHandler = (FileHandler)values[2];

        if (fileHandler.FileModel is not CharacterModel model)
        {
            return;
        }

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        List<(string, FrameModel)> frames = [];
        int frameCounter = 0;
        int indexOfElement = 0;

        foreach (KeyValuePair<string, FrameModel> item in animation.Frames)
        {
            frames.Add((item.Key, item.Value));

            if (item.Value.ID == frameID)
            {
                indexOfElement = frameCounter;

                // can't move further
                if (indexOfElement == 0)
                {
                    return;
                }
            }

            frameCounter++;
        }

        MoveBackward(frames, indexOfElement);

        Dictionary<string, FrameModel> newFrames = [];

        foreach ((string key, FrameModel value) item in frames)
        {
            newFrames.Add(item.key, item.value);
        }

        animation.Frames = newFrames;

        fileHandler.Save();

        SignalManager.Get<MoveFrameBackwardSignal>().Dispatch(animation.ID, indexOfElement);
    }

    private static void MoveBackward(List<(string, FrameModel)> list, int index)
    {
        if (index > 0 && index < list.Count)
        {
            (list[index], list[index - 1]) = (list[index - 1], list[index]);
        }
    }
}
