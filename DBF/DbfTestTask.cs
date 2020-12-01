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

        [TestMethod]
        public void SecondTestTask()
        {
            // Clear the header list set by the previous test
            OutputRow.Headers.Clear();

            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";

            var outputs = GetOrderedFileDataBinarySearch(GetFiles(RootDir, RelevantFileName));

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

            WriteResultsToFile(@".\output_second_test.txt", outputs);
        }

        [TestMethod]
        public void ThirdTestTask()
        {
            // Clear the header list set by the previous test
            OutputRow.Headers.Clear();

            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";

            var outputs = GetOrderedFileDataDictionary(GetFiles(RootDir, RelevantFileName));

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

            WriteResultsToFile(@".\output_second_test.txt", outputs);
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
        /// Gets data from files and maintains order of values in files using BinarySearch.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private List<OutputRow> GetOrderedFileDataBinarySearch(List<string> files)
        {
            List<OutputRow> outputs = new List<OutputRow>();
            foreach (var file in files)
            {
                OutputRow.Headers.Add(file);
                var values = new DbfReader().ReadValues(file);
                foreach (var value in values)
                {
                    var item = new OutputRow
                    {
                        Timestamp = value.Timestamp,
                        Values = new List<double?>(new double?[files.Count])
                    };
                    var index = outputs.BinarySearch(item);
                    if (index < 0)
                    {
                        item.Values[OutputRow.Headers.Count - 1] = value.Value;
                        outputs.Insert(~index, item);
                    }
                    else
                    {
                        outputs[index].Values[OutputRow.Headers.Count - 1] = value.Value;
                    }
                }
            }
            return outputs.OrderBy(x => x.Timestamp).ToList();
        }

        /// <summary>
        /// Gets data from files and maintains order of values in files using a Dictionary.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private List<OutputRow> GetOrderedFileDataDictionary(List<string> files)
        {
            Dictionary<DateTime, List<double?>> keyValues = new Dictionary<DateTime, List<double?>>();
            foreach (var file in files)
            {
                OutputRow.Headers.Add(file);
                var values = new DbfReader().ReadValues(file);
                foreach (var value in values)
                {
                    var item = new OutputRow
                    {
                        Timestamp = value.Timestamp,
                        Values = new List<double?>(new double?[files.Count])
                    };

                    if (keyValues.ContainsKey(item.Timestamp))
                    {
                        keyValues[item.Timestamp][OutputRow.Headers.Count - 1] = value.Value;
                    }
                    else
                    {
                        item.Values[OutputRow.Headers.Count - 1] = value.Value;
                        keyValues.Add(value.Timestamp, item.Values);
                    }
                }
            }
            return keyValues.GroupBy(x => x.Key).OrderBy(x => x.Key).Select(x => new OutputRow
            {
                Timestamp = x.Key,
                Values = x.SelectMany(y => y.Value).ToList(),
            }).ToList();
        }

        /// <summary>
        /// Writes results to file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outputRows"></param>
        private void WriteResultsToFile(string path, List<OutputRow> outputRows)
        {
            File.WriteAllText(path, $"Time\t{ string.Join("\t", OutputRow.Headers)}{Environment.NewLine}{string.Join(Environment.NewLine, outputRows.Select(outputRow => outputRow.AsTextLine()))}");
        }
    }
}
