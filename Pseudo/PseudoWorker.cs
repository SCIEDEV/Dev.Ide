﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dev.Ide.Models;

namespace Dev.Ide.Pseudo
{
	public class PseudoWorker : IDisposable
	{
        public List<string> outputs = new List<string>();
        public List<string> errors = new List<string>();
        public bool terminate = false;

        Process p;
        RunCode run;
        public PseudoWorker(RunCode _run)
		{
            run = _run;
            Init();
        }

        private int timeoutSeconds = 120;

        public async void Init()
        {
            try
            {
                var task = Run();
                if (await Task.WhenAny(task, Task.Delay(timeoutSeconds * 1000)) == task)
                {
                    terminate = true;
                }
                else
                {
                    errors.Add("TIMEOUT AFTER 60 SECONDS");
                    terminate = true;
                }
                
                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                terminate = true;
                p.Close();
                p.Dispose();
            }
        }

        private bool canBeginInput = false;

		public async Task Run()
		{
            try
            {
                var filePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + $"/Buffer/{run.connectionId}.pseudo";
                var fileName = $"/Buffer/{run.connectionId}.pseudo";
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                run.code = run.code.Replace("OPENFILE", "");
                
                await File.WriteAllTextAsync(filePath, run.code);

                string enginePath = "/Pseudo/PseudoEngine2-macOS";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    enginePath = "/Pseudo/PseudoEngine2-v0.5.1-linux";
                }

                //enginePath = "/Pseudo/PseudoEngine2-v0.1-linux";

                var info = new ProcessStartInfo
                {
                    FileName = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + enginePath, // File to execute
                    Arguments = filePath, // arguments to use
                    UseShellExecute = false, // use process creation semantics
                    RedirectStandardOutput = true, // redirect standard output to this Process object
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true, // if this is a terminal app, don't show it
                    WindowStyle = ProcessWindowStyle.Hidden, // if this is a terminal app, don't show it,
                };

                p = new Process();
                p.StartInfo = info;

                p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                    {
                        //var output = p.StandardOutput.ReadToEnd();
                        if (e.Data != null) outputs.Add(e.Data);
                    }
                );

                p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    //var output = p.StandardOutput.ReadToEnd();
                    if (e.Data != null && e.Data.Contains("on line"))
                    {
                        errors.Add(e.Data.Substring(0, e.Data.Length - (filePath.Length + 4)));
                    }
                    else if (e.Data != null)
                    {
                        errors.Add(e.Data);
                    }
                });

                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                await Task.Delay(100);

                canBeginInput = true;

                foreach(var input in toInputQueue)
                {
                    p.StandardInput.WriteLine(input.Replace("OPENFILE", ""));
                    await Task.Delay(50);
                }

                /*while (!p.HasExited)
                {
                    await Task.Delay(100);
                }*/

                p.WaitForExit(1000 * 120);

                await Task.Delay(100);

                while(outputs.Count > 0 || errors.Count > 0)
                {
                    await Task.Delay(50);
                }

                p.Close();


                p.Dispose();
                // display what the process output

                Console.WriteLine("ECOMPLETE EXECUTION");
            }
            catch(Exception ex)
            {
                errors.Add(ex.ToString());
                errors.Add("INTERNAL EXCEPTION, PLEASE CONTACT MAINTAINER");
                p.WaitForExit(1000 * 120);
                p.Close();
                p.Dispose();
            }
        }

        private List<string> toInputQueue = new List<string>();

        public void Input(string input)
        {
            if(!canBeginInput)
            {
                toInputQueue.Add(input);
            }
            else
            {
                p.StandardInput.WriteLine(input);
            }
        }

        public void Dispose()
        {

        }
    }
}

