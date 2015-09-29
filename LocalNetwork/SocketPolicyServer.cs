// Socket Policy Server (sockpol)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//
// Based on XSP source code (ApplicationServer.cs and XSPWebSource.cs)
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (c) Copyright 2002-2007 Novell, Inc
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TicketMachine.LocalNetwork
{
    class SocketPolicyServer
    {
        const string PolicyFileRequest = "<policy-file-request/>";
        static byte[] request = Encoding.UTF8.GetBytes(PolicyFileRequest);
        private byte[] _policy;
        private int _policyPort = 843;
        private Socket _listenSocket;
        private Thread _thread;

        private AsyncCallback _acceptCallBack;

        public int PolicyPort
        {
            get 
            {
                return _policyPort;
            }
            set
            {
                _policyPort = value; 
            }
        }

        class Request
        {
            public Request(Socket s)
            {
                Socket = s;
                // the only answer to a single request (so it's always the same length)
                Buffer = new byte[request.Length];
                Length = 0;
            }

            public Socket Socket { get; private set; }
            public byte[] Buffer { get; set; }
            public int Length { get; set; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="xml">CrossDomain в формате xml</param>
        public SocketPolicyServer(string xml)
        {
            // transform the policy to a byte array (a single time)
            _policy = Encoding.UTF8.GetBytes(xml);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="xml">CrossDomain в формате xml</param>
        /// <param name="port">Номер порта</param>
        public SocketPolicyServer(string xml, int port)
            : this(xml)
        {
            _policyPort = port;
        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void Start()
        {
            try
            {
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(new IPEndPoint(IPAddress.Any, _policyPort));
                _listenSocket.Listen(500);
                _listenSocket.Blocking = false;
                Dispatcher.Logger.Trace("Policy service started on Port: " + _policyPort);
            }
            catch (SocketException se)
            {
                // Most common mistake: port 843 is not user accessible on unix-like operating systems
                if (se.SocketErrorCode == SocketError.AccessDenied)
                    Dispatcher.Logger.Error("Must be run as root if listen is <1024 to port " + _policyPort);

                Dispatcher.Logger.Error("Failed to start policy server: " + se.Message.ToString());
                Console.WriteLine();
                return;
            }

            _thread = new Thread(new ThreadStart(RunServer));
            _thread.Start();
        }

        //--------------------------------------------//
        // Начало работы сервера, ожидание соединения //
        //--------------------------------------------//
        void RunServer()
        {
            _acceptCallBack = new AsyncCallback(OnAccept);
            _listenSocket.BeginAccept(_acceptCallBack, null);

            while (true) // Just sleep until we're aborted.
                Thread.Sleep(1000000);
        }

        //--------------------------------//
        // Обработчик подключения клиента //
        //--------------------------------//
        void OnAccept(IAsyncResult ar)
        {
            Dispatcher.Logger.Trace("Incoming connection");
            Socket accepted = null;
            try
            {
                accepted = _listenSocket.EndAccept(ar);
            }
            catch
            {
            }
            finally
            {
                _listenSocket.BeginAccept(_acceptCallBack, null);
            }

            if (accepted == null)
                return;

            accepted.Blocking = true;

            Request request = new Request(accepted);
            accepted.BeginReceive(request.Buffer, 0, request.Length, SocketFlags.None, new AsyncCallback(OnReceive), request);
        }

        //---------------------------//
        // Обработчик приема запроса //
        //---------------------------//
        void OnReceive(IAsyncResult ar)
        {
            Request r = (ar.AsyncState as Request);
            Socket socket = r.Socket;
            try
            {
                r.Length += socket.EndReceive(ar);

                // compare incoming data with expected request
                for (int i = 0; i < r.Length; i++)
                {
                    if (r.Buffer[i] != request[i])
                    {
                        // invalid request, close socket
                        socket.Close();
                        return;
                    }
                }

                if (r.Length == request.Length)
                {
                    Dispatcher.Logger.Trace("Got policy request, sending response");
                    // request complete, send policy
                    socket.BeginSend(_policy, 0, _policy.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
                }
                else
                {
                    // continue reading from socket
                    socket.BeginReceive(r.Buffer, r.Length, request.Length - r.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), ar.AsyncState);
                }
            }
            catch
            {
                // if anything goes wrong we stop our connection by closing the socket
                socket.Close();
            }
        }

        //----------------------------//
        // Обработчик отправки ответа //
        //----------------------------//
        void OnSend(IAsyncResult ar)
        {
            Socket socket = (ar.AsyncState as Socket);
            try
            {
                socket.EndSend(ar);
            }
            catch
            {
                // whatever happens we close the socket
            }
            finally
            {
                socket.Close();
            }
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            _thread.Abort();
            _listenSocket.Close();
            Dispatcher.Logger.Trace("Policy server stopped.");
        }

        /// <summary>
        /// Политика без ограничений
        /// </summary>
        public const string AllPolicy =

    @"<?xml version='1.0'?>
<cross-domain-policy>
        <allow-access-from domain=""*"" to-ports=""*"" />
</cross-domain-policy>";

        /// <summary>
        /// Политика для портов 45040 - 45050
        /// </summary>
        public const string LocalPolicy =

    @"<?xml version='1.0'?>
<cross-domain-policy>
	<allow-access-from domain=""*"" to-ports=""45040-45050"" />
</cross-domain-policy>";
    }
}