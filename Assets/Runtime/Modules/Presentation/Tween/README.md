# Tween

Small standalone tween runner for values, transforms, uGUI, sequences, loops, and common feedback effects.

```csharp
TweenCore movement = transform
    .LocalMoveTo(targetPosition, 0.45f)
    .SetEase(EaseType.OutCubic)
    .SetTarget(this);
```

Build a sequence with paused children by passing `autoPlay: false` to `Tween.To`:

```csharp
var move = Tween.To(this, GetX, SetX, 100f, 0.5f, autoPlay: false);
var fade = Tween.To(this, GetAlpha, SetAlpha, 1f, 0.2f, autoPlay: false);

Tween.Sequence()
    .Append(move)
    .Join(fade)
    .Play();
```

Keep the returned `TweenCore` when code must pause, resume, rewind, complete, kill, or await it. Assign an owner with `SetTarget(owner)` and call `Tween.Kill(owner)` when that owner ends. Use `SetUpdate(..., unscaledTime: true)` for pause-menu and hit-feedback animation.

Open `../../../Examples/Presentation/Tween/TweenExample.unity` for sequences, easing comparison, yoyo loops, lifecycle controls, unscaled feedback, and Unity `Awaitable` integration.
