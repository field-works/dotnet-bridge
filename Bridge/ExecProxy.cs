using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Threading.Tasks;

/**
 * .NET Bridgeで発生する例外
 * 
 */
namespace FieldWorks.FieldReports
{
    internal class ExecProxy : IProxy
    {
        private string ExePath { get; }
        private string WorkingDirectory { get; }
        private int LogLevel { get; set; }
        private TextWriter LogWriter { get; }

        public ExecProxy(
            string exePath, string cwd,
            int logLevel, TextWriter logWriter)
        {
            ExePath = exePath;
            WorkingDirectory = cwd;
            LogLevel = logLevel;
            LogWriter = logWriter;
        }

        private Task<Process> RunReportsAsync(string args, byte[] data = null)
        {
            var tcs = new TaskCompletionSource<Process>();

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(ExePath, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory,
                    RedirectStandardInput = data != null,
                    RedirectStandardOutput = true,
                    RedirectStandardError = LogLevel > 0,
                },
                EnableRaisingEvents = true,
            };
            process.Exited += (_, e) => tcs.SetResult(process);
            process.Start();
            if (LogLevel > 0) {
                process.ErrorDataReceived += (_, e) => LogWriter.WriteLine(e.Data);
                process.BeginErrorReadLine();
            }
            if (data != null)
            {
                using (var stream = process.StandardInput.BaseStream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return tcs.Task;
        }

        private async Task<string> VersionAsync()
        {
            try
            {
                using (var result = new MemoryStream())
                {
                    var process = await RunReportsAsync("version");
                    process.WaitForExit();
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    if (process.ExitCode != 0)
                        throw new Exception($"Exit Code = {process.ExitCode}");
                    return Encoding.UTF8.GetString(result.ToArray()).Trim();
                }
            }
            catch (Exception exn)
            {
                throw CreateReportsException(exn); 
            }
        }

        public string Version()
        {
            try
            {
                return VersionAsync().Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        public async Task<byte[]> RenderAsync(object param)
        {
            try
            {
                using (var result = new MemoryStream())
                {
                    var jstring = (param is string) ? (string)param : JsonSerializer.Serialize(param);
                    var process = await RunReportsAsync($"render -l{LogLevel} - -", Encoding.UTF8.GetBytes(jstring));
                    process.WaitForExit();
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    if (process.ExitCode != 0)
                        throw new Exception($"Exit Code = {process.ExitCode}");
                    return result.ToArray();
                }
            }
            catch (Exception exn)
            {
                throw CreateReportsException(exn); 
            }
        }

        public byte[] Render(object param)
        {
            try
            {
                return RenderAsync(param).Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        public async Task<string> ParseAsync(byte[] pdf)
        {
            try
            {
                using (var result = new MemoryStream())
                {
                    var process = await RunReportsAsync("parse -", pdf);
                    process.WaitForExit();
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    if (process.ExitCode != 0)
                        throw new Exception($"Exit Code = {process.ExitCode}");
                    return Encoding.UTF8.GetString(result.ToArray()).Trim();
                }
            }
            catch (Exception exn)
            {
                throw CreateReportsException(exn); 
            }
        }

        public string Parse(byte[] pdf)
        {
            try
            {
                return ParseAsync(pdf).Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        private Exception CreateReportsException(Exception exn)
        {
            return new ReportsException($"Process terminated abnormally: {exn.Message}.", exn); 
        }
    }
}