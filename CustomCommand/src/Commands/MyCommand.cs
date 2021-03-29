using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using BuildVision.Common.Logging;
using EnvDTE;
using Spring.Rest.Client;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Spring.Http;
using Spring.Http.Converters;
using Spring.Http.Converters.Json;
using System.Net.Http;
using Spring.IO;
using System.Text;
using System.Net.Http.Headers;

namespace CustomCommandSample
{
    internal sealed class MyCommand
    {


        public static async Task InitializeAsync(AsyncPackage package, EnvDTE80.DTE2 dte2)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;

            // must match the button GUID and ID specified in the .vsct file
            var cmdId = new CommandID(Guid.Parse("2b40859b-27f8-4dc6-85b1-f253386aa5f6"), 0x0100);
            var cmd = new MenuCommand((s, e) => Execute(package), cmdId);
            commandService.AddCommand(cmd);

            DTE dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            string result = dte.Solution.FullName;

            RestTemplate template = new RestTemplate();
            template.MessageConverters.Add(new ByteArrayHttpMessageConverter());
            template.MessageConverters.Add(new ResourceHttpMessageConverter());
            template.MessageConverters.Add(new FormHttpMessageConverter());


            Spring.Http.HttpHeaders header = new Spring.Http.HttpHeaders();
            header.Add("Content-Type", "application/json");
            header.Add("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiIzZjY3NTAyNS1kNTRjLTQ2YzAtOTU0MC0wY2MxOGY3YjU5YTAiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vYTEzMTIxZDUtZGVjOS00MWE1LWEyZjktNTU4MTkyOGY3M2UzL3YyLjAiLCJpYXQiOjE2MTcwMTk3ODMsIm5iZiI6MTYxNzAxOTc4MywiZXhwIjoxNjE3MDIzNjgzLCJhaW8iOiJFMlpnWVBqcjBtRGFvN0Q3WU5YOGlzL1BXTnhNQVE9PSIsImF6cCI6IjNmNjc1MDI1LWQ1NGMtNDZjMC05NTQwLTBjYzE4ZjdiNTlhMCIsImF6cGFjciI6IjEiLCJvaWQiOiIzOGNjZTI3MS1mOTE3LTQ0MmQtYjhhMy0xMDY5ZWU0OTJmYzkiLCJyaCI6IjAuQVNJQTFTRXhvY25lcFVHaS1WV0Jrbzl6NHlWUVp6OU0xY0JHbFVBTXdZOTdXYUFqQUFBLiIsInN1YiI6IjM4Y2NlMjcxLWY5MTctNDQyZC1iOGEzLTEwNjllZTQ5MmZjOSIsInRpZCI6ImExMzEyMWQ1LWRlYzktNDFhNS1hMmY5LTU1ODE5MjhmNzNlMyIsInV0aSI6Ijd5cmxHZTItZzBxbVVpZThaQ1BiQUEiLCJ2ZXIiOiIyLjAifQ.YTe_xFmG6G8SPKUw8b3wJAf9qdy9bbbbGbgt-mZVU2yCUo-ca1WsUh_r02Oge0BcsKEN96rDHY7BK6H2Gg1-HSTflCHmDemWG2aWr9Pqp8J0PUw-_kSJfUzYHfmILpWEK5duxLBk74F9G0Qmbr5kryOffuU1Kvgwg75jEFnA_THQ3Jd8Gu9KAgS16juvms-67LE9ccChvtf_KoTpfTRcAH1kZ3F4Gy8_bfz92GHo44NaNtUVSH48Xt4rRl-Kje2qYecc3HAohOUDv6PBeC-WzQfKbPChC9MUHneZePIz6ttnFowGaXrCgNvbNBKJwv_-2lMR4qExbm2Cp3X7xKC6Jw");




            string[] paths = Directory.GetFiles(Directory.GetParent(result).ToString(), "*.csproj", SearchOption.AllDirectories);


