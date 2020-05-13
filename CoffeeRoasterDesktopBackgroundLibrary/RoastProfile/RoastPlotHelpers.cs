using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary.RoastProfile
{
    public static class RoastPlotHelpers
    {
        public static List<Tuple<double, double>> UpdateTemperaturePlotPoints(List<RoastPoint> roastPoints)
        {
            var temperaturePlotPoints = new List<Tuple<double, double>>();
            var priorTempSet = false;
            double priorTemp = 0;
            double timeCounter = 0;

            foreach (var rp in roastPoints)
            {
                if (!priorTempSet)
                {
                    priorTempSet = true;
                    priorTemp = rp.Temperature;

                    for (var i = rp.StartSeconds; i < rp.EndSeconds; i++)
                    {
                        temperaturePlotPoints.Add(new Tuple<double, double>(timeCounter, priorTemp));
                        timeCounter++;
                    }
                    continue;
                }

                var deltaTemperature = rp.Temperature - priorTemp;
                var deltaSeconds = rp.EndSeconds - rp.StartSeconds;
                if (deltaSeconds <= 0)
                    return temperaturePlotPoints;

                var temperatureStep = deltaTemperature / deltaSeconds;

                for (var j = rp.StartSeconds; j <= rp.EndSeconds; j++)
                {
                    priorTemp += temperatureStep;
                    temperaturePlotPoints.Add(new Tuple<double, double>(timeCounter, priorTemp));
                    timeCounter++;
                }
            }

            return temperaturePlotPoints;
        }
    }
}