// Simple frame timing monitor
// OnGUI debug is optional, and can be enabled via scripting using FrameStatsDisplayEnabled = true

using System.Collections.Generic;

namespace UnityEngine.Diagnostics
{
	public class FrameStatsDisplay : MonoBehaviour
	{
		public bool FrameStatsDisplayEnabled
		{
			get { return _enableStatsDisplay; }
			set { _enableStatsDisplay = value; }
		}

		public TimingEntry[] GetLatestStats()
		{
			return entryDefinitions; 
		}

		private static readonly uint NUM_FRAMES = 10;
		private List<FrameTiming> _times = new List<FrameTiming>();
		private FrameTiming[] timing;
		private double[] frameValues;
		private TimingEntry[] entryDefinitions;
		private bool _enableStatsDisplay = false;
		private double _targetFrameTime;
		private uint _sampledFrames;

		public class TimingEntry
		{
			public string Label;
			public double Value;
			public bool IsFrameTime = false;
		}

		void Init()
		{
			if (timing == null)
			{
				timing = new FrameTiming[1];
			}

			if (entryDefinitions == null)
			{
				entryDefinitions = new TimingEntry[]
				{
					new TimingEntry() { Label = "Target Frame Time: {0:N1}ms" },
					new TimingEntry() { Label = "CPU Frame Time: {0:N1}ms", IsFrameTime = true },
					new TimingEntry() { Label = "GPU Frame Time: {0:N1}ms", IsFrameTime = true },
					new TimingEntry() { Label = "FPS: {0:N0}" }
				};
			}
		}

		Color ColorForFrameTime(double targetFrameTime, double frameTime)
		{
			if (frameTime > targetFrameTime) return Color.red;
			if (frameTime + (targetFrameTime*0.2f) > targetFrameTime) return Color.yellow;
			return Color.green;
		}

		void Update()
		{
			Init();

			FrameTimingManager.CaptureFrameTimings();
			_sampledFrames = FrameTimingManager.GetLatestTimings(1, timing);

			_times.Add(timing[0]);

			while (_times.Count > NUM_FRAMES)
			{
				_times.RemoveAt(0);
			}

			double avgCpuFrameTime = 0f;
			double avgGpuFrameTime = 0f;
			_targetFrameTime = (1f / Application.targetFrameRate) * 1000;

			foreach (var t in _times)
			{
				avgCpuFrameTime += t.cpuFrameTime;
				avgGpuFrameTime += t.gpuFrameTime;
			}

			avgCpuFrameTime /= NUM_FRAMES;
			avgGpuFrameTime /= NUM_FRAMES;

			// Assing to array
			entryDefinitions[0].Value = _targetFrameTime;
			entryDefinitions[1].Value = avgCpuFrameTime;
			entryDefinitions[2].Value = avgGpuFrameTime;
			entryDefinitions[3].Value = 1000f / Mathf.Max(Mathf.Max((float)_targetFrameTime, (float)avgCpuFrameTime), (float)avgGpuFrameTime);
		}

#if DEBUG
		public void OnGUI()
		{
			if (entryDefinitions == null) return;
			
			if (_enableStatsDisplay)
			{
				float hudHeight = Screen.height * 0.05f;
				float padding = hudHeight * 0.1f;
				float widthPerElement = Screen.width / (float) entryDefinitions.Length;

				int origFontSize = GUI.skin.label.fontSize;
				GUI.skin.label.fontSize = (int) (hudHeight * 0.6f);
				GUILayout.BeginHorizontal();
				GUI.Box(new Rect(0, 0, Screen.width, hudHeight + (padding * 2f)), GUIContent.none);

				var oldAlignment = GUI.skin.label.alignment;
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				if (_sampledFrames == 0)
				{
					GUI.Label(new Rect(0, 0, Screen.width, hudHeight + (padding * 2f)),
						new GUIContent("Sample API not available on this platform"));

				}
				else
				{
					for (int i = 0; i < entryDefinitions.Length; i++)
					{
						var f = entryDefinitions[i];
						GUI.color = (f.IsFrameTime ? ColorForFrameTime(_targetFrameTime, f.Value) : Color.white);
						GUI.Label(new Rect(padding + (widthPerElement * i), padding, widthPerElement, hudHeight),
							string.Format(f.Label, f.Value));
					}
				}

				GUI.skin.label.alignment = oldAlignment;
				GUI.color = Color.white;

				GUILayout.EndHorizontal();
				GUI.skin.label.fontSize = origFontSize;
			}
		}
#endif
	}
}
