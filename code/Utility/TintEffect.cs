using Sandbox.Utility;
using Sandbox;

public class TintEffect
{
	public Color Tint { get; init; }
	public ColorBlendMode BlendMode { get; set; }
	public RealTimeUntil? UntilFadeEnd { get; init; }
	public Easing.Function EasingFunction { get; init; }
}
