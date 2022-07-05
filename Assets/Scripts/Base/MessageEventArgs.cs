using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Sockets
{
    public class MessageEventArgs : EventArgs
    {
        public String message = "";

        public MessageEventArgs(String _message)
        {
            this.message = _message;
        }
    }
}
