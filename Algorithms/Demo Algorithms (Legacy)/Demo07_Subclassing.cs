﻿//==============================================================================
// Project:     TuringTrader, demo algorithms
// Name:        Demo07_Subclassing
// Description: demonstrate subclassable algorithms
// History:     2019v21, FUB, created
//------------------------------------------------------------------------------
// Copyright:   (c) 2011-2020, Bertram Solutions LLC
//              https://www.bertram.solutions
// License:     This file is part of TuringTrader, an open-source backtesting
//              engine/ market simulator.
//              TuringTrader is free software: you can redistribute it and/or 
//              modify it under the terms of the GNU Affero General Public 
//              License as published by the Free Software Foundation, either 
//              version 3 of the License, or (at your option) any later version.
//              TuringTrader is distributed in the hope that it will be useful,
//              but WITHOUT ANY WARRANTY; without even the implied warranty of
//              MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//              GNU Affero General Public License for more details.
//              You should have received a copy of the GNU Affero General Public
//              License along with TuringTrader. If not, see 
//              https://www.gnu.org/licenses/agpl-3.0.
//==============================================================================

#region libraries
using System;
using System.Globalization;
using System.Collections.Generic;
using TuringTrader.Simulator;
using TuringTrader.Indicators;
#endregion

namespace Demos
{
    #region Demo07_Subclassing_Child
    // note how this class is not declared public
    // because of this, the class will not show up in TuringTrader's
    // algorithm selector, but it can still be instantiated
    class Demo07_Subclassing_Child : Algorithm
    {
        private static readonly string SPX = "$SPX";
        private Plotter _plotter = new Plotter();

        public override string Name => "Algo as DataSource";

        public override IEnumerable<Bar> Run(DateTime? startTime, DateTime? endTime)
        {
            // startTime and endTime are passed in from the parent data-source, 
            // and only in case we are sub-classed.
            // if running stand-alone, we are free to set any values
            StartTime = startTime ?? DateTime.Parse("01/01/2008");
            EndTime = endTime ?? DateTime.Now.Date - TimeSpan.FromDays(5);

            AddDataSource(SPX);
            Instrument spx = null;

            foreach (var s in SimTimes)
            {
                spx = spx ?? FindInstrument(SPX);


                if (IsDataSource)
                {
                    // this is only relevant, if we are running
                    // stand-alone. it is good practice not to write
                    // to the plotter when running as a data source.
                    _plotter.SelectChart(Name, "date");
                    _plotter.SetX(SimTime[0]);
                    _plotter.Plot(spx.Name, spx.Close[0]);
                }

                // this is where the sub-classed algorithm
                // adds a new bar to the parent data-source
                var bar = Bar.NewOHLC(
                    Name, SimTime[0],
                    spx.Close[0] / 10.0, spx.Close[0] / 10.0, spx.Close[0] / 10.0, spx.Close[0] / 10.0, 0);

                yield return bar;
            }
        }

        public override void Report()
        {
            // this is only called when the algorithm
            // is run stand-alone, never when sub-classed
            _plotter.OpenWith("SimpleChart");
        }
    }
    #endregion

    #region Demo07_Subclassing
    public class Demo07_Subclassing : Algorithm
    {
        private static readonly string DATA = "algo:Demo07_Subclassing_Child";
        private Plotter _plotter = new Plotter();
        public override void Run()
        {
            StartTime = DateTime.Parse("01/01/2008", CultureInfo.InvariantCulture);
            EndTime = DateTime.Now.Date - TimeSpan.FromDays(5);

            // our sub-classed data source is used exactly
            // the same way as any other data source
            AddDataSource(DATA);
            Instrument data = null;

            foreach (var s in SimTimes)
            {
                data = data ?? FindInstrument(DATA);

                _plotter.SelectChart(Name, "date");
                _plotter.SetX(SimTime[0]);
                _plotter.Plot(data.Name, data.Close[0]);
                _plotter.Plot("SMA( " + data.Name + " )", data.Close.SMA(200)[0]);
            }
        }

        public override void Report()
        {
            _plotter.OpenWith("SimpleChart");
        }
    }
    #endregion
}

//==============================================================================
// end of file