using UnityEngine;

namespace StarryFramework
{
	public class FoldOutGroupAttribute : PropertyAttribute
	{
		public string name;
		public bool foldEverything;

		/// <summary>Adds the property to the specified foldout group.</summary>
		/// <param Name="name">Name of the foldout group.</param>
		/// <param Name="foldEverything">Foldout to put all properties to the specified group</param>
		public FoldOutGroupAttribute(string name, bool foldEverything = false)
		{
			this.foldEverything = foldEverything;
			this.name = name;
		}
	}
}