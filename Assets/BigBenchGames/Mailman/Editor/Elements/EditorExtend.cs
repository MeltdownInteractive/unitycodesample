#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BigBenchGames.Editor.Elements
{
	/// <summary>
	/// Extends editor functionaility
	/// Most of this code was take from: http://www.clonefactor.com/wordpress/public/1769/
	/// </summary>
	public sealed class EditorExtend
	{
		#region Text AutoComplete
		private const string m_AutoCompleteField = "AutoCompleteField";
		private static List<string> m_CacheCheckList = null;
		private static string m_AutoCompleteLastInput;
		private static string m_EditorFocusAutoComplete;

		/// <summary>
		/// A textField to popup a matching popup, based on developers input values.
		/// </summary>
		/// <param name="input">string input.</param>
		/// <param name="source">the data of all possible values (string).</param>
		/// <param name="maxShownCount">the amount to display result.</param>
		/// <param name="levenshteinDistance">
		/// value between 0f ~ 1f,
		/// - more then 0f will enable the fuzzy matching
		/// - 1f = anything thing is okay.
		/// - 0f = require full match to the reference
		/// - recommend 0.4f ~ 0.7f
		/// </param>
		/// <returns>output string.</returns>
		public static string TextFieldAutoComplete(Rect position, string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
		{
			string tag = m_AutoCompleteField + GUIUtility.GetControlID(FocusType.Passive);
			int uiDepth = GUI.depth;
			GUI.SetNextControlName(tag);
			string rst = WithoutSelectAll(() => EditorGUI.TextField(position, input));

			if (input != null && input.Length > 0 && !string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()) && GUI.GetNameOfFocusedControl() == tag)
			{
				if (m_AutoCompleteLastInput != input || // input changed
					m_EditorFocusAutoComplete != tag) // another field.
				{
					// Update cache
					m_EditorFocusAutoComplete = tag;
					m_AutoCompleteLastInput = input;

					List<string> uniqueSrc = new List<string>(new HashSet<string>(source)); // remove duplicate
					int srcCnt = uniqueSrc.Count;
					m_CacheCheckList = new List<string>(System.Math.Min(maxShownCount, srcCnt)); // optimize memory alloc

					//TODO (28/03/2024): Consider multithreading to improve speed.
                    // Start with - slow
                    for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                    {
                    	if (uniqueSrc[i].ToLower().StartsWith(input.ToLower()))
                    	{
                    		m_CacheCheckList.Add(uniqueSrc[i]);
                    		uniqueSrc.RemoveAt(i);
                    		srcCnt--;
                    		i--;
                    	}
                    }

                    // Contains - very slow
                    if (m_CacheCheckList.Count == 0)
                    {
                        for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                        {
                            if (uniqueSrc[i].ToLower().Contains(input.ToLower()))
                            {
                                m_CacheCheckList.Add(uniqueSrc[i]);
                                uniqueSrc.RemoveAt(i);
                                srcCnt--;
                                i--;
                            }
                        }
                    }

                    // Levenshtein Distance - very very slow.
                    //if (levenshteinDistance > 0f && // only developer request
                    //    input.Length > 3 && // 3 characters on input, hidden value to avoid doing too early.
                    //    m_CacheCheckList.Count < maxShownCount) // have some empty space for matching.
                    //{
                    //    levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
                    //    string keywords = input.ToLower();
                    //    for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                    //    {
                    //        int distance = LevenshteinDistance(uniqueSrc[i], keywords, caseSensitive: false);
                    //        bool closeEnough = (int)(levenshteinDistance * uniqueSrc[i].Length) > distance;
                    //        if (closeEnough)
                    //        {
                    //            m_CacheCheckList.Add(uniqueSrc[i]);
                    //            uniqueSrc.RemoveAt(i);
                    //            srcCnt--;
                    //            i--;
                    //        }
                    //    }
                    //}

					//Cosine Similarity
					if(m_CacheCheckList.Count < maxShownCount)
                    {
						string keywords = input.ToLower();
						for(int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
                        {
							double distance = GetSimilarity(uniqueSrc[i], keywords);
							bool closeEnough = distance > levenshteinDistance;
                            if (closeEnough)
                            {
								m_CacheCheckList.Add(uniqueSrc[i]);
								uniqueSrc.RemoveAt(i);
								srcCnt--;
								i--;
                            }
                        }
                    }
                }

				// Draw recommend keyward(s)
				if (m_CacheCheckList.Count > 0)
				{
					int cnt = m_CacheCheckList.Count;
					float height = cnt * EditorGUIUtility.singleLineHeight;
					Rect area = position;
					area = new Rect(area.x, area.y + EditorGUIUtility.singleLineHeight, area.width, height);
					GUI.depth -= 10;
					// GUI.BeginGroup(area);
					// area.position = Vector2.zero;
					GUI.BeginClip(area);
					Rect line = new Rect(0, 0, area.width, EditorGUIUtility.singleLineHeight);

					for (int i = 0; i < cnt; i++)
					{
						if (GUI.Button(line, m_CacheCheckList[i]))//, EditorStyles.toolbarDropDown))
						{
							rst = m_CacheCheckList[i];
							GUI.changed = true;
							GUI.FocusControl(""); // force update
						}
						line.y += line.height;
					}
					GUI.EndClip();
					//GUI.EndGroup();
					GUI.depth += 10;
				}
			}
			return rst;
		}

		/// <summary>
		/// A textField to popup a matching popup, based on developers input values. Use with EditorGUILayout
		/// </summary>
		/// <param name="input">string input.</param>
		/// <param name="source">the data of all possible values (string).</param>
		/// <param name="maxShownCount">the amount to display result.</param>
		/// <param name="levenshteinDistance">
		/// value between 0f ~ 1f,
		/// - more then 0f will enable the fuzzy matching
		/// - 1f = anything thing is okay.
		/// - 0f = require full match to the reference
		/// - recommend 0.4f ~ 0.7f
		/// </param>
		/// <returns>output string.</returns>
		public static string TextFieldAutoComplete(string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
		{
			Rect rect = EditorGUILayout.GetControlRect();
			return TextFieldAutoComplete(rect, input, source, maxShownCount, levenshteinDistance);
		}
		#endregion

		/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
		/// <typeparam name="T">The type of the items in the enumerables.</typeparam>
		/// <param name="lhs">The first enumerable.</param>
		/// <param name="rhs">The second enumerable.</param>
		/// <returns>The edit distance.</returns>
		/// <see cref="https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/"/>
		public static int LevenshteinDistance<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) where T : System.IEquatable<T>
		{
			// Validate parameters
			if (lhs == null) throw new System.ArgumentNullException("lhs");
			if (rhs == null) throw new System.ArgumentNullException("rhs");

			// Convert the parameters into IList instances
			// in order to obtain indexing capabilities
			IList<T> first = lhs as IList<T> ?? new List<T>(lhs);
			IList<T> second = rhs as IList<T> ?? new List<T>(rhs);

			// Get the length of both.  If either is 0, return
			// the length of the other, since that number of insertions
			// would be required.
			int n = first.Count, m = second.Count;
			if (n == 0) return m;
			if (m == 0) return n;

			// Rather than maintain an entire matrix (which would require O(n*m) space),
			// just store the current row and the next row, each of which has a length m+1,
			// so just O(m) space. Initialize the current row.
			int curRow = 0, nextRow = 1;

			int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
			for (int j = 0; j <= m; ++j)
				rows[curRow][j] = j;

			// For each virtual row (since we only have physical storage for two)
			for (int i = 1; i <= n; ++i)
			{
				// Fill in the values in the row
				rows[nextRow][0] = i;

				for (int j = 1; j <= m; ++j)
				{
					int dist1 = rows[curRow][j] + 1;
					int dist2 = rows[nextRow][j - 1] + 1;
					int dist3 = rows[curRow][j - 1] +
						(first[i - 1].Equals(second[j - 1]) ? 0 : 1);

					rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
				}

				// Swap the current and next rows
				if (curRow == 0)
				{
					curRow = 1;
					nextRow = 0;
				}
				else
				{
					curRow = 0;
					nextRow = 1;
				}
			}

			// Return the computed edit distance
			return rows[curRow][m];
		}

		/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
		/// <param name="lhs">The first enumerable.</param>
		/// <param name="rhs">The second enumerable.</param>
		/// <returns>The edit distance.</returns>
		/// <see cref="https://en.wikipedia.org/wiki/Levenshtein_distance"/>
		public static int LevenshteinDistance(string lhs, string rhs, bool caseSensitive = true)
		{
			if (!caseSensitive)
			{
				lhs = lhs.ToLower();
				rhs = rhs.ToLower();
			}
			char[] first = lhs.ToCharArray();
			char[] second = rhs.ToCharArray();
			return LevenshteinDistance<char>(first, second);
		}

		#region CosineSimilarity
		private const int VectorSize = 26;

		/// <summary>
		/// Uses Cosine similarity to get the similiarity between 2 strings
		/// </summary>
		/// <param name="s1">The first string to compare</param>
		/// <param name="s2">The second string to compare</param>
		/// <returns>The cosine similarity between the two strings</returns>
		public static double GetSimilarity(string s1, string s2)
		{
			var v1 = GetVector(s1);
			var v2 = GetVector(s2);

			return GetDotProduct(v1, v2) / (GetMagnitude(v1) * GetMagnitude(v2));
		}

		private static int[] GetWordCounts(string s)
		{
			var counts = new int[VectorSize];

			foreach (var c in s)
			{
				if (char.IsLetter(c))
				{
					counts[char.ToLower(c) - 'a']++;
				}
			}

			return counts;
		}

		private static double[] GetVector(string s)
		{
			var vector = new double[VectorSize];
			var counts = GetWordCounts(s);

			for (var i = 0; i < VectorSize; i++)
			{
				vector[i] = counts[i];
			}

			return vector;
		}

		private static double GetDotProduct(double[] v1, double[] v2)
		{
			var dotProduct = 0.0;

			for (var i = 0; i < VectorSize; i++)
			{
				dotProduct += v1[i] * v2[i];
			}

			return dotProduct;
		}

		private static double GetMagnitude(double[] vector)
		{
			var sum = vector.Sum(v => v * v);
			return Math.Sqrt(sum);
		}
		#endregion

		/// <summary>
		/// Create a input field without automatically selecting it when clicking on it (thanks Unity)
		/// </summary>
		/// <typeparam name="T">The return type of the field</typeparam>
		/// <param name="guiCall">The call to the GUI field display function</param>
		/// <returns>The return value of the GUI field type</returns>
		public static T WithoutSelectAll<T>(Func<T> guiCall)
		{
			bool preventSelection = (Event.current.type == EventType.MouseDown);
			Color oldCursorColor = GUI.skin.settings.cursorColor;

			if (preventSelection)
				GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

			T value = guiCall();

			if (preventSelection)
				GUI.skin.settings.cursorColor = oldCursorColor;

			return value;
		}
	}
}
#endif