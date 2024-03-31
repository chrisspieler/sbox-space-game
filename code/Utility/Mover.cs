using Sandbox;

public sealed class Mover : Component
{
	[Property] public Vector3 Direction { get; set; }
	[Property] public float UnitsPerSecond { get; set; }
	[Property] public float? LerpSpeed { get; set; } = 5f;

	private Vector3 _currentTranslation;

	protected override void OnUpdate()
	{
		if ( LerpSpeed.HasValue )
		{
			_currentTranslation = _currentTranslation.LerpTo( UnitsPerSecond, Time.Delta * LerpSpeed.Value );
		}
		else
		{
			_currentTranslation = UnitsPerSecond;
		}
		Transform.Position += Direction.Normal * _currentTranslation * Time.Delta;
	}
}
