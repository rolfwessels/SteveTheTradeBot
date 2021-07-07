using System;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests
{
    [TestFixture]
    public class CodeScannerTests
    {
        private static readonly CodeSanner _codeSanner;

        #region Setup/Teardown

        public void Setup()
        {
        }

        #endregion

        [Test]
        [Ignore("not working right")]
        public void FindAllIssues()
        {
            // arrange
            Setup();

            // action
            var fileReports = _codeSanner.ScanNow();
            // assert
            fileReports.SelectMany(x => x.Issues)
                .GroupBy(x => x.Type)
                .ToDictionary(x => x.Key, x => x.Count())
                .Dump("Issue break down");
            foreach (var fileReport in fileReports.OrderBy(x => x.LinesOfCode))
                Console.Out.WriteLine(fileReport.ToString());
            fileReports.Should().HaveCount(0);
        }

        [Test]
        public void GetSourcePath_ShouldReturnTheSourceFiles()
        {
            // arrange
            Setup();
            // action
            _codeSanner.GetSourcePath().Should().EndWith("\\src");
            // assert
        }


        static CodeScannerTests()
        {
            _codeSanner = new CodeSanner();
        }
    }
}