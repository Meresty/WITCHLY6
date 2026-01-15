
using System;

public class Timer
{
    bool active = true;
    float duration;
    public float TimeLeft {get; private set; }
    public bool hasFinished => TimeLeft <= 0;
    public Action OnTimerFinished;

    public Timer(float duration = 0) {
        this.duration = duration;
        TimeLeft = this.duration;
    }

    public void Start(float? duration = null) {
        this.duration = duration ?? this.duration;
        TimeLeft = this.duration;
        active = true;
    }

    public void Reset(float? duration) {
        this.duration = duration ?? this.duration;
        TimeLeft = this.duration;
        active = false;
    }

    public void Tick(float deltaTime) {
        if (!active || hasFinished) return;
        TimeLeft -= deltaTime;
        if (TimeLeft < 0) {
            TimeLeft = 0;
            active = false;
            OnTimerFinished?.Invoke();
        }
    }
}