using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.Sockets;

namespace Sift
{
    class RemotingServer
    {
        #region Properties
        TcpChannel _channel;
        TcpChannel Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                _channel = value;
            }
        }
        #endregion

        /// <summary>
        /// Initialize the remoting services required by remote administration clients. (If allowed)
        /// </summary>
        public RemotingServer(int port)
        {
            Channel = new TcpChannel(port);

            ChannelServices.RegisterChannel(Channel, true);

            // register adapter settings service
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Resources.Settings.RemotingSettings),
                Resources.Constants.RemotingGetConfiguration,
                WellKnownObjectMode.Singleton);
        }

        ~RemotingServer()
        {
            ChannelServices.UnregisterChannel(Channel);
        }
    }
}
