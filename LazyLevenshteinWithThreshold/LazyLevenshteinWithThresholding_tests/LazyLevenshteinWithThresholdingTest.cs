using System;
using Xunit;

namespace LazyLevenshteinWithThresholding_tests
{
    public class LazyLevenshteinWithThresholdingTest
    {
        [Theory]
        [InlineData("", "", 0)]
        [InlineData("", "kitten", 6)]
        [InlineData("kitten", "", 6)]
        [InlineData("kitten", "sitting", 3)]
        [InlineData("sitting", "kitten", 3)]
        [InlineData("dump", "facility", 8)]
        [InlineData("executrix", "perfume", 8)]
        [InlineData("parking", "stab", 7)]
        [InlineData("deputy", "affinity", 6)]
        [InlineData("offense", "sour", 7)]
        [InlineData("wait", "frank", 4)]
        [InlineData("marketing", "thread", 7)]
        [InlineData("imposter", "bland", 8)]
        [InlineData("facility", "dump", 8)]
        [InlineData("perfume", "executrix", 8)]
        [InlineData("stab", "parking", 7)]
        [InlineData("affinity", "deputy", 6)]
        [InlineData("sour", "offense", 7)]
        [InlineData("frank", "wait", 4)]
        [InlineData("thread", "marketing", 7)]
        [InlineData("bland", "imposter", 8)]

        public void It_calculates_the_correct_string_distance(string string1, string string2, int expected)
        {
            Assert.Equal(expected, LazyLevenshteinWithThreshold.LazyLevenshteinWithThreshold.CalculateDistance(string1, string2));
        }

        [Theory]
        [InlineData("kitten", "sitting", 3, 3)]
        [InlineData("kitten", "sitting", 4, 3)]
        [InlineData("kitten", "sitting", 2, int.MaxValue)]
        [InlineData("sitting", "kitten", 2, int.MaxValue)]
        [InlineData("", "kitten", 6, 6)]
        public void IntMaxValue_is_returned_if_threshold_is_reached(string string1, string string2, int threshold, int expected)
        {
            Assert.Equal(expected, LazyLevenshteinWithThreshold.LazyLevenshteinWithThreshold.CalculateDistance(string1, string2, threshold));
        }

    }
}
