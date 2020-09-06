using CIM.Diagram.Model;

namespace CIM.Diagram
{
    public interface IProcess
    {
        int Interval { get; }

        bool StartProcess();

        void StopProcess();

        void CloseProcess();
    }

    public class TriggerProcess<T, R> : IProcess
        where T : Message, new()
        where R : class, new()
    {
        public int Interval => throw new System.NotImplementedException();

        public void CloseProcess()
        {
            throw new System.NotImplementedException();
        }

        public bool StartProcess()
        {
            throw new System.NotImplementedException();
        }

        public void StopProcess()
        {
            throw new System.NotImplementedException();
        }
    }

    public class DataProcess<T, R> : IProcess
        where T : Message, new()
        where R : class, new()
    {
        public int Interval => throw new System.NotImplementedException();

        public void CloseProcess()
        {
            throw new System.NotImplementedException();
        }

        public bool StartProcess()
        {
            throw new System.NotImplementedException();
        }

        public void StopProcess()
        {
            throw new System.NotImplementedException();
        }
    }

}