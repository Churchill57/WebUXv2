namespace WebUXv2.Components
{
    public interface IContextFinder
    {
        void ForceNextContextSearch();
        bool EntityExists(int id);
    }
}