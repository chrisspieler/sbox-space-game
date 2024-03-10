using Sandbox;
using System;

public sealed class BeamTrace : Component
{
	[Property] public Action<TraceResult> OnHit { get; set; }
	[Property] public Action<TraceResult> OnMiss { get; set; }

	[Property] public GameObject Source { get; set; }
	[Property] public float Distance { get; set; } = 2000f;
	[Property] public TagSet IncludeAny { get; set; }
	[Property] public TagSet ExcludeAny { get; set; }

	protected override void OnUpdate()
	{
		Source ??= GameObject;
		var tr = GetTraceResult();
		( tr.Hit ? OnHit : OnMiss )?.Invoke( tr );
	}

	public SceneTraceResult GetTraceResult()
	{
		Source ??= GameObject;
		return Scene.Trace
			.Ray( Source.GetRay(), Distance )
			.WithAnyTags( IncludeAny )
			.WithoutTags( ExcludeAny )
			.Run();
	}
}
