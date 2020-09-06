using CIM.Diagram.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Diagram
{
    public abstract class ClientManager<T, R> : ISubject
            where T : Message, new()
            where R : class, new()
    {
        protected List<BaseClient<T, R>> ClientList = new List<BaseClient<T, R>>();
        public IEnumerable<IClient<T, R>> Clients => ClientList.OfType<IClient<T, R>>();
        public IEnumerable<IClientObserver> Controlls => ClientList.OfType<IClientObserver>();


        public bool IsCancelRequested { get; set; } = false;
        public LocalServerInfo LocalServer { get; set; }

        public ITransfer<T, R> Transfer { get; set; }

        public EManagerStatus Status { get; set; }

        public CommunicationState WebServiceState { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;

        public void Start()
        {
        }

        public async Task<bool> StopAsync()
        {
            return false;
        }

        public async Task<bool> CloseAsync()
        {
            return false;
        }

        public Task RestartAsync()
        {
            throw new NotImplementedException();
        }
    }
}
