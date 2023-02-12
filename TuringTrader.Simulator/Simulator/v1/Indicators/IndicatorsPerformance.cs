﻿//==============================================================================
// Project:     TuringTrader, simulator core
// Name:        IndicatorsPerformance
// Description: collection of performance indicators
// History:     2018xii10, FUB, created
//------------------------------------------------------------------------------
// Copyright:   (c) 2011-2023, Bertram Enterprises LLC dba TuringTrader.
//              https://www.turingtrader.org
// License:     This file is part of TuringTrader, an open-source backtesting
//              engine/ trading simulator.
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
using System.Runtime.CompilerServices;
using TuringTrader.Simulator;
#endregion

namespace TuringTrader.Indicators
{
    /// <summary>
    /// Collection of performance indicators
    /// </summary>
    public static class IndicatorsPerformance
    {
        #region public static ITimeSeries<double> SharpeRatio(this ITimeSeries<double> series, ITimeSeries<double> riskFreeRate, int n)
        /// <summary>
        /// Calculate Sharpe Ratio, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Sharpe_ratio"/>
        /// </summary>
        /// <param name="series"></param>
        /// <param name="riskFreeRate"></param>
        /// <param name="n"></param>
        /// <param name="parentId">caller cache id, optional</param>
        /// <param name="memberName">caller's member name, optional</param>
        /// <param name="lineNumber">caller line number, optional</param>
        /// <returns></returns>
        public static ITimeSeries<double> SharpeRatio(this ITimeSeries<double> series, ITimeSeries<double> riskFreeRate, int n,
            CacheId parentId = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var cacheId = new CacheId(parentId, memberName, lineNumber,
                series.GetHashCode(), riskFreeRate.GetHashCode(), n);

            var excessReturn = series
                .Return(cacheId)
                .Subtract(riskFreeRate
                        .Return(cacheId),
                    cacheId);

            return excessReturn
                .EMA(n, cacheId)
                .Divide(excessReturn
                        .FastStandardDeviation(n, cacheId)
                        .Max(IndicatorsBasic.Const(1e-10, cacheId), cacheId),
                    cacheId);
        }
        #endregion

        #region public static ITimeSeries<double> Drawdown(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Return current drawdown in percent, as value between 0 and 1.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">length of observation window</param>
        /// <param name="parentId">caller cache id, optional</param>
        /// <param name="memberName">caller's member name, optional</param>
        /// <param name="lineNumber">caller line number, optional</param>
        /// <returns>drawdown as time series</returns>
        public static ITimeSeries<double> Drawdown(this ITimeSeries<double> series, int n,
            CacheId parentId = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var cacheId = new CacheId(parentId, memberName, lineNumber,
                series.GetHashCode(), n);

            // TODO: rewrite this, using buffered lambda, see MaxDrawdown
            return IndicatorsBasic.Const(1.0, cacheId)
                .Subtract(
                    series
                        .Divide(
                            series
                                .Highest(n, cacheId),
                            cacheId),
                    cacheId);
        }
        #endregion
        #region public static ITimeSeries<double> MaxDrawdown(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Return maximum drawdown.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">length of observation window</param>
        /// <param name="parentId">caller cache id, optional</param>
        /// <param name="memberName">caller's member name, optional</param>
        /// <param name="lineNumber">caller line number, optional</param>
        /// <returns>maximum drawdown as time series</returns>
        public static ITimeSeries<double> MaxDrawdown(this ITimeSeries<double> series, int n,
            CacheId parentId = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var cacheId = new CacheId(parentId, memberName, lineNumber,
                series.GetHashCode(), n);

            return IndicatorsBasic.BufferedLambda(
                (p) =>
                {
                    double highestHigh = 0.0;
                    double maxDrawdown = 0.0;
                    for (int t = n - 1; t >= 0; t--)
                    {
                        highestHigh = Math.Max(highestHigh, series[t]);
                        maxDrawdown = Math.Max(maxDrawdown, 1.0 - series[t] / highestHigh);
                    }
                    return maxDrawdown;
                },
                0.0,
                cacheId);
        }
        #endregion
        #region public static ITimeSeries<double> ReturnOnMaxDrawdown(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Calculate return over maximum drawdown.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">length of observation window</param>
        /// <param name="parentId">caller cache id, optional</param>
        /// <param name="memberName">caller's member name, optional</param>
        /// <param name="lineNumber">caller line number, optional</param>
        /// <returns>RoMaD</returns>
        public static ITimeSeries<double> ReturnOnMaxDrawdown(this ITimeSeries<double> series, int n,
            CacheId parentId = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var cacheId = new CacheId(parentId, memberName, lineNumber,
                series.GetHashCode(), n);

            return IndicatorsBasic.BufferedLambda(
                (p) =>
                {
                    double ret = series[0] / series[n] - 1.0;
                    double dd = series.MaxDrawdown(n)[0];
                    return ret / Math.Max(1e-3, dd);
                },
                0.0,
                cacheId);
        }
        #endregion

        #region public static ITimeSeries<double> Runup(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Return current n-day runup as a value between 0 and 1.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">observation window</param>
        /// <param name="parentId">caller cache id, optional</param>
        /// <param name="memberName">caller's member name, optional</param>
        /// <param name="lineNumber">caller line number, optional</param>
        /// <returns>runup as time series</returns>
        public static ITimeSeries<double> Runup(this ITimeSeries<double> series, int n,
            CacheId parentId = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var cacheId = new CacheId(parentId, memberName, lineNumber,
                series.GetHashCode(), n);

            return IndicatorsBasic.BufferedLambda(
                (p) =>
                {
                    double lowestLow = 1e99;
                    for (int t = n - 1; t >= 0; t--)
                    {
                        lowestLow = Math.Min(lowestLow, series[t]);
                    }
                    var runup = series[0] / lowestLow - 1.0;
                    return runup;
                },
                0.0,
                cacheId);
        }
        #endregion
    }
}

//==============================================================================
// end of file