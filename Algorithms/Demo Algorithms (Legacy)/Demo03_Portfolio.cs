﻿//==============================================================================
// Project:     TuringTrader, demo algorithms
// Name:        Demo03_Portfolio
// Description: portfolio trading demo
// History:     2018xii10, FUB, created
//------------------------------------------------------------------------------
// Copyright:   (c) 2011-2023, Bertram Enterprises LLC dba TuringTrader.
//              https://www.turingtrader.org
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuringTrader.Simulator;
using TuringTrader.Indicators;
#endregion

namespace TuringTrader.Demos
{
    public class Demo03_Portfolio: Algorithm
    {
        #region internal data
        private Plotter _plotter = new Plotter();
        private readonly string _template = "SimpleChart";
        private readonly double _initialCash = 100000.00;
        private double _initialSpx = 0.0;
        private readonly string _spx = "SPY";
        private readonly List<string> _universe = new List<string>()
        {
            "XLY", "XLV", "XLK",
            "XLP", "XLE", "XLI",
            "XLF", "XLU", "XLB",
        };
        #endregion

        override public void Run()
        {
            //---------- initialization

            // set simulation time frame
            StartTime = DateTime.Parse("01/01/2007", CultureInfo.InvariantCulture);
            EndTime = DateTime.Parse("12/01/2018", CultureInfo.InvariantCulture);

            // set initial account value
            Deposit(_initialCash);

            // add instruments
            AddDataSource(_spx);
            foreach (string nickname in _universe)
                AddDataSource(nickname);

            //---------- simulation

            // loop through all bars
            foreach (DateTime simTime in SimTimes)
            {
                if (_initialSpx == 0.0)
                    _initialSpx = FindInstrument(_spx).Open[0];

                // this list of instruments is dynamic: the simulator engine
                // adds a new instrument whenever needed. we need to weed out
                // stale instruments, as well as our benchmark instrument
                var activeInstruments = Instruments
                        .Where(i => i.Time[0] == simTime
                            && _universe.Contains(i.Nickname));

                // calculate indicators for all active instruments.
                // the result is stored to a list, to make sure indicators
                // are evaluated exactly once per bar
                var evalInstruments = activeInstruments
                    .Select(i => new
                    {
                        instrument = i,
                        romad = i.Close.ReturnOnMaxDrawdown(252)[0],
                    })
                    .ToList();

                // select instruments, based on the evaluation
                var holdInstruments = evalInstruments
                    .Where(e => e.romad > 0.0)
                    .OrderByDescending(e => e.romad)
                    .Take(5)
                    .Select(e => e.instrument)
                    .ToList();

                double equityPerInstrument = NetAssetValue[0] / Math.Max(holdInstruments.Count, 3);
                foreach (Instrument instr in activeInstruments)
                {
                    // determine equity per instrument
                    double targetEquity = holdInstruments.Contains(instr)
                        ? equityPerInstrument
                        : 0.0;

                    // determine number of shares
                    int targetShares = (int)Math.Floor(targetEquity / instr.Close[0]);
                    int currentShares = instr.Position;

                    // place trades
                    Order newOrder = instr.Trade(targetShares - currentShares);

                    // add comment
                    if (newOrder != null)
                    {
                        if (currentShares == 0)
                            newOrder.Comment = "open";
                        else if (targetShares == 0)
                            newOrder.Comment = "close";
                        else
                            newOrder.Comment = "rebalance";
                    }
                }

                // plot net asset value on Sheet1
                _plotter.SelectChart("Performance vs Time", "date");
                _plotter.SetX(simTime);
                _plotter.Plot(FindInstrument(_spx).Name, FindInstrument(_spx).Close[0] / _initialSpx);
                _plotter.Plot("Net Asset Value", NetAssetValue[0] / _initialCash);

                // we could plot our indicators here
                //foreach (var i in evalInstruments)
                //    _plotter.Plot(i.instrument.Symbol, i.romad);
            }

            //---------- post-processing

            // create a list of trades on Sheet2
            _plotter.SelectChart("trades", "time");
            foreach (LogEntry entry in Log)
            {
                _plotter.SetX(entry.BarOfExecution.Time);
                _plotter.Plot("action", entry.Action);
                _plotter.Plot("instr", entry.Symbol);
                _plotter.Plot("qty", entry.OrderTicket.Quantity);
                _plotter.Plot("fill", entry.FillPrice);
                _plotter.Plot("comment", entry.OrderTicket.Comment ?? "");
            }
        }

        public override void Report()
        {
            _plotter.OpenWith(_template);
        }
    }
}

//==============================================================================
// end of file