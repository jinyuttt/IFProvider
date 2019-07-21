using LibInterfaceProvider;

namespace ClientProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            var cls=  ClsProvider.Create<ICall>();

            cls.Call();
        }
    }
}
