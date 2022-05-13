using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        private Process RunReports(string args, byte[] data = null)
        {
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
            return process;
        }

        private async Task<string> VersionAsync()
        {
            try
            {
                using (var result = new MemoryStream())
                {
                    var process = RunReports("version");
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    process.WaitForExit();
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
                    var jstring = (param is string) ? (string)param : JsonConvert.SerializeObject(param);
                    var process = RunReports($"render -l{LogLevel} - -", Encoding.UTF8.GetBytes(jstring));
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    process.WaitForExit();
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
                    var process = RunReports("parse -", pdf);
                    using (var stream = process.StandardOutput.BaseStream)
                    {
                        await stream.CopyToAsync(result);
                    }
                    process.WaitForExit();
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