            EnvDTE.OutputWindowPanes panes =
              dte2.ToolWindows.OutputWindow.OutputWindowPanes;
            foreach (EnvDTE.OutputWindowPane pane in panes)
            {
                if (pane.Name.Contains("Build"))
                {
                    pane.OutputString("HL" + "\n");
                    pane.Activate();
                    // return;
                }
            }

            CreatePane(dte2, "AVAS");

            EnvDTE.OutputWindowPane avasPane = null;
            foreach (EnvDTE.OutputWindowPane pane in panes)
            {
                if (pane.Name.Contains("AVAS"))
                {
                    pane.OutputString("HELLO FROM AVAS" + "\n");
                    pane.Activate();
                    avasPane = pane;
                    //return;
                }
            }


            foreach (string path in paths)
            {
                LogManager.ForContext<Project>().Information("Message " + path);
                FullReport fullReport = new FullReport();
                fullReport.DependencyReports = new List<DependencyReport> { GetDependencyReport(path, "project") };
                fullReport.IsFast = true;
                string output = JsonSerializer.Serialize(fullReport, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                //    string output = JsonConvert.SerializeObject();


                LogManager.ForContext<Project>().Information("Output report " + output);

                if (output != null && !output.Equals("null"))
                {
                    HttpEntity httpEntity = new HttpEntity(output, header);



                    Task<string> stringAnswer = template.PostForObjectAsync<string>("http://127.0.0.1:6345/api/article/package", httpEntity);

                    string answer = await stringAnswer;

                    LogManager.ForContext<Project>().Information("Output body1 " + answer);




                    /* HttpClient client = new HttpClient();
                     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiIzZjY3NTAyNS1kNTRjLTQ2YzAtOTU0MC0wY2MxOGY3YjU5YTAiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vYTEzMTIxZDUtZGVjOS00MWE1LWEyZjktNTU4MTkyOGY3M2UzL3YyLjAiLCJpYXQiOjE2MTY3MDcyODgsIm5iZiI6MTYxNjcwNzI4OCwiZXhwIjoxNjE2NzExMTg4LCJhaW8iOiJFMlpnWUxCZCtNK1BNNHZ6TXVldmlZRmNQZy8vQWdBPSIsImF6cCI6IjNmNjc1MDI1LWQ1NGMtNDZjMC05NTQwLTBjYzE4ZjdiNTlhMCIsImF6cGFjciI6IjEiLCJvaWQiOiIzOGNjZTI3MS1mOTE3LTQ0MmQtYjhhMy0xMDY5ZWU0OTJmYzkiLCJyaCI6IjAuQVNJQTFTRXhvY25lcFVHaS1WV0Jrbzl6NHlWUVp6OU0xY0JHbFVBTXdZOTdXYUFqQUFBLiIsInN1YiI6IjM4Y2NlMjcxLWY5MTctNDQyZC1iOGEzLTEwNjllZTQ5MmZjOSIsInRpZCI6ImExMzEyMWQ1LWRlYzktNDFhNS1hMmY5LTU1ODE5MjhmNzNlMyIsInV0aSI6Ik45ZkFTVHRrZUV1aTMxeVhCUUFHQUEiLCJ2ZXIiOiIyLjAifQ.CpGv9fvI465JzAS5qzgCANLFsI2HrFhqw9u-k5UkZJ7VdAstF76ZPdRsMt0uUjYE_J_atAXpPQ2bSgRq2ahIweFga51LNvx1dY-ujuvwSInwJbF13i1Q54r159WIV5FuhkRu3C-v7s4rcC5Y7axpO4aZ05IuRI70bPgwTzbKRoxyOrl_sDtLQ8kIiWqifFqobrfQwT_hD3Mu7pEVrerchnvkjD2Qj7c2dynF0L3CDqrkeu11ceGhqvHH4g8OZk29yT4x4so6UcB4QMNcY332YPIX4xPbVkP2TlckpAarqVxAQyp1r9p1GtlyLZvzWd_xLrAJgZa6YtCYaoSnrN-uFg");

                     MultipartFormDataContent form = new MultipartFormDataContent();
                     FileInfo file = new FileInfo(@"C:\\Users\\andro\\source\\repos\\ConsoleApp1\\Lms.Common.csproj");
                     form.Add(new StreamContent(file.OpenRead()), "file", file.Name);
                     System.Net.Http.HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:6345/api/article/package1", form);

                     LogManager.ForContext<Project>().Information("Output body1 " + response.StatusCode);
                     LogManager.ForContext<Project>().Information("Output body1 " + response.ToString());
                     LogManager.ForContext<Project>().Information("Output body1 " + response.ReasonPhrase);
                     LogManager.ForContext<Project>().Information("Output body1 " + Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync())); 
                    */

                    /* Dictionary<string, object> keyValues = new Dictionary<string, object>();
                     keyValues["file"] = new ByteArrayResource(File.ReadAllBytes("C:\\Users\\andro\\source\\repos\\ConsoleApp1\\Lms.Common.csproj"));

                     httpEntity = new HttpEntity(keyValues, header);
                     stringAnswer = template.PostForObjectAsync<string>("http://127.0.0.1:6345/api/article/package1", httpEntity);
                     answer = await stringAnswer;

                     LogManager.ForContext<Project>().Information("Output body2 " + answer);*/

                    /*if (stringAnswer.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        
                    } else
                    {
                        LogManager.ForContext<Project>().Information("Output body " + stringAnswer.StatusCode + " " + stringAnswer.StatusDescription + " " + stringAnswer.ToString());
                    }*/
                    //TODO wfsdfsdf

                    //TODO wfsdfsdf
                    avasPane.Activate();
                    avasPane.OutputString("HELLO FROM AVAS" + "\n" + " answer " + answer);

                }


                // 
            }




            /*  dte.Solution.SolutionBuild.Build();
              dte.Solution.SolutionBuild.Clean();*/
        }
        static void CreatePane(EnvDTE80.DTE2 dte, string title)
        {

            OutputWindowPanes panes =
                dte.ToolWindows.OutputWindow.OutputWindowPanes;

            try
            {
                // If the pane exists already, write to it.
                panes.Item(title);


            }
            catch (ArgumentException)
            {
                panes.Add(title);
                // Create a new pane and write to it.

            }
        }




        public static DependencyReport GetDependencyReport(string path, string projectName)
        {
            var logFile = File.ReadAllLines(path);
            var logList = new List<string>(logFile);
            List<Dependency> dependencies = new List<Dependency>();
            LogManager.ForContext<Project>().Information("Count of lines  " + logList.Count);
            foreach (string line in logList)
            {
                if (line.Contains("Reference") && line.Contains("Version"))
                {
                    string[] splitArray = line.Replace("PackageReference", "").Replace("Reference", "").
                        Replace("<", "").Replace(">", "").Replace("\\", "").Replace("\t", "").Replace("/n", "").Replace("\"", "").Split(' ');
                    string product = null;
                    string version = null;
                    LogManager.ForContext<Project>().Information("Count of lines2  " + splitArray.Length);
                    foreach (string element in splitArray)
                    {
                        if (element.Contains("Include"))
                        {
                            product = element.Split('=')[1];
                        }
                        else if (element.Contains("Version"))
                        {
                            version = element.Split('=')[1];
                        }

                    }
                    if (product != null && version != null)
                    {
                        Dependency dependency = new Dependency
                        {
                            Vendor = "nuget",
                            Product = product,
                            Version = version
                        };

                        dependencies.Add(dependency);
                    }

                }
            }
            if (dependencies.Count == 0)
            {
                return null;
            }
            return new DependencyReport
            {
                FileName = path.Substring(path.LastIndexOf(projectName) + projectName.Length),
                Dependencies = dependencies,
                Type = "nuget"
            };

        }




        private static void Execute(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VsShellUtilities.ShowMessageBox(
                package,
                $"AVAS: package scanner was executed please check output pane AVAS",
                nameof(MyCommand),
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
