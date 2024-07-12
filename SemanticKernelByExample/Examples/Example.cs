using Microsoft.SemanticKernel;

namespace SemanticKernelByExample.Examples;

public abstract class Example
{
    public abstract Task ExecuteAsync(Kernel kernel);
    public abstract override string ToString();
}