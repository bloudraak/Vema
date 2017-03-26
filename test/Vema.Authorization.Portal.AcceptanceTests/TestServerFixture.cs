#region License

// The MIT License (MIT)
// 
// Copyright (c) 2017 Werner Strydom
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;

namespace Vema.Authorization.Portal.AcceptanceTests
{
    public class TestServerFixture<TStartup> : IDisposable where TStartup : class
    {
        private const string SolutionName = "Vema.sln";
        private readonly ITestOutputHelper _log;
        private Lazy<TestServer> _testServer;
        private readonly Uri _baseAddress;


        public TestServerFixture(ITestOutputHelper log) : this(log, new Uri("http://localhost"))
        {
        }

        public TestServerFixture(ITestOutputHelper log, string baseAddress) : this(log, new Uri(baseAddress))
        {
        }

        public TestServerFixture(ITestOutputHelper log, Uri baseAddress)
        {
            _log = log;
            _baseAddress = baseAddress;
            _testServer = new Lazy<TestServer>(CreateTestServer);
        }

        public TestServer TestServer => _testServer.Value;

        private TestServer CreateTestServer()
        {
            var contentRoot = GetProjectPath("src");

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .UseStartup<TStartup>()
                .ConfigureRazorFix();

            builder.ConfigureServices(c =>
            {
                var provider = c.BuildServiceProvider();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                loggerFactory.AddXunit(_log);
            });

            var server = new TestServer(builder);
            server.BaseAddress = _baseAddress;
            return server;
        }

        private string GetProjectPath(string solutionRelativePath)
        {
            var assembly = typeof(TStartup).GetTypeInfo().Assembly;
            var projectName = assembly.GetName().Name;
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            _log.WriteLine("Searching for project `{0}` starting in directory `{1}`", projectName, applicationBasePath);
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, SolutionName));
                if (solutionFileInfo.Exists)
                {
                    var projectPath =
                        Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath, projectName));
                    _log.WriteLine("Found project project `{0}` in `{1}`", projectName, projectPath);
                    return projectPath;
                }
                directoryInfo = directoryInfo.Parent;
            } while (directoryInfo.Parent != null);

            Assert.True(false, $"Failed to find project `${projectName}`");
            return null;
        }

        public void Dispose()
        {
            if (_testServer != null && _testServer.IsValueCreated)
            {
                _testServer?.Value?.Dispose();
                _testServer = null;
            }
        }
    }
}