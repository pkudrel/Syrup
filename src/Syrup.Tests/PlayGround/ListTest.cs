using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Syrup.Tests.PlayGround
{
    public class ListTest


    {
        public ListTest()
        {
            _nums = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18};
        }

        private readonly List<int> _nums;

        [Fact]
        public void FactMethodName()
        {
            var list1 = _nums.Take(5).ToList();
            var list2 = _nums.Skip(5).ToList();
        }
    }
}