using UnityEngine;

public class KalmanFilter
{
    // SOURCE: https://web.mit.edu/kirtley/kirtley/binlustuff/literature/control/Kalman%20filter.pdf
    // Constants
    public const float DEFAULT_Q = 0.000001f;
    public const float DEFAULT_R = 0.01f;
    public const float DEFAULT_P = 1;

    public Vector3 State { get; private set; } // Estimated state
    private Vector3 _lastState; // Previous state
    private float _q; // Process noise covariance
    private float _r; // Measurement noise covariance
    private float _p; // Error covariance
    private float _k; // Kalman gain

    public KalmanFilter(Vector3 initialState, float processNoise = DEFAULT_Q, float measurementNoise = DEFAULT_R)
    {
        _q = processNoise;
        _r = measurementNoise;
        _p = DEFAULT_P;
        State = _lastState = initialState;
    }

    public Vector3 Update(Vector3 measurement)
    {
        // Kalman gain
        _k = (_p + _q) / (_p + _q + _r);

        // Update estimate
        State = _lastState + _k * (measurement - _lastState);

        // Update covariance
        _p = _r * (_p + _q) / (_r + _p + _q);

        // Save state for the next iteration
        _lastState = State;

        return State;
    }

    public void Reset()
    {
        Reset(Vector3.zero);
    }

    public void Reset(Vector3 newState)
    {
        _p = DEFAULT_P;
		_k = 0;
		State = _lastState = newState;
    }
}
