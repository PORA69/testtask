using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbfTests
{
    [TestClass]
    public class DbfTestTask
    {
        [TestMethod]
        public void TestTask()
        {
            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";

            // TODO read all RelevantFileName files recursively from RootDir (will be copied on build)
            // use DbfReader to read them and extract all DataValues
            // here an example call for one file:
            //var reader = new DbfReader();
            //var values = reader.ReadValues(@".\Data\ELEKTRO\E01\E600DI01\128.dbf");

            OutputRow.Headers = GetFiles(RootDir, RelevantFileName);
            var values = ReadValues(OutputRow.Headers);
            var outputs = OrderValuesByTimestamp(values).FillRange();

            // put all DataValues into ONE ordered (by timestamp) list of OutputRow (each timestamp shall exist only once, each file should be like a column)
            // the OutputRow has 2 lists: 1 static one for the headers (directory path of file) and one for the values (values of all files (same timestamp) must be merged into one OutputRow)
            //var outputs = new List<OutputRow>();
            // if there is time left, improve example where you think it isn't good enough

            // the following asserts should pass
            Assert.AreEqual(25790, outputs.Count);
            Assert.AreEqual(27, OutputRow.Headers.Count);
            Assert.AreEqual(27, outputs[0].Values.Count);
            Assert.AreEqual(27, outputs[11110].Values.Count);
            Assert.AreEqual(27, outputs[25789].Values.Count);
            Assert.AreEqual(633036852000000000, outputs.Min(o => o.Timestamp).Ticks);
            Assert.AreEqual(634756887000000000, outputs.Max(o => o.Timestamp).Ticks);
            Assert.AreEqual(633036852000000000, outputs[0].Timestamp.Ticks);
            Assert.AreEqual(634756887000000000, outputs.Last().Timestamp.Ticks);

            // write into file that we can compare results later on (you don't have to do something)
            //string content = $"Time\t{ string.Join("\t", OutputRow.Headers)}{Environment.NewLine}{string.Join(Environment.NewLine, outputs.Select(o => o.AsTextLine()))}";
            //File.WriteAllText(@".\output.txt", content);

            WriteResultsToFile(@".\output.txt", outputs);
        }

        /// <summary>
        /// Gets files from specified path which match the pattern provided. All Directories are searched.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns>List<string></returns>
        private List<string> GetFiles(string path, string pattern)
        {
            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories).ToList();
        }

        /// <summary>
        /// Reads values from provided file paths.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns>IEnumerable<DbfReader.ValueRow></returns>
        private IEnumerable<DbfReader.ValueRow> ReadValues(List<string> filePaths)
        {
            return filePaths.SelectMany(x => new DbfReader().ReadValues(x));
        }


        /// <summary>
        /// Orders values by Timestamp ascending.
        /// </summary>
        /// <param name="values"></param>
        /// <returns>List<OutputRow></returns>
        private List<OutputRow> OrderValuesByTimestamp(IEnumerable<DbfReader.ValueRow> values)
        {
            return values.GroupBy(x => x.Timestamp).OrderBy(x => x.Key).Select(x => new OutputRow
            {
                Timestamp = x.Key,
                Values = x.Select(y => y.Value as double?).ToList(),
            }).ToList();
        }

        /// <summary>
        /// Writes results to file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outputRows"></param>
        private void WriteResultsToFile(string path, List<OutputRow> outputRows)
        {
            File.WriteAllText(@".\output.txt", $"Time\t{ string.Join("\t", OutputRow.Headers)}{Environment.NewLine}{string.Join(Environment.NewLine, outputRows.Select(outputRow => outputRow.AsTextLine()))}");
        }
    }
}
