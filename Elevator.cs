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
            private List<Floor> _floors = new List<Floor>();
            private IMyTextPanel _lcdMonitor = null;
            private Direction _direction = Direction.none;
            private Floor _destinationFloor;
            private double _totalPistonPosition = 0f;
            //private double _destination = 0f;
            private bool _isInitializedErrorFree = true;
            private string _errorMessage = string.Empty;
            private const double _tolerance = 0.1f;
            private const float _activeVelocity = 2.0f;

            // =========================[ CONSTRUCTOR ]========================= \\
            internal Elevator(List<IMyPistonBase> pistons, List<Floor> floors, IMyTextPanel textPanel = null)
            {
                if(textPanel != null)
                {
                    this._lcdMonitor = textPanel;
                }

                // Allocate Pistons
                foreach(IMyPistonBase piston in pistons)
                {
                    if(piston == null)
                    {
                        this._errorMessage = "Cannot allocate all pistons.";
                        this._isInitializedErrorFree = false;
                        if(this._lcdMonitor != null)
                        {
                            this._lcdMonitor.WriteText("Elevator initialization failed!\n" + this._errorMessage);
                        }
                    }
                    else
                    {
                        this._pistons.Add(piston);
                    }                        
                }

                // Checking floors
                if(this._isInitializedErrorFree)
                {
                    if(floors.Count >= 2)
                    {
                        // Check maximum heigth wich can be reach with N pistons (1 piston = 10 units) and allocate the value if it is ok.
                        foreach(Floor floor in floors)
                        {
                            if (floor.Height > (this._pistons.Count * 10))
                            {
                                this._errorMessage = "The floor " + floor.Name + " cannot be reached, because there are not enough pistons.";
                                this._isInitializedErrorFree = false;
                                if (this._lcdMonitor != null)
                                {
                                    this._lcdMonitor.WriteText("Elevator initialization failed!\n" + this._errorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        this._errorMessage = "You need at least two floors!";
                        this._isInitializedErrorFree = false;
                        if (this._lcdMonitor != null)
                        {
                            this._lcdMonitor.WriteText("Elevator initialization failed!\n" + this._errorMessage);
                        }
                    }
                }
            } // End of constructor

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
                    this._errorMessage = "(Cannot parse " + extractedValue + ")";
                    return -1.0f;
                }
            }

            internal string ErrorMessage
            {
                get
                {
                    return this._errorMessage;
                }
            }

            internal bool IsInitializedErrorFree
            {
                get
                {
                    return this._isInitializedErrorFree;
                }
            }

            internal double ElevatorPosition
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

            internal void StopElevator()
            {
                for (int i = 0; i < this._pistons.Count; i++)
                {
                    this._pistons[i].Velocity = 0.0000f;
                }

                this._direction = Direction.none;
                this._destinationFloor = null;
            }

            internal void UpdateLcdScreen()
            {
                if(this._lcdMonitor != null)
                {
                    this._lcdMonitor.WriteText(
                        "========[ Elevator Info ]========" +
                        "\nDate & Time:" + DateTime.Now.ToString() +
                        "\nDestination name: " + this._destinationFloor == null ? "null" : this._destinationFloor.Name +
                        "\nDestination height: " + this._destinationFloor == null ? "null" : this._destinationFloor.Height +
                        "\nCurrent height: " + this.ElevatorPosition +
                        "\nDirection: " + this._direction.ToString() +
                        "\nDestination reached: " + this.IsDestinationReached.ToString() +
                        "\n\nError: " + this.ErrorMessage == "" ? "no errors found :-)" : this._errorMessage
                    , false); ;
                }
            }

            internal bool IsDestinationReached
            {
                get
                {
                    if(this._destinationFloor != null)
                    {
                        if (this.ElevatorPosition >= this._destinationFloor.Height - tolerance && this.ElevatorPosition <= this._destinationFloor.Height + tolerance)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Destination floor wasn't found.
                        // Stops elevator indirectly by pretending that elevator has reached the target assuming that
                        // the main program stops the lift if the destination will be reached.
                        return true;
                    }
                }
            }

            internal bool SetDestination(string floor)
            {
                // Search a certain element:
                this._destinationFloor = this._floors.Find(f => f.Name == floor);                

                if (this._destinationFloor != null)
                {
                    if (this._destinationFloor.Height > this.ElevatorPosition)
                    {
                        this._direction = Direction.up;
                    }
                    else
                    {
                        this._direction = Direction.down;
                    }

                    return true;
                }
                else
                {
                    this._errorMessage = "Cannot reach floor " + floor + " or this floor does not exist.";
                    this._direction = Direction.none;
                    return false;
                }
            }

            internal void Move()
            {
                if(this._direction == Direction.up)
                {
                    for(int i = 0; i < this._pistons.Count; i++)
                    {
                        if(this.GetPistonPosition(this._pistons[i]) < 10.0f)
                        {
                            this._pistons[i].Velocity = _activeVelocity;
                            break;
                        }
                        else
                        {
                            this._pistons[i].Velocity = 0.0f;
                        }
                    }
                }
                else if(this._direction == Direction.down)
                {
                    for (int i = this._pistons.Count - 1; i >= 0; i--)
                    {
                        if (this.GetPistonPosition(this._pistons[i]) > 0.0f)
                        {
                            this._pistons[i].Velocity = _activeVelocity * -1.0f;
                            break;
                        }
                        else
                        {
                            this._pistons[i].Velocity = 0.0f;
                        }
                    }
                }
                else
                {
                    this.StopElevator();
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
