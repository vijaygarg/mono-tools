//
// Comparison.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace GuiCompare {

	public enum ComparisonStatus {
		None,
		Missing,
		Extra,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (CompType type, string displayName)
		: this (type, displayName, null)
		{
		}
		
		public ComparisonNode (CompType type, string displayName, string typeName)
		{
			Type = type;
			Name = displayName;
			TypeName = typeName;
			Children = new List<ComparisonNode>();
			Messages = new List<string>();
			Todos = new List<string>();
		}

		public void AddChild (ComparisonNode node)
		{
			Children.Add (node);
			node.Parent = this;
		}

		public void PropagateCounts ()
		{
			Todo = Todos.Count;
			Niex = ThrowsNIE ? 1 : 0;
			foreach (ComparisonNode n in Children) {
				n.PropagateCounts ();
				Extra += n.Extra + (n.Status == ComparisonStatus.Extra ? 1 : 0);
				Missing += n.Missing + (n.Status == ComparisonStatus.Missing ? 1 : 0);
				Present += n.Present; // XXX
				Todo += n.Todo;
				Niex += n.Niex;
				Warning += n.Warning + (n.Status == ComparisonStatus.Error ? 1 : 0);
			}
		}

		public void AddError (string msg)
		{
			Status = ComparisonStatus.Error;
			Messages.Add (msg);
		}

		// TODO: detect user's locale and reflect that in the url
		public string MSDNUrl {
			get {
				if (msdnUrl != null)
					return msdnUrl;
				
				if (String.IsNullOrEmpty (TypeName)) {
					msdnUrl = MSDN_BASE_URL + ConstructMSDNUrl ();
					return msdnUrl;
				}
				
				if (msdnUrl == null)
					msdnUrl = MSDN_BASE_URL + TypeName.ToLower () + ".aspx";

				return msdnUrl;
			}
		}

		string FormatMyName ()
		{
			string name = Name;
			int start = name.IndexOf (' ') + 1;
			int end = name.IndexOf ('(');
			int len = end - start;

			if (len <= 0)
				return name;
			
			return name.Substring (start, end - start).Trim ();
		}
		
		string ConstructMSDNUrl ()
		{
			StringBuilder sb = new StringBuilder (Name);
			ComparisonNode n = Parent;
			List <string> segments = new List <string> ();
			string name;
			
			segments.Add ("aspx");
			segments.Insert (0, FormatMyName ().ToLower ());
			n = Parent;
			while (n != null) {
				name = n.Name.ToLower ();
				if (name.EndsWith (".dll"))
					break;
				
				segments.Insert (0, n.Name.ToLower ());
				n = n.Parent;
			}

			string[] path = segments.ToArray ();
			return String.Join (".", path);
		}
		
		public ComparisonStatus Status;
		public readonly CompType Type;

		public ComparisonNode Parent;

		public readonly string Name;
		public readonly string TypeName;
		public readonly List<string> Messages;
		public readonly List<string> Todos;
		public bool ThrowsNIE;
		
		public int Extra;
		public int Missing;
		public int Present;
		public int Warning;
		public int Todo;
		public int Niex;

		public readonly List<ComparisonNode> Children;

		string msdnUrl;

		const string MSDN_BASE_URL = "http://msdn.microsoft.com/en-us/library/";
	}
}
