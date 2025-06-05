using UnityEngine;

namespace fwp.localizator.dialog
{
	/// <summary>
	/// a line within a dialog
	/// </summary>
	public interface iDialogLine
	{
		/// <summary>
		/// identifier of line
		/// </summary>
		public string getLineUid();

		/// <summary>
		/// extract localization for this line
		/// </summary>
		public string getContent();
	}
}