using PCSC;
using PCSC.Iso7816;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcScWatcher
{
    public class SCardWatcher : IDisposable
    {
        private ISCardMonitor monitor;

        public Action<string> LogAction;

        public delegate void SCardUidReceivedHandler(object sender, CardUidReceivedEventArgs e);
        public event SCardUidReceivedHandler UidReceived;
        public bool IsInitialized
        {
            get
            {
                return monitor != null;
            }
        }

        public string ConnectedDeviceName
        {
            get
            {
                return monitor == null ? null : monitor.ReaderNames[0];
            }
        }

        public void Initialize()
        {
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    var readerNames = context.GetReaders();
                    monitor = MonitorFactory.Instance.Create(SCardScope.System);

                    monitor.CardInserted += Monitor_CardInserted;
                    monitor.Start(readerNames[0]);

                    LogAction?.Invoke($"SCardWatcher Initialized : Connected to {readerNames[0]}");
                }
            }
            catch (Exception e)
            {
                LogAction?.Invoke($"SCardWatcher Initialize Error : Problem with Auto connect.");
                LogAction?.Invoke(e.ToString());
                throw;
            }
           
        }

        public void Initialize(string explicitName)
        {
            try
            {
                monitor = MonitorFactory.Instance.Create(SCardScope.System);

                monitor.CardInserted += Monitor_CardInserted;
                monitor.Start(explicitName);

                LogAction?.Invoke($"SCardWatcher Initialized : Connected to {explicitName}");
            }
            catch(Exception e)
            {
                LogAction?.Invoke($"SCardWatcher Initialize Error : Problem with connected to {explicitName}.");
                LogAction?.Invoke(e.ToString());
                throw;
            }

        }

        private void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            var contextFactory = ContextFactory.Instance;

            try
            {
                using (var ctx = contextFactory.Establish(SCardScope.System))
                {
                    using (var isoReader = new IsoReader(ctx, monitor.ReaderNames[0], SCardShareMode.Shared, SCardProtocol.Any, false))
                    {

                        var apdu = new CommandApdu(IsoCase.Case2Short, isoReader.ActiveProtocol)
                        {
                            CLA = 0xFF,
                            Instruction = (InstructionCode)0xCA,
                            P1 = 0x00, 
                            P2 = 0x00, 
                            Le = 0x00 
                        };

                        var response = isoReader.Transmit(apdu);
                        var data = response.GetData();
                        UidReceived(this, new CardUidReceivedEventArgs(data));
                        LogAction?.Invoke("SCardWatcher Received : " + BitConverter.ToString(data));
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction?.Invoke("SCardWatcher Error - " + ex);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    monitor.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class CardUidReceivedEventArgs : EventArgs
    {
        public byte[] ReceivedData { get; set; }

        public CardUidReceivedEventArgs(byte[] data)
        {
            ReceivedData = data;
        }
    }
}
