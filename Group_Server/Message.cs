using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group_Server
{
    public class Message
    {
        public string _senderIP;
        public string message;
        public int byteSize;
        public DateTime Date;

        public Message(string message, string IP, DateTime date)
        {
            _senderIP = IP;
            this.message = message;
            Date = date;
            byteSize = Encoding.UTF8.GetByteCount(message);
        }
    }
}
