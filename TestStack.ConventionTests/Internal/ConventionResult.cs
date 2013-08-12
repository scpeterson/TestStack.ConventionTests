﻿namespace TestStack.ConventionTests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TestStack.ConventionTests.Reporting;

    public class ConventionResult : IConventionResult
    {
        readonly List<ResultInfo> conventionResults;
        readonly string dataDescription;

        public ConventionResult(string dataDescription)
        {
            this.dataDescription = dataDescription;
            conventionResults = new List<ResultInfo>();
        }

        public ResultInfo[] ConventionResults
        {
            get { return conventionResults.ToArray(); }
        }

        public void Is<T>(string resultTitle, IEnumerable<T> failingData)
        {
            // ReSharper disable PossibleMultipleEnumeration
            conventionResults.Add(new ResultInfo(
                failingData.None() ? TestResult.Passed : TestResult.Failed,
                resultTitle,
                dataDescription,
                failingData.Select(FormatData).ToArray()));
        }

        public void IsSymmetric<TResult>(
            string conventionResultTitle, IEnumerable<TResult> conventionFailingData,
            string inverseResultTitle, IEnumerable<TResult> inverseFailingData)
        {
            conventionResults.Add(new ResultInfo(
                conventionFailingData.None() ? TestResult.Passed : TestResult.Failed,
                conventionResultTitle,
                dataDescription,
                conventionFailingData.Select(FormatData).ToArray()));
            conventionResults.Add(new ResultInfo(
                inverseFailingData.None() ? TestResult.Passed : TestResult.Failed,
                inverseResultTitle,
                dataDescription,
                inverseFailingData.Select(FormatData).ToArray()));
        }

        public void IsSymmetric<TResult>(string conventionResultTitle, string inverseResultTitle,
            Func<TResult, bool> isInclusiveData, Func<TResult, bool> dataConformsToConvention,
            IEnumerable<TResult> allData)
        {
            var conventionFailingData = allData.Where(isInclusiveData).Where(d => !dataConformsToConvention(d));
            var inverseFailingData = allData.Where(d => !isInclusiveData(d)).Where(dataConformsToConvention);

            IsSymmetric(
                conventionResultTitle, conventionFailingData,
                inverseResultTitle, inverseFailingData);
        }

        static ConventionReportFailure FormatData<T>(T failingData)
        {
            var formatter = Convention.Formatters.FirstOrDefault(f => f.CanFormat(failingData));

            if (formatter == null)
            {
                throw new NoDataFormatterFoundException(
                    typeof (T).Name +
                    " has no formatter, add one with `Convention.Formatters.Add(new MyDataFormatter());`");
            }

            return formatter.Format(failingData);
        }
    }
}