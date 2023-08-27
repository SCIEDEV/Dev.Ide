using System;
using Dev.Ide.Models;

namespace Dev.Ide.Pseudo
{
	public static class PsuedoEngine
	{
		public static Dictionary<string, PseudoWorker> workers = new Dictionary<string, PseudoWorker>();

		public static async void ExecuteCode(RunCode run)
		{
			try
			{
				if (workers.ContainsKey(run.connectionId))
				{
					workers.Remove(run.connectionId);
				}

				var worker = new PseudoWorker(run);
				workers.Add(run.connectionId, worker);
			}
			catch (Exception ex)
			{

			}
		}

		public static async void Input(TermInput input)
		{
			try
			{
                if (workers.ContainsKey(input.connectionId)) workers[input.connectionId].Input(input.input);
            }
			catch (Exception ex)
			{

			}
		}
	}
}

