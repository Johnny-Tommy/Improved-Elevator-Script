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
            internal float Height { get; private set; }
            // A optional list with doors which will open automatically until the elevator reached its destination
            // and will be closed when the elevator starts moving.
            internal List<IMyDoor> Doors { get; private set; } = new List<IMyDoor>();
            internal List<IMyTextPanel> Displays { get; private set; } = new List<IMyTextPanel>();

            internal Floor(string name, float height, IMyBlockGroup blockGroup = null)
            {
                this.Name = name;
                this.Height = height;

                if (blockGroup != null)
                {
                    blockGroup.GetBlocksOfType<IMyDoor>(this.Doors);
                    blockGroup.GetBlocksOfType<IMyTextPanel>(this.Displays);

                    // Setup the displays
                    foreach(IMyTextPanel display in this.Displays)
                    {
                        display.ContentType = ContentType.TEXT_AND_IMAGE;
                        display.Font = "DEBUG";
                        display.FontSize = 8f;
                        display.BackgroundColor = new Color(0, 0, 0);
                        display.FontColor = new Color(255, 255, 255);
                        display.Alignment = TextAlignment.CENTER;
                        display.WriteText("Initialized!");
                    }
                }
            }

            internal void SetStatus(Status status)
            {
                switch(status)
                {
                    case Status.Coming:
                        this.ShowStatusOnDisplay("Elevator is coming...", Color.LightYellow);
                        this.CloseAllDoors();
                        break;

                    case Status.Busy:
                        this.ShowStatusOnDisplay("Elevator is busy.", Color.Orange);
                        this.CloseAllDoors();
                        break;

                    case Status.Arrived:
                        this.ShowStatusOnDisplay("Elevator arrived.", Color.LightGreen);
                        this.OpenAllDoors();
                        break;

                    case Status.Idle:
                        this.ShowStatusOnDisplay("idle -.-zzZ", Color.White);
                        break;

                    default:
                        this.ShowStatusOnDisplay("default o_O", Color.Red);
                        break;
                }
            }

            private void ShowStatusOnDisplay(string text, Color fontColor)
            {
                foreach (IMyTextPanel display in this.Displays)
                {
                    display.FontColor = fontColor;
                    display.WriteText(text);
                }
            }

            private void OpenAllDoors()
            {
                foreach (IMyDoor door in this.Doors)
                {
                    door.OpenDoor();
                }
            }

            internal void CloseAllDoors()
            {
                foreach (IMyDoor door in this.Doors)
                {
                    door.CloseDoor();
                }
            }

            internal enum Status
            {
                Busy,
                Coming,
                Arrived,
                Idle
            }
        }
    }
}
