using Sandbox;

public static class PrefabFileExtensions
{
	public static GameObject GetPrefabScene(this PrefabFile prefabFile )
	{
		return SceneUtility.GetPrefabScene( prefabFile );
	}
}
