using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp2;
using System.Net;
using System.Net.Sockets;
using RtmpSharp2.Abstract;
using RtmpSharp2.Abstract.CommandMessages;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ExampleApp
{
    class Client : RtmpSharp2.Abstract.ClientBase
    {
        private TcpClient client;
        private bool _connect = true;
        private bool _sendToken = true;
//        private TwitchUsher _usher;

        public Client(string host, int port)
        {
            client = new TcpClient(host, port);
            client.Client.Blocking = false;
            client.NoDelay = true;
            client.SendBufferSize = 10000;
            client.ReceiveBufferSize = 10000;
//            _usher = usher;
        }

        protected override void SendData(byte[] array)
        {
            client.Client.Send(array);
//            System.Diagnostics.Debug.WriteLine(Encoding.ASCII.GetString(array));
        }

        protected override bool CanReceiveData()
        {
            return client.Available > 0;
        }

        protected override byte[] ReceivedData()
        {
        	
            var buffer = new byte[client.Available];
            var ret = client.Client.Receive(buffer);
            
            return buffer;
        }

        protected override void Debug(string str)
        {
            base.Debug(str);
            Console.WriteLine(str);
            System.Diagnostics.Debug.WriteLine(str);
        }

        public override void Update()
        {
            base.Update();
            if (CurrentState == ClientStates.Handshake_Done)
            {
                if (_connect)
                {
                    _connect = false;
					
                    var url = "rtmp://nlace02.live.nicovideo.jp:1935/fileorigin/02";
                    var que = "/content/20181130/lv317142895_040012248000_1_3e0752.f4v";
                    var ticket = "225832:lv317142895:0:1543535055:57db0469d58860d4";
                    makeTs(url, que, ticket);
                    
                    
                }
            }
        }
        protected override void ParseChunk(Chunk chunk)
        {
            base.ParseChunk(chunk);
            
        }
		/*
        private void sendToken()
        {
        	/*
            if (_usher.Streams.Count == 0)
                return;

            AmfWriter writer = new AmfWriter();

            writer.WriteString("NetStream.Authenticate.UsherToken");
            writer.WriteNumber(0);
            writer.WriteNull();
//            writer.WriteString(_usher.Streams[0].Token);

            RtmpSharp2.Abstract.CommandMessage message = new CommandMessage(writer);

            SendMessage(message);
            SendMessage(new CreateStream());
            SendMessage(new Play(_usher.Streams[0].PlayStream));
            *
        }
        */
       public static string getRegGroup(string target, string reg, int group = 1, Regex r = null) {
			if (r == null)
				 r = new Regex(reg);
			var m = r.Match(target);
	//		Console.WriteLine(m.Groups.Count +""+ m.Groups[0]);
			if (m.Groups.Count>group) {
				return m.Groups[group].ToString();
			} else return null;
		}
		private bool makeTs(string url, string que, string ticket) {
       		
			SendMessage(new RtmpSharp2.Abstract.ControlMessages.SetChunkSize(10000));
            var connect = new Connect(ticket);
            connect.PageUrl = "http://live.nicovideo.jp/nicoliveplayer.swf";
            connect.SwfUrl = "http://live.nicovideo.jp/nicoliveplayer.swf";
            /*
            connect.App = "fileorigin/01";
            connect.ServerUrl = "rtmp://nlace02.live.nicovideo.jp:1935/fileorigin/01";         
            connect.Data = Encoding.ASCII.GetBytes("225832:lv316983063:0:1543462925:7c12cb3f6a81c679");
            */
           connect.App = getRegGroup(url, "(fileorigin.+)");
           connect.ServerUrl = url;
           connect.Data = Encoding.ASCII.GetBytes(ticket);
           
            connect.ApplyValues(ticket);
            SendMessage(connect);
            
            
            var createStream = new CreateStream();
            createStream.ApplyValues();
            SendMessage(createStream);
            
            
            for (var i = 0; i < 200; i++) {
                var cfr = new byte[]{0x02,0x00,0x0f,0x73,0x65,0x6e,0x64,0x46,0x69,0x6c,0x65,0x52,0x65,0x71,0x75,0x65,
						0x73,0x74,0x00,0x40,0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x05,0x11,0x09,0x03,0x01,
						0x06}; //,0x6f
                var len = (byte)0x6f;
                //var que = Encoding.ASCII.GetBytes(getRegGroup("/content/20181128/lv316983063_200012298000_1_659aac.f4v");
                var _que = Encoding.ASCII.GetBytes(que);
            	var arr = new List<byte>();
            	arr.AddRange(cfr);
            	arr.Add(len);
            	arr.AddRange(_que);
				
            	var r = new RtmpSharp2.Abstract.CommandMessage(arr.ToArray());
                
				SendData(r.ToBytes());
				
				client.Client.Blocking = true;
				var b = new byte[10000];
				var s = client.GetStream();
				var ii = s.Read(b, 0, b.Length);
				var ch = new char[b.Length];
				string a = "";
				System.Diagnostics.Debug.WriteLine(i);
				for (var iii = 0; iii < ii; iii++) {
					if (b[iii] < 33 || b[iii] > 126) continue;
					
					a += (char)b[iii];
//					System.Diagnostics.Debug.WriteLine((int)ch[iii]);
//					System.Diagnostics.Debug.Write(ch[iii]);
				}
				System.Diagnostics.Debug.WriteLine(a);
				if (a.IndexOf("/content/") > -1) return true;
				//System.Diagnostics.Debug.WriteLine(Encoding.ASCII.GetString(b));
				if (i > 1)
					System.Threading.Thread.Sleep(1000);
            }
            return false;
		}
    }
}
