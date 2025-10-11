using Xunit;

namespace Relevo.FunctionalTests;

/// <summary>
/// Collection definition to ensure tests run sequentially.
/// All test classes marked with [Collection("Sequential")] will run one at a time.
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollectionDefinition
{
}

