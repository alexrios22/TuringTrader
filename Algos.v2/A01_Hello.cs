﻿//==============================================================================
// Project:     TuringTrader, simulator core v2
// Name:        A01_Hello
// Description: Develop & test 'hello world' algorithm.
// History:     2021iv23, FUB, created
//------------------------------------------------------------------------------
// Copyright:   (c) 2011-2021, Bertram Enterprises LLC
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
using TuringTrader.Optimizer;
using TuringTrader.SimulatorV2;
#endregion

// NOTE: the public interface between an application and the various
// algorithms is identical between simulators v1 and v2. Consequently,
// existing v1 algorithms won't need to be updated and can co-exist
// with v2 algorithms for a while.

namespace TuringTrader.DemoV2
{
    public class A01_Hello : Algorithm
    {
        [OptimizerParam(1, 10, 1)]
        public int TEST_PARAM { get; set; } = 1;

        public override string Name => "A01_Hello";

        public override void Run() => Output.WriteLine("Hello Trader. TEST_PARAM = {0}", TEST_PARAM);
        public override void Report() => Output.WriteLine("Here is your report");
    }
}

//==============================================================================
// end of file
