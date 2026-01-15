using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace fwp.localizator.subtitles
{

	/// <summary>
	/// for structure see sample file
	/// search for file in Resources/localization/subtitles/
	/// 
	/// > load file
	/// > assign text field (tmpro)
	/// > update(timecode)
	/// 
	/// ONLY WORKS WITH .txt files (using <TextAsset>)
	/// 
	/// </summary>
	public class LocalizationSubtitleFile
	{

		/// <summary>
		/// Text Assets are a format for imported text files. When you drop a text file into your Project Folder, 
		/// it will be converted to a Text Asset. The supported text formats are:
		/// </summary>
		// readonly string[] supportedExtensions = new[] { "txt","html", "htm","xml","bytes","json","csv","yaml","fnt", };

		const string locaSubPath = "localization/subtitles/";
		const string regLineEnding = "\r\n|\r|\n";
		TextAsset ta;

		List<LocalizationSubtitleLine> lines = new List<LocalizationSubtitleLine>();

		string _path;
		TMPro.TextMeshProUGUI txt;

		/// <summary>
		/// was setup and HAS LINES
		/// </summary>
		public bool IsValid
		{
			get
			{
				if (ta == null) return false;
				// if (string.IsNullOrEmpty(_path)) return false;
				if (lines == null) return false;
				return lines.Count > 0;
			}
		}

		public LocalizationSubtitleFile(string videoFileName)
		{
			setupForVideo(videoFileName);
		}

		/// <summary>
		/// solve all data based on video file name
		/// seek subtitle file within resources path
		/// </summary>
		public void setupForVideo(string videoFileName)
		{
			_path = Path.Combine(locaSubPath, videoFileName);
			ta = Resources.Load<TextAsset>(_path);

			//https://docs.unity3d.com/Manual/class-TextAsset.html
			if (ta == null)
			{
				Debug.LogWarning("<color=red>failed to load</color> TextAsset @ " + _path);
				Debug.LogWarning("> file does not exist ? or must be .txt");

				_path = null; // invalid

				return;
			}

			// NOTE: DO NOT REMOVE EMPTY LINES (separator)

			//https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
			string[] rawLines = Regex.Split(ta.text, regLineEnding);

			if (rawLines.Length <= 0) Debug.LogWarning("no lines from raw text");
			else
			{
				Debug.Log("solving sub(s) out of x" + rawLines.Length + " lines");

				lines = new List<LocalizationSubtitleLine>();

				List<string> tmp = new List<string>();

				//tmp will gather data until an empty line is reached
				for (int i = 0; i < rawLines.Length; i++)
				{
					string line = rawLines[i];

					//Debug.Log("  --> (" + line.Length + ") " + line);

					if (line.Length <= 0) // search for empty line (might have line endings !) ...
					{
						//has gathered content ? creating sub with it
						if (tmp.Count > 2)
						{
							lines.Add(new LocalizationSubtitleLine(tmp));
							//Debug.Log("added new sub ! new total : x" + lines.Count);
						}
						tmp.Clear();
					}
					else tmp.Add(line);
				}

			}

#if UNITY_EDITOR
			log("<b>loaded subtitles</b> for video : " + videoFileName + " | at path : " + _path + " | lines count : " + lines.Count);
			if (lines.Count > 0) log("<b>loaded subtitles</b>  L first line : " + lines[0].buffLine);
			else log("no subtitles will be displayed ...");
#endif

			//for (int i = 0; i < lines.Count; i++) Debug.Log("  " + lines[i].line);
		}

		public void assignTextfield(TMPro.TextMeshProUGUI txt)
		{
			this.txt = txt;

			txt.text = string.Empty;
			txt.enabled = true;

			//Debug.Log("setupAndStart");
		}

		public void update(double timecode)
		{
			if (lines.Count <= 0)
			{
				Debug.LogWarning("sub file has x0 lines to match timecode " + timecode);
				return;
			}

			txt.text = string.Empty;

			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].isWithingLineTimecodeRange(timecode))
				{
					txt.text = lines[i].buffLine;

					//Debug.Log("sub | time?"+timecode + " , text:" + txt.text);
				}
			}

		}

		public string stringify()
		{
			string ret = "[srt]" + _path + " x" + lines.Count;

			if (!IsValid) ret += " INVALID";
			else
			{
				foreach (var l in lines)
				{
					ret += "\n> " + l;
				}
			}
			return ret;
		}

		static public void log(string msg, object tar = null)
		{
			Debug.Log("[subtitles] " + msg, tar as Object);
		}
	}

	/// <summary>
	/// 
	/// #0 >	1
	/// #1 >	00:01:26,880 --> 00:01:32,160
	/// #2 >	a subtitle sentence.
	/// 
	/// </summary>
	public class LocalizationSubtitleLine
	{
		//int id = -1;
		float timecode_start = 0f;
		float timecode_end = 0f;

		public string rawLine = string.Empty; // actual value stored in file
		public string buffLine = string.Empty; // translated content from rawLine id

		public LocalizationSubtitleLine(List<string> rawLines)
		{
			//id = int.Parse(rawLines[0]);

			//Debug.Log("  generated subtitle line #" + id);

			string timecode = rawLines[1];
			string[] tmp;
			string[] fields;

			//for (int i = 0; i < rawLines.Count; i++) Debug.Log(rawLines[i]);

			//start
			tmp = timecode.Split(' '); // HH:MM:SS,MMM
			fields = tmp[0].Split(':');
			timecode_start = int.Parse(fields[0]) * 60f * 60f; // HH
			timecode_start += int.Parse(fields[1]) * 60f; // MM
			fields = fields[2].Split(',');
			timecode_start += int.Parse(fields[0]); // SS
			timecode_start += int.Parse(fields[1]) / 1000f; // MMM -> SS

			//Debug.Log(timecode + " --> " + timecode_start);

			//end
			fields = tmp[tmp.Length - 1].Split(':');
			timecode_end = int.Parse(fields[0]) * 60f * 60f; // HH
			timecode_end += int.Parse(fields[1]) * 60f; // MM
			fields = fields[2].Split(',');
			timecode_end += int.Parse(fields[0]); // SS
			timecode_end += int.Parse(fields[1]) / 1000f; // MMM -> SS

			//Debug.Log(timecode + " --> " + timecode_end);

			rawLine = rawLines[2]; // actual subtitle line (after timings)
			buffLine = localizeLine(rawLine); // localized value

			LocalizationSubtitleFile.log("generated subtitle line #" + rawLine + " [" + timecode_start + " , " + timecode_end + "]   : " + buffLine);
		}

		virtual protected string localizeLine(string line)
		{
			if (LocalizationManager.instance != null)
				return LocalizationManager.instance.getContent(rawLine);

			return line;
		}

		public bool isWithingLineTimecodeRange(double timecode)
		{
			//Debug.Log(timecode + " ? " + timecode_start + " , " + timecode_end);
			return (timecode_start < timecode && timecode_end > timecode);
		}

	}
}