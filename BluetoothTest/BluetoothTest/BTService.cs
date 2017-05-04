using System.IO;
using System.Runtime.CompilerServices;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace BluetoothTest
{
   public class BTService
    {
        private const string Tag = "BTService";
        private const bool Debug = true;

        private const string Name = "BluetoothTest";
        private static UUID MY_UUID = UUID.FromString("");

        protected BluetoothAdapter _adapter;
        protected Handler _handler;
        private AcceptThread acceptThread;
        protected ConnectThread connectThread;
        private ConnectedThread connectedThread;
        protected int _state;

        public const int State_None = 0;
        public const int State_Listen = 1;
        public const int State_Connecting = 2;
        public const int State_Connected = 3;

        public BTService(Context context, Handler handler)
        {
            _adapter = BluetoothAdapter.DefaultAdapter;
            _state = State_None;
            _handler = handler; 
        }

        // En metode der siger status'en på selve chatten med en int. 
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetState(int state)
        {
            if (Debug)
                Log.Debug(Tag, "setState() " + _state + " -> " + state);

            _state = state;

            // GIVER EN BESKED VIDERE TIL GUI'EN. DER SKAL LIGE KIGGES PÅ DENNE HER SENERE. 

            //_handler.ObtainMessage(BluetoothTest.MESSAGE_STATE_CHANGE, state, -1).SendToTarget();
            
        }

        // Returnerer "resultatet"
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetState()
        {
            return _state; 
        }

        // Starter selve servicen og sætter vores accept tråd til at lytte til serveren.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            if (Debug)
                Log.Debug(Tag, "Start");

            // Stopper alle tråde for at prøve at connecte
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if(connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // starter tråden ved at lytte på BT server socket
            if(acceptThread == null)
            {
                acceptThread = new AcceptThread(this);
                acceptThread.Start();
            }

            SetState(State_Listen);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(BluetoothDevice device)
        {
            if (Debug)
                Log.Debug(Tag, "Connect to: " + device);

            // Igen sørger vi for at der ikke er andre tråde der prøver at få forbindelse
            if (_state == State_Connecting)
            {
                if (connectThread != null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            connectThread = new ConnectThread(device, this);
            connectThread.Start();

            SetState(State_Connecting);
        }

        // Når vi så er connected så skal vi styre vores connection
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            if (Debug)
                Log.Debug(Tag, "Connected");

            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

           if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Canceller accepttråden fordi vi kun vil have et device på vores connection. 
            if (acceptThread != null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }

            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Start();
            
            // Her skal der være noget der sender vores connected device til gui.

            SetState(State_Connected);

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (Debug)
                Log.Debug(Tag, "stop");

            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (acceptThread != null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }

            SetState(State_None);
        }

        // Udskriver beskeden usynkroniseret
        public void Write(byte[] @out)
        {
          
            ConnectedThread r;
           
            lock (this)
            {
                if (_state != State_Connected)
                    return;
                r = connectedThread;
            }
            
            r.Write(@out);
        }

        private class AcceptThread : Thread
        {
            private BluetoothServerSocket serverSocket;
            private
        }
    }
}
