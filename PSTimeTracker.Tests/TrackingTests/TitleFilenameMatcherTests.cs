using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSTimeTracker.Tracking;
using Xunit;

namespace PSTimeTracker.Tests.TrackingTests
{
    public class TitleFilenameMatcherTests
    {
        internal TitleFilenameMatcher _sut = new TitleFilenameMatcher();

        [Theory]
        [InlineData("Untitled-1 @ 54% (Layer1, RGB/8)*", "Untitled-1")]
        [InlineData("Untitled-1 @ 0% (Layer1, RGB/8)*", "Untitled-1")]
        [InlineData("Untitled-1 @ 0% (layer@asd, RGB/8)*", "Untitled-1")]
        [InlineData("Untitled-1 @ 0% (layer@asd, CMYK/16)*", "Untitled-1")]
        [InlineData("Untitled-1@asd @ 0% (layer@asd, CMYK/16)*", "Untitled-1@asd")]
        [InlineData("Untitled-1 @   asd @ 0% (layer@asd, CMYK/16)*", "Untitled-1 @   asd")]
        [InlineData("Untitled-1    @   asd @ 123% (layer@asd, CMYK/16)*", "Untitled-1    @   asd")]
        public void ShouldMatchProperly(string title, string expected)
        {
            string result = _sut.GetFilename(title);
            Assert.Equal(expected, result);
        } 

    }
}
