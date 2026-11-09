using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;

namespace GBATool.Commands.Character;

public class DeleteAnimationFrameCommand : Command
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

        if (model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            int frameIndex = 0;
            foreach (KeyValuePair<string, FrameModel> item in animation.Frames)
            {
                if (item.Value.ID == frameID)
                {
                    break;
                }
                frameIndex++;
            }

            if (animation.Frames.Remove(frameID))
            {
                SignalManager.Get<DeleteAnimationFrameSignal>().Dispatch(animationID, frameIndex);

                fileHandler.Save();
            }
        }
    }
}
