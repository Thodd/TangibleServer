using System;
using System.Windows;
using Microsoft.Surface.Core;
using System.Windows.Interop;
using System.Net;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Net.WebSockets;
using Alchemy;
using Alchemy.Classes;
using System.Collections.Generic;

namespace TangibleServer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TouchTarget TouchTarget;
        private bool LoggingActivated = true;

        public MainWindow()
        {
            InitializeComponent();
            this.LogMessages.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //make the stuff available through a websocket server
            SetupWebSocketServer();
            SetupTangibleHook();
        }

        #region Handling Tangible contacts

        private void SetupTangibleHook()
        {
            //try to setup a Surface TouchTarget in order to capture the Touch-/Contact-Events
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;

                this.TouchTarget = new TouchTarget(IntPtr.Zero, EventThreadChoice.OnBackgroundThread);
                this.TouchTarget.EnableInput();
                this.TouchTarget.TouchDown += new EventHandler<Microsoft.Surface.Core.TouchEventArgs>(ContactDownHandler);
                this.TouchTarget.TouchMove += new EventHandler<Microsoft.Surface.Core.TouchEventArgs>(ContactMoveHandler);
                this.TouchTarget.TouchUp += new EventHandler<Microsoft.Surface.Core.TouchEventArgs>(ContactUpHandler);

                this.LogWindow("Tangible-Hook successfully initialised, Handle: " + handle);
            }
            catch (Exception ex)
            {
                this.LogWindow("Oh no! Something failed :( See console for more information.");
                Console.WriteLine(ex);
            }
        }

        private void ContactDownHandler(object sender, Microsoft.Surface.Core.TouchEventArgs e)
        {
            if (e.TouchPoint.IsTagRecognized)
            {
                Tags2JSData data = new Tags2JSData("tag_down", e.TouchPoint);
                this.BroadcastMessageToAllClients(data.GetJSONString());
                this.LogContactInformation(e.TouchPoint, "added");
            }  
        }

        private void ContactMoveHandler(object sender, Microsoft.Surface.Core.TouchEventArgs e)
        {
            if (e.TouchPoint.IsTagRecognized)
            {
                Tags2JSData data = new Tags2JSData("tag_move", e.TouchPoint);
                this.BroadcastMessageToAllClients(data.GetJSONString());
                this.LogContactInformation(e.TouchPoint, "moved");
            }
        }

        private void ContactUpHandler(object sender, Microsoft.Surface.Core.TouchEventArgs e)
        {
            if (e.TouchPoint.IsTagRecognized)
            {
                Tags2JSData data = new Tags2JSData("tag_up", e.TouchPoint);
                this.BroadcastMessageToAllClients(data.GetJSONString());
                this.LogContactInformation(e.TouchPoint, "removed");
            }            
        }

        #endregion

        #region WebSocket Server Implementierung

        private WebSocketServer server;
        private LinkedList<UserContext> connections = new LinkedList<UserContext>();

        private void SetupWebSocketServer()
        {
            try
            {
                this.server = new WebSocketServer(8181, IPAddress.Any)
                {
                    OnReceive = OnReceive,
                    //OnSend = OnSend,
                    //OnConnect = OnConnect,
                    OnConnected = OnConnected,
                    OnDisconnect = OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };

                this.server.Start();
                this.LogWindow("WebSocket Server started: 'ws://localhost:8181'.", true);

            }
            catch (Exception excep)
            {
                Console.WriteLine(excep);
            }
        }

        private void OnReceive(UserContext context)
        {
            //echo test
            this.LogWindow("Client Message: " + context.DataFrame.ToString());
        }

        private void OnConnected(UserContext context)
        {
            this.LogWindow("Client connection From: " + context.ClientAddress.ToString());
            this.connections.AddLast(context);
        }

        private void OnDisconnect(UserContext context)
        {
            this.LogWindow("Client disconnected: " + context.ClientAddress.ToString());
            this.connections.Remove(context);
        }

        private void BroadcastMessageToAllClients(String msg)
        {
            foreach(UserContext context in this.connections){
                context.Send(msg);
            }
        }

        #endregion

        #region Logging

        private void LogContactInformation(Microsoft.Surface.Core.TouchPoint tp, String eventType, String msg = "")
        {
            if (this.LoggingActivated)
            {
                String contactMessage = "Unknown";
                if (tp.IsTagRecognized)
                {
                    contactMessage = "Tag(" + tp.Tag.Value + ") " + eventType + " at [" + tp.Bounds.Left + ", " + tp.Bounds.Top + "]~" + tp.Orientation + "; id: " + tp.Id;
                }
                else if (tp.IsFingerRecognized)
                {
                    contactMessage = "Finger " + eventType + " at [" + tp.Bounds.Left + ", " + tp.Bounds.Top + "]~" + tp.Orientation + "; id: " + tp.Id;
                }
                else
                {
                    contactMessage = "Blob " + eventType + " at [" + tp.Bounds.Left + ", " + tp.Bounds.Top + "]~" + tp.Orientation + "; id: " + tp.Id;
                }

                if (msg != "")
                {
                    contactMessage += ("; " + msg);
                }

                this.LogWindow(contactMessage);
            }
        }

        private void LogWindow(String msg, bool clear=false)
        {
            if (this.LoggingActivated)
            {
                //to access gui controls from another thread, the dispatcher must invoke the code
                //in the gui thread.
                //As an action to invoke a lambda expression is used
                Dispatcher.Invoke(() =>
                {
                    if (clear)
                    {
                        this.LogMessages.Document.Blocks.Clear();
                        this.LogMessages.AppendText(msg);
                    }
                    else
                    {
                        this.LogMessages.AppendText("\r" + msg);
                    }
                    this.LogMessages.ScrollToEnd();
                });
            }
        }

        #endregion

        #region WindowButtons, Minimize and Close
        private void WindowButtons_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.Source is Canvas)
            {
                (e.Source as Canvas).Background = Brushes.DodgerBlue;
            }
        }

        private void WindowButtons_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.Source is Canvas)
            {
                (e.Source as Canvas).Background = new SolidColorBrush(Color.FromRgb(91,91,91));
            }
        }

        private void Minimize_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Closing the Program-Window
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket Server closed.");
            Console.WriteLine("Tangible-Hook destructed.");
        }
        /// <summary>
        /// Making the Window draggable with the mouse
        /// </summary>
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        /// <summary>
        /// Touch-Down Handler for the MainWindow, 
        /// delegates the dragging action to the mouse-down event handler
        /// </summary>
        private void Window_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            //delegate to mouse-down event handler, the internal "DragMove" Method of the Window Class doesn't work with touch-events
            this.Window_MouseDown(sender, new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left));
        }

        #endregion
    }
}
