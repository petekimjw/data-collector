using CIM.Diagram.Model;

namespace CIM.Diagram
{
    public interface ITransfer<T, R>
        where T : Message, new()
        where R : class, new()
    {
    }
}