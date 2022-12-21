using System.Linq;

namespace GitClient.Operations
{
    public interface IOperation
    {
        string Name { get; }
        void Operation();
    }
}