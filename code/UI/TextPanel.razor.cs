using Sandbox;

public partial class TextPanel : PanelComponent
{
	[Property] public Target Target { get; set; }
	[Property] public string Text { get; set; } = "Hello World!";
	[Property] public float Lifetime { get; set; } = 1.5f;
	[Property] public bool IsAlert { get; set; }

	protected override int BuildHash() => System.HashCode.Combine( Text, Target, IsAlert );

	private string TextClass => IsAlert ? "alert" : "";
	private float Opacity { get; set; } = 1f;
	private TimeUntil _untilDestroy;

	protected override void OnStart()
	{
		base.OnStart();
		StateHasChanged();

		_untilDestroy = Lifetime;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		HandleFadeOut();
	}

	private void HandleFadeOut()
	{
		if ( _untilDestroy.Fraction < 0.5f )
			return;

		if ( _untilDestroy.Fraction < 1f )
		{
			Opacity = 1f - (_untilDestroy.Fraction - 0.5f) * 2;
			StateHasChanged();
			return;
		}
		Panel.Delete();
		GameObject.Destroy();
	}
}
