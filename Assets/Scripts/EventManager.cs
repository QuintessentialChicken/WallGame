using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public delegate void FloatEvent(float value);
    public delegate void IntEvent(int value);
    public delegate void TransformEvent(Transform value);
    public delegate void PathEvent(Vector3[] path, int vertexCount);

    public static event TransformEvent OnEnterCatapult;
    public static event TransformEvent OnExitCatapult;
    public static event PathEvent OnCatapultFire;
    public static event FloatEvent OnPlayerStunned;

    public static event IntEvent OnReplenishResource;
    
    public static void RaiseCatapultFire(Vector3[] path, int vertexCount)
    {
        OnCatapultFire?.Invoke(path, vertexCount);
    }

    public static void RaiseEnterCatapult(Transform catapultBowl)
    {
        OnEnterCatapult?.Invoke(catapultBowl);
    }
    
    public static void RaiseExitCatapult(Transform exitPoint)
    {
        OnExitCatapult?.Invoke(exitPoint);
    }

    public static void RaisePlayerStunned(float duration)
    {
        OnPlayerStunned?.Invoke(duration);
    }

    public static void RaiseOnReplenishResource(int amount)
    {
        OnReplenishResource?.Invoke(amount);
    }
}
