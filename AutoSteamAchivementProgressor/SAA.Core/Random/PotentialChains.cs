using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAA.Core.Random
{
	public class PotentialChains
	{
		public string id { get; set; } = string.Empty; // flattened array of ints which is the achivement ids in unlock order. separator is a dash. ex: "5-1-3-2-4-8-7-12"
		List<(int, double)> achivSteps = new List<(int, double)>(); // Add pairs like this: achivSteps.Add((1, 1.23));
	}
}
