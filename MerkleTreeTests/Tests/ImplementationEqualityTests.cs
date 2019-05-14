using Xunit.Abstractions;

namespace MerkleTests.Tests
{
    public class ImplementationEqualityTests
    {
        protected readonly ITestOutputHelper Output;
        
        public ImplementationEqualityTests(ITestOutputHelper output)
        {
            Output = output;
        }
    }
}