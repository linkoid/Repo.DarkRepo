using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Linkoid.Repo.DarkRepo;

internal readonly struct AdjustmentCurve
{
    const float x_0 = -1f, x_1 = 0f, x_2 = 1f;
    const float y_0 = 0f, y_2 = 1f;

    readonly float y_1;
    readonly float dydx_0, dydx_1, dydx_2;
    readonly float ddydx_1_0, ddydx_1_1, ddydx_2_1, ddydx_2_2;
    readonly float a_1, b_1, c_1, d_1;
    readonly float a_2, b_2, c_2, d_2;

    public AdjustmentCurve(float yIntercept)
    {
        y_1 = yIntercept;

        // float dydx_0, dydx_1, dydx_2;
        {
            dydx_1 = 2 / ((x_2 - x_1) / (y_2 - y_1) + (x_1 - x_0) / (y_1 - y_0));
            if (dydx_1 < 0f)
            {
                dydx_1 = 0f;
                dydx_2 = 0f;
            }
            else
            {
                dydx_2 = 3*(y_2-y_1) / 2*(x_2-x_1) - dydx_1/2;
            }
            dydx_0 = 3*(y_1-y_0) / 2*(x_1-x_0) - dydx_1/2;
        }

        // float ddydx_1_0, ddydx_1_1, ddydx_2_1, ddydx_2_2;
        {
            ddydx_1_0 = -2*(  dydx_1+2*dydx_0) / (x_1-x_0) + 6*(y_1-y_0) / ((x_1-x_0)*(x_1-x_0));
            ddydx_1_1 =  2*(2*dydx_1+  dydx_0) / (x_1-x_0) - 6*(y_1-y_0) / ((x_1-x_0)*(x_1-x_0));
            ddydx_2_1 = -2*(  dydx_2+2*dydx_1) / (x_2-x_1) + 6*(y_2-y_1) / ((x_2-x_1)*(x_2-x_1));
            ddydx_2_2 =  2*(2*dydx_2+  dydx_1) / (x_2-x_1) - 6*(y_2-y_1) / ((x_2-x_1)*(x_2-x_1));
        }

        const float x_0_p2 = x_0 * x_0;
        const float x_0_p3 = x_0 * x_0 * x_0;

        const float x_1_p2 = x_1 * x_1;
        const float x_1_p3 = x_1 * x_1 * x_1;

        const float x_2_p2 = x_2 * x_2;
        const float x_2_p3 = x_2 * x_2 * x_2;

        // float a_1, b_1, c_1, d_1;
        {
            d_1 = (ddydx_1_1-ddydx_1_0) / (6*(x_1-x_0));
            c_1 = (x_1*ddydx_1_0-x_0*ddydx_1_1) / (2*(x_1-x_0));
            b_1 = ((y_1-y_0) - c_1*(x_1_p2 - x_0_p2) -d_1*(x_1_p3 - x_0_p3)) / (x_1-x_0);
            a_1 = y_0 - b_1*x_0 - c_1*x_0_p2 - d_1*x_0_p3;
        }

        // float a_2, b_2, c_2, d_2;
        {
            d_2 = (ddydx_2_2-ddydx_2_1) / (6*(x_2-x_1));
            c_2 = (x_2*ddydx_2_1-x_1*ddydx_2_2) / (2*(x_2-x_1));
            b_2 = ((y_2-y_1) - c_2*(x_2_p2 - x_1_p2) - d_2*(x_2_p3 - x_1_p3)) / (x_2-x_1);
            a_2 = y_1 - b_2*x_1 - c_2*x_1_p2 - d_2*x_1_p3;
        }
    }

    public float Evaluate(float x)
    {
        if (x <= 0f)
        {
            return a_1 + b_1*x + c_1*x*x + d_1*x*x*x;
        }
        else
        {
            return a_2 + b_2*x + c_2*x*x + d_2*x*x*x;
        }
    }
}
