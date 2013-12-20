﻿// <copyright file="CubicSpline.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Cubic Spline Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class CubicSpline : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _c0;
        readonly double[] _c1;
        readonly double[] _c2;
        readonly double[] _c3;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">sample points (N+1), sorted ascending</param>
        /// <param name="c0">Zero order spline coefficients (N)</param>
        /// <param name="c1">First order spline coefficients (N)</param>
        /// <param name="c2">second order spline coefficients (N)</param>
        /// <param name="c3">third order spline coefficients (N)</param>
        public CubicSpline(double[] x, double[] c0, double[] c1, double[] c2, double[] c3)
        {
            _x = x;
            _c0 = c0;
            _c1 = c1;
            _c2 = c2;
            _c3 = c3;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        public bool SupportsDifferentiation
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        public bool SupportsIntegration
        {
            get { return true; }
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return _c0[k] + x*(_c1[k] + x*(_c2[k] + x*_c3[k]));
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return _c1[k] + x*(2*_c2[k] + x*3*_c3[k]);
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return 2*_c2[k] + x*6*_c3[k];
        }

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        public double Integrate(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return _indefiniteIntegral.Value[k] + x*(_c0[k] + x*(_c1[k]/2 + x*(_c2[k]/3 + x*_c3[k]/4)));
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        public double Integrate(double a, double b)
        {
            return Integrate(b) - Integrate(a);
        }

        double[] ComputeIndefiniteIntegral()
        {
            var integral = new double[_c1.Length];
            for (int i = 0; i < integral.Length - 1; i++)
            {
                double w = _x[i + 1] - _x[i];
                integral[i + 1] = integral[i] + w*(_c0[i] + w*(_c1[i]/2 + w*(_c2[i]/3 + w*_c3[i]/4)));
            }
            return integral;
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        int LeftBracketIndex(double t)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _x.Length - 1;
            while (low != high - 1)
            {
                int middle = (low + high)/2;
                if (_x[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            return low;
        }
    }
}
