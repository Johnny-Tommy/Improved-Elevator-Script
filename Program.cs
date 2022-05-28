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
        private IMyPistonBase piston3;
        private IMyPistonBase piston2;
        private IMyPistonBase piston1;
        private IMyTextPanel output1;
        private Direction direction;
        private string destinationFloor;
        private bool destinationReached;
        private string stopReason;

        private const double tolerance = 0.1f;
        private const float activeVelocity = 2.0f;

        public Program()
        {
            // The constructor
            this.piston3 = GridTerminalSystem.GetBlockWithName("_piston_3") as IMyPistonBase;
            this.piston2 = GridTerminalSystem.GetBlockWithName("_piston_2") as IMyPistonBase;
            this.piston1 = GridTerminalSystem.GetBlockWithName("_piston_1") as IMyPistonBase;

            this.output1 = GridTerminalSystem.GetBlockWithName("_output_1") as IMyTextPanel;

            Storage = "0.0";
            this.destinationReached = false;
            this.stopReason = "constructor";

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string arg, UpdateType updateSource)
        {
            // Piston 1, 2 and 3
            // 1st floor: 0m + 0m + 1,8m
            // 2nd floor: 10m + 6,8m + 0m
            // 3rd floor: 10m + 10m + 6,8m

            if (Runtime.UpdateFrequency == UpdateFrequency.None)
            {
                switch (arg)
                {
                    case "floor3":
                        Storage = "26.8";
                        destinationFloor = "=[ 3 ]=";
                        this.destinationReached = false;
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        break;
                    case "floor2":
                        Storage = "16.8";
                        destinationFloor = "=[ 2 ]=";
                        this.destinationReached = false;
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        break;
                    case "floor1":
                        Storage = "1.8";
                        destinationFloor = "=[ 1 ]=";
                        this.destinationReached = false;
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        break;
                    default:
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        this.stopReason = "initialized";
                        Storage = GetTotalPistonPosition().ToString();
                        destinationFloor = "=[ INIT ]=";
                        this.direction = Direction.none;
                        this.destinationReached = true;
                        StopPistons();
                        break;
                }
            }

            double destinationTotal = Double.Parse(Storage);

            if (destinationTotal != GetTotalPistonPosition() && Storage != "0.0")
            {
                if (destinationTotal > GetTotalPistonPosition())
                {
                    direction = Direction.up;

                    if (GetPistonPosition(piston1) < 10.0f)
                    {
                        piston1.Velocity = activeVelocity;
                    }
                    else if (GetPistonPosition(piston2) < 10.0f)
                    {
                        piston2.Velocity = activeVelocity;
                    }
                    else if (GetPistonPosition(piston3) < 10.0f)
                    {
                        piston3.Velocity = activeVelocity;
                    }

                    if (GetTotalPistonPosition() >= destinationTotal - tolerance && GetTotalPistonPosition() <= destinationTotal + tolerance)
                    {
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        this.stopReason = "Reached floor " + destinationFloor;
                        direction = Direction.none;
                        this.destinationReached = true;
                        StopPistons();
                        Storage = "0.0";
                    }
                }
                else
                {
                    direction = Direction.down;

                    if (GetPistonPosition(piston3) > 0.0f)
                    {
                        piston3.Velocity = activeVelocity * -1.0f;
                    }
                    else if (GetPistonPosition(piston2) > 0.0f)
                    {
                        piston2.Velocity = activeVelocity * -1.0f;
                    }
                    else if (GetPistonPosition(piston1) > 0.0f)
                    {
                        piston1.Velocity = activeVelocity * -1.0f;
                    }

                    if (GetTotalPistonPosition() >= destinationTotal - tolerance && GetTotalPistonPosition() <= destinationTotal + tolerance)
                    {
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        this.stopReason = "Reached floor " + destinationFloor;
                        direction = Direction.none;
                        this.destinationReached = true;
                        StopPistons();
                        Storage = "0.0";
                    }
                }
            }
            else
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                this.stopReason = "Reached floor " + destinationFloor;
                direction = Direction.none;
                this.destinationReached = true;
                StopPistons();
                Storage = "0.0";
            }

            this.output1.WriteText("Destination floor: " + destinationFloor +
                                                "\nDestination height: " + destinationTotal.ToString() +
                                                "\nCurrent height: " + GetTotalPistonPosition().ToString() +
                                                "\nDirection: " + direction.ToString() +
                                                "\nDestination reached: " + destinationReached.ToString() +
                                                "\n" + DateTime.Now.ToString() +
                                                "\nVelo. Piston3: " + this.piston3.Velocity.ToString() +
                                                "\nVelo. Piston2: " + this.piston2.Velocity.ToString() +
                                                "\nVelo. Piston1: " + this.piston1.Velocity.ToString() +
                                                "\nStorage: " + Storage +
                                                "\nArg: " + arg +
                                                "\nStop reason: " + this.stopReason
                                               , false);
        }
    }
}
