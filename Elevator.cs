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
            private IMyTextPanel _lcdMonitor;
            private Direction _direction = Direction.none;
            private Floor _destinationFloor;
            private double _totalPistonPosition = 0f;
            private bool _isInitializedErrorFree = true;
            private string _errorMessage = string.Empty;

            private float _startPosition = 0.0f;
            private float _tolerance = 0.05f;
            private float _maxVelocity = 5.0f;
            private float _minVelocity = 0.5f; 
            private float _accelleration = 0.5f;

            // =========================[ CONSTRUCTOR ]========================= \\
            internal Elevator(List<IMyPistonBase> pistons, List<Floor> floors, IMyTextPanel textPanel = null)
            {
                this._lcdMonitor = textPanel;

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
                            else
                            {
                                this._floors.Add(floor);
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

            internal float ElevatorPosition
            {
                get
                {
                    this._totalPistonPosition = 0f;

                    foreach (IMyPistonBase p in this._pistons)
                    {
                        this._totalPistonPosition += this.GetPistonPosition(p);
                    }

                    return (float)this._totalPistonPosition;
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
                string tmpName = this._destinationFloor == null ? "null" : this._destinationFloor.Name;
                string tmpDestHeight = this._destinationFloor == null ? "null" : this._destinationFloor.Height.ToString();
                string tmpErrorMsg = this._errorMessage == "" ? "no errors found :-)" : this._errorMessage;

                if (this._lcdMonitor != null)
                {
                    this._lcdMonitor.WriteText(
                        "========[ Elevator Info ]========" +
                        "\nDate & Time: " + DateTime.Now.ToString() +
                        "\nDestination name: " + tmpName +
                        "\nStart position: " + this._startPosition.ToString() +
                        "\nCurrent height: " + this.ElevatorPosition.ToString() +
                        "\nDestination height: " + tmpDestHeight.ToString() +
                        "\nDirection: " + this._direction.ToString() +
                        "\nDestination reached: " + this.IsDestinationReached.ToString() +
                        "\n\nError: " + tmpErrorMsg
                    , false);
                }
            }

            internal bool IsDestinationReached
            {
                get
                {
                    if(this._destinationFloor != null)
                    {
                        if (this.ElevatorPosition >= this._destinationFloor.Height - this._tolerance && this.ElevatorPosition <= this._destinationFloor.Height + this._tolerance)
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

                // Remember the start position for later calculations to get the current velocity.
                this._startPosition = this.ElevatorPosition;

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
                            this._pistons[i].Velocity = this.GetCurrentVelocity();
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
                            this._pistons[i].Velocity = this.GetCurrentVelocity() * -1.0f;
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

            // Calculate active velocity dynamically and relative to the distance that must be covered.
            // We will achieve a smooth start and stop speed.
            internal float GetCurrentVelocity()
            {
                // X must not be zero, because the velocity would be zero forever and the elevator would not start.
                // To avoid this, we add a very tiny value to the result.
                float x = this.ElevatorPosition - this._startPosition;
                float y1, y2;

                if (this._direction == Direction.up)
                {
                    y1 = this._accelleration * x;
                    y2 = this._accelleration * -(x - this._destinationFloor.Height + this._startPosition);
                }
                else if (this._direction == Direction.down)
                {
                    // X must not be negative, else we always would get a negative value
                    // So, when the elevator moves down, we have to change the negative value into an positive.
                    x = x * -1f;
                    y1 = this._accelleration * x;
                    y2 = (this._accelleration * -(x - (this._startPosition - this._destinationFloor.Height)));                    
                }
                else // none
                {
                    return 0f;
                }

                float v = Math.Min(Math.Min(y1, y2), this._maxVelocity);

                if (v < this._minVelocity)
                {
                    return this._minVelocity;
                }
                else
                {
                    return v;
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
