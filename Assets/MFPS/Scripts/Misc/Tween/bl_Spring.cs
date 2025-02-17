﻿using System;
using UnityEngine;

[Serializable]
public class bl_Spring
{
    public float damping = 26f;
    public float mass = 1f;
    public float stiffness = 169f;
    public float startValue;
    public float endValue;
    public float initialVelocity;

    private float springTime;
    private float currentValue;

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        springTime = 0f;
        currentValue = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetValues(float start, float end)
    {
        startValue = start;
        endValue = end;
    }

    /// <summary>
    /// Update the end value in the middle of motion.
    /// This reuse the current velocity and interpolate the value smoothly afterwards.
    /// </summary>
    /// <param name="value">End value</param>
    public void UpdateEndValue(float value, float velocity)
    {
        startValue = currentValue;
        endValue = value;
        initialVelocity = velocity;
        springTime = 0f;
    }

    /// <summary>
    /// Advance a step by deltaTime(seconds).
    /// </summary>
    /// <param name="deltaTime">Delta time since previous frame</param>
    /// <returns>Evaluated value</returns>
    public float Evaluate(float deltaTime)
    {
        springTime += deltaTime;

        var c = damping;
        var m = mass;
        var k = stiffness;
        var v0 = -initialVelocity;
        var t = springTime;

        var zeta = c / (2 * Mathf.Sqrt(k * m)); // damping ratio
        var omega0 = Mathf.Sqrt(k / m); // undamped angular frequency of the oscillator (rad/s)
        var x0 = endValue - startValue;

        var omegaZeta = omega0 * zeta;
        var x = 0f;
        var v = 0f;

        if (zeta < 1) // Under damped
        {
            var omega1 = omega0 * Mathf.Sqrt(1.0f - zeta * zeta); // exponential decay
            var e = Mathf.Exp(-omegaZeta * t);
            var c1 = x0;
            var c2 = (v0 + omegaZeta * x0) / omega1;
            var cos = Mathf.Cos(omega1 * t);
            var sin = Mathf.Sin(omega1 * t);
            x = e * (c1 * cos + c2 * sin);
            v = -e * ((x0 * omegaZeta - c2 * omega1) * cos + (x0 * omega1 + c2 * omegaZeta) * sin);
        }
        else if (zeta > 1) // Over damped
        {
            var omega2 = omega0 * Mathf.Sqrt(zeta * zeta - 1.0f); // frequency of damped oscillation
            var z1 = -omegaZeta - omega2;
            var z2 = -omegaZeta + omega2;
            var e1 = Mathf.Exp(z1 * t);
            var e2 = Mathf.Exp(z2 * t);
            var c1 = (v0 - x0 * z2) / (-2 * omega2);
            var c2 = x0 - c1;
            x = c1 * e1 + c2 * e2;
            v = c1 * z1 * e1 + c2 * z2 * e2;
        }
        else // Critically damped
        {
            var e = Mathf.Exp(-omega0 * t);
            x = e * (x0 + (v0 + omega0 * x0) * t);
            v = e * (v0 * (1 - t * omega0) + t * x0 * (omega0 * omega0));
        }

        currentValue = endValue - x;

        return currentValue;
    }
}