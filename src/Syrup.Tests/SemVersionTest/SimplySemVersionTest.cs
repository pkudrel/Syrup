using Syrup.Self.Parts.Sem;
using Xunit;

namespace Syrup.Tests.SemVersionTest
{
    public class SimplySemVersionTest
    {
        [Theory]
        [InlineData("0.0.0", "0.0.0", false)]
        [InlineData("0.0.0", "1.0.0", true)]
        [InlineData("0.0.0", "0.1.0", true)]
        [InlineData("0.0.0", "0.0.1", true)]
        [InlineData("1.0.0", "0.0.0", false)]
        [InlineData("0.1.0", "0.0.0", false)]
        [InlineData("0.0.1", "0.0.0", false)]
        [InlineData("1.0.0", "1.0.0", false)]
        [InlineData("0.1.0", "0.1.0", false)]
        [InlineData("0.0.1", "0.0.1", false)]
        [InlineData("1.1.0", "1.1.0", false)]
        [InlineData("1.0.1", "1.0.1", false)]
        [InlineData("1.1.1", "1.1.1", false)]
        [InlineData("0.2.4 ", "0.2.4", false)]
        private void CompareLessThen(string a, string b, bool result)
        {
            var va = SemVersion.Parse(a);
            var vb = SemVersion.Parse(b);
            Assert.Equal(result, va < vb);
        }

        [Theory]
        [InlineData("0.0.0", "0.0.0", false)]
        [InlineData("0.0.0", "1.0.0", false)]
        [InlineData("0.0.0", "0.1.0", false)]
        [InlineData("0.0.0", "0.0.1", false)]
        [InlineData("1.0.0", "0.0.0", true)]
        [InlineData("0.1.0", "0.0.0", true)]
        [InlineData("0.0.1", "0.0.0", true)]
        [InlineData("1.0.0", "1.0.0", false)]
        [InlineData("0.1.0", "0.1.0", false)]
        [InlineData("0.0.1", "0.0.1", false)]
        [InlineData("1.1.0", "1.1.0", false)]
        [InlineData("1.0.1", "1.0.1", false)]
        [InlineData("1.1.1", "1.1.1", false)]
        public void CompareGreaterThen(string a, string b, bool result)

        {
            var va = SemVersion.Parse(a);
            var vb = SemVersion.Parse(b);

            Assert.Equal(result, va > vb);
        }

        [Theory]
        [InlineData("0.0.0", "0.0.0", true)]
        [InlineData("0.0.0", "1.0.0", false)]
        [InlineData("0.0.0", "0.1.0", false)]
        [InlineData("0.0.0", "0.0.1", false)]
        [InlineData("1.0.0", "0.0.0", false)]
        [InlineData("0.1.0", "0.0.0", false)]
        [InlineData("0.0.1", "0.0.0", false)]
        [InlineData("1.0.0", "1.0.0", true)]
        [InlineData("0.1.0", "0.1.0", true)]
        [InlineData("0.0.1", "0.0.1", true)]
        [InlineData("1.1.0", "1.1.0", true)]
        [InlineData("1.0.1", "1.0.1", true)]
        [InlineData("1.1.1", "1.1.1", true)]
        public void CompareEqual(string a, string b, bool result)

        {
            var va = SemVersion.Parse(a);
            var vb = SemVersion.Parse(b);
            Assert.Equal(result, va == vb);
        }


    }
}