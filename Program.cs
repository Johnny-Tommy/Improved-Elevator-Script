/*
 * Improved Elevator Script for Space Engineers by Johannes Thom (2022) 
 * 
 */
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
    partial class Program : MyGridProgram
    {
        private Elevator elevator;

        // ===[Constructor]=== \\
        public Program()
        {
            // Piston 1, 2 and 3
            // 1st floor: 0m + 0m + 1,8m
            // 2nd floor: 10m + 6,8m + 0m
            // 3rd floor: 10m + 10m + 6,8m

            IMyBlockGroup elevatorObjects = GridTerminalSystem.GetBlockGroupWithName("main_elevator");

            List<Floor> floors = new List<Floor>();
            floors.Add(new Floor("floor1", 1.8f, GridTerminalSystem.GetBlockGroupWithName("grpFloor1")));
            floors.Add(new Floor("floor2", 16.8f, GridTerminalSystem.GetBlockGroupWithName("grpFloor2")));
            floors.Add(new Floor("floor3", 26.8f, GridTerminalSystem.GetBlockGroupWithName("grpFloor3")));

            elevator = new Elevator(elevatorObjects, floors);
            elevator.StartDelay = 0;

            Runtime.UpdateFrequency = UpdateFrequency.None;

            if(elevator.IsInitializedErrorFree)
            {
                Echo("Elevator initilized successfully.");
            }
            else
            {
                Echo("An error occured during elevator initializing: " + elevator.ErrorMessage);
            }
        }

        public void Main(string arg, UpdateType updateSource)
        {
            if (elevator.IsInitializedErrorFree)
            {
                if (Runtime.UpdateFrequency == UpdateFrequency.None)
                {
                    if (elevator.SetDestination(arg))
                    {
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    }
                    else
                    {
                        Echo(elevator.ErrorMessage);
                    }
                }
                else
                {
                    if (elevator.IsDestinationReached)
                    {
                        elevator.Stop();
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                    }
                    else
                    {
                        elevator.Move();
                    }
                }
            }

            elevator.UpdateLcdScreen();
        }
    }
}
