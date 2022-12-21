namespace GitClient.Operations
{
    internal class Close : IOperation
    {
        public string Name => "Quit";
        public void Operation()
        {
            System.Environment.Exit(0);
        }
    }
}