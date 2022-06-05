using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        internal class Floor
        {
            internal string Name { get; private set; }
            internal double Height { get; private set; }
            // A optional list with doors which will open automatically until the elevator reached its destination
            // and will be closed when the elevator starts moving.
            internal List<IMyDoor> Doors { get; private set; } = new List<IMyDoor>();

            internal Floor(string name, double height, IMyBlockGroup blockGroup = null)
            {
                this.Name = name;
                this.Height = height;

                if (blockGroup != null)
                {
                    blockGroup.GetBlocksOfType<IMyDoor>(this.Doors);
                }
            }
        }
    }
}
