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
		bool verbose = false;

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

		/// <summary>
		/// activate some logs
		/// </summary>
		public bool FlagVerbose() => verbose = true;

		public LocalizationSubtitleFile(string videoFileName, bool setVerbose = false)
		{
			if (setVerbose) FlagVerbose();

			if (!string.IsNullOrEmpty(videoFileName))
			{
				setupForVideo(videoFileName);
			}
		}

		/// <summary>
		/// solve all data based on video file name
		/// seek subtitle file within resources path
		/// </summary>
		public LocalizationSubtitleFile setupForVideo(string videoFileName)
		{
			_path = Path.Combine(locaSubPath, videoFileName);
			ta = Resources.Load<TextAsset>(_path);

			//https://docs.unity3d.com/Manual/class-TextAsset.html
			if (ta == null)
			{
				if (verbose)
				{
					Debug.LogWarning("<color=red>failed to load</color> TextAsset @ " + _path);
					Debug.LogWarning("> file does not exist ? or must be .txt");
				}

				_path = null; // invalid

				return null;
			}

			// NOTE: DO NOT REMOVE EMPTY LINES (separator)

			//https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
			string[] rawLines = Regex.Split(ta.text, regLineEnding);

			if (rawLines.Length <= 0) Debug.LogWarning("no lines from raw text");
			else
			{
				if (verbose) log("solving sub(s) out of x" + rawLines.Length + " lines");

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

			return this;
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
			// can't update invalid subtitle
			if (!IsValid)
			{
				if (verbose) log("updating invalid");
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
			string ret = "[srt] ";

			if (!IsValid)
			{
				ret += "	INVALID";
				return ret;
			}

			ret += _path + " x" + lines.Count;
			foreach (var l in lines)
			{
				ret += "\n> " + l;
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
		}

		public string stringify()
		{
			return "line #" + rawLine + " [" + timecode_start + " , " + timecode_end + "] : " + buffLine;
		}

		/// <summary>
		/// specific way to localize line loca UID
		/// </summary>
		virtual protected string localizeLine(string line)
		{
			if (LocalizationManager.instance != null)
				return LocalizationManager.instance.getContent(rawLine);

			return line;
		}

		public bool isWithingLineTimecodeRange(double timecode, bool verbose = false)
		{
			if (timecode < timecode_start)
			{
				if (verbose) Debug.Log($"timecode {timecode} < start {timecode_start}");
				return false;
			}

			if (timecode > timecode_end)
			{
				if (verbose) Debug.Log($"timecode {timecode} > end {timecode_end}");
				return false;
			}

			return true;
		}

	}
}