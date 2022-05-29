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
        private Direction direction;
        private string destinationFloor;
        private Elevator elevator;
        private const double tolerance = 0.1f;
        private const float activeVelocity = 2.0f;

        public Program()
        {
            // Piston 1, 2 and 3
            // 1st floor: 0m + 0m + 1,8m
            // 2nd floor: 10m + 6,8m + 0m
            // 3rd floor: 10m + 10m + 6,8m

            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            pistons.Add(GridTerminalSystem.GetBlockWithName("_piston_1") as IMyPistonBase);
            pistons.Add(GridTerminalSystem.GetBlockWithName("_piston_2") as IMyPistonBase);
            pistons.Add(GridTerminalSystem.GetBlockWithName("_piston_3") as IMyPistonBase);

            Dictionary<string, double> floors = new Dictionary<string, double>();
            floors.Add("floor1", 1.8f);
            floors.Add("floor2", 16.8f);
            floors.Add("floor3", 26.8f);

            IMyTextPanel lcdDisplay = GridTerminalSystem.GetBlockWithName("_output_1") as IMyTextPanel;

            elevator = new Elevator(pistons, floors, lcdDisplay);
            
            Runtime.UpdateFrequency = UpdateFrequency.None;

            if(elevator.IsInitializedErrorFree)
            {
                if(lcdDisplay != null)
                {
                    lcdDisplay.WriteText("Elevator initilized successfully.");
                }
                Echo("Elevator initilized successfully.");
            }
            else
            {
                if (lcdDisplay != null)
                {
                    lcdDisplay.WriteText("An error occured during elevator initializing: " + elevator.ErrorMessage);
                }
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
                        elevator.StopElevator();
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
