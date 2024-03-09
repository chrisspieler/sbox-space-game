using Sandbox;
using System;

public sealed class BeamTrace : Component
{
	[Property] public Action<SceneTraceResult> OnHit { get; set; }
	[Property] public Action<SceneTraceResult> OnMiss { get; set; }

	[Property] public GameObject Source { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public TagSet IncludeAny { get; set; }
	[Property] public TagSet ExcludeAny { get; set; }

	protected override void OnUpdate()
	{
		if ( Target is null )
			return;

		Source ??= GameObject;
		var tr = GetTraceResult();
		( tr.Hit ? OnHit : OnMiss )?.Invoke( tr );
	}

	private SceneTraceResult GetTraceResult()
	{
		return Scene.Trace
			.Ray( Source.Transform.Position, Target.Transform.Position )
			.WithAnyTags( IncludeAny )
			.WithoutTags( ExcludeAny )
			.Run();
	}
}
