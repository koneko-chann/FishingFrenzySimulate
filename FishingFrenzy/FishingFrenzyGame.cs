using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingFrenzy
{
    public class FishingFrenzyGame
    {
        public const int FPS = 20;
        public const double ADD_FRAM_REPLAY_DELAY = 1f / 20f;
        public double OneSecondCounter { get; set; }
        public double AmountEnergy { get; set; }
        public double AmountMoney { get; set; }
        public GameComponent Fish { get; set; }=new GameComponent(100);
        public GameComponent Net { get; set; } = new GameComponent(200);
        public double EnergyFillRate { get; set; } = 0.21;
        public double EnergyDrainRate { get; set; } = 0.045;
        public int CatchFishTime { get; set; } = 20;
        public int Speed { get; set; } = 150;
        public List<List<int>> Frs { get; set; } = new List<List<int>>();
        public FishingFrenzyGame(double energyFillRate,double energyDrainRate)
        {
            this.EnergyFillRate = energyFillRate;
            this.EnergyDrainRate = energyDrainRate;
        }
        private bool IsFishInNet()
        {
            double fishTop = Fish.PositionY + Fish.Height / 2f;
            double fishBottom = Fish.PositionY - Fish.Height / 2f;
            double netTop = Net.PositionY + Net.Height / 2f;
            double netBottom = Net.PositionY - Net.Height / 2f;
            return (fishBottom >= netBottom) && (fishTop <= netTop);
        }
        private void RandomPosition(MoveDirection moveDirection)
        {
            Fish.PositionY=new Random().Next(0,400);
            Net.PositionY = Fish.PositionY + new Random().Next(-100, 100);
        }
        public void AddFrameToReplay(int e = -1, int t = 0)
        {
            RandomPosition(MoveDirection.KeepDirection);
            if (e != -1 && t != 0)
            {
                Frs.Add(new List<int>
            {
            (int)Math.Round(Fish.PositionY), 
            (int)Math.Round(Net.PositionY), 
            e, 
            t  
        });
            }
            else
            {
                Frs.Add(new List<int>
        {
            (int)Math.Round(Fish.PositionY),
            (int)Math.Round(Net.PositionY)
        });
            }
        }
        public void EmulateFishPosition(MoveDirection moveDirection)
        {

        }
        public string Update(double deltaTime)
        {
            string jsonMessage = "";
            OneSecondCounter += ADD_FRAM_REPLAY_DELAY;
            if (Net != null && Fish != null)
            {
                if (IsFishInNet())
                {
                    AmountEnergy += EnergyFillRate * deltaTime;
                    AmountEnergy = Math.Min(1, AmountEnergy); 
                }
                else
                {
                    AmountEnergy -= EnergyDrainRate * deltaTime;
                    AmountEnergy = Math.Max(0, AmountEnergy); 
                }

                if (AmountEnergy >= 1)
                {
                    var message = new
                    {
                        cmd = "end",
                        rep = Frs, 
                        en = AmountEnergy
                    };
                    jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                }
            }

            if (OneSecondCounter >= ADD_FRAM_REPLAY_DELAY)
            {
                AddFrameToReplay(-1, 0);
                OneSecondCounter = 0;
            }
            return jsonMessage;
        }

    }
    public class GameComponent
    {
        public int Height { get; set; }
        public double PositionY { get; set; }
        public int PreviousPositionY { get; set; }
        public GameComponent(int height)
        {
            Height = height;
        }
    }
    public enum MoveDirection
    {
        Up=1,
        KeepDirection=0,
        Down=-1
    }
}
