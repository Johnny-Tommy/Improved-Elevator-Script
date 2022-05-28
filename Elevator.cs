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
        internal class Elevator
        {
            private List<IMyPistonBase> _pistons = new List<IMyPistonBase>();
            private IMyTextPanel _lcdMonitor = null;
            private double _totalPistonPosition = 0f;

            private const double _tolerance = 0.1f;
            private const float _activeVelocity = 2.0f;

            private bool _destinationReached;

            internal Elevator(List<IMyPistonBase> pistons)
            {
                foreach(IMyPistonBase p in pistons)
                {
                    this._pistons.Add(p);
                }
            }

            private double GetPistonPosition(IMyPistonBase piston)
            {
                string info = piston.DetailedInfo;
                string[] infoSplit = info.Split(':');
                string extractedValue = infoSplit[1].Substring(0, infoSplit[1].Length - 1);

                double dValue;

                if (Double.TryParse(extractedValue, out dValue))
                {
                    return dValue;
                }
                else
                {
                    //Echo("ERROR! (Cannot parse " + extractedValue);
                    return -1.0f;
                }
            }

            internal double TotalPistonPosition
            {
                get
                {
                    this._totalPistonPosition = 0f;

                    foreach (IMyPistonBase p in this._pistons)
                    {
                        this._totalPistonPosition += this.GetPistonPosition(p);
                    }

                    return this._totalPistonPosition;
                }
            }

            internal void StopPistons()
            {
                for (int i = 0; i < this._pistons.Count; i++)
                {
                    this._pistons[i].Velocity = 0.0000f;
                }
            }

            internal void UpdateLcdScreen()
            {
                if(this._lcdMonitor != null)
                {
                    this._lcdMonitor.WriteText("");
                }
            }
        }
    }

    internal enum Direction
    {
        up,
        down,
        none
    }
}
