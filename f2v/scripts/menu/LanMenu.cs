using Godot;
using System;

public partial class LanMenu : GridContainer
{
    private TextEdit _ipEdit;
    private TextEdit _portEdit;
    private TextEdit _maxClientsEdit;
    private Button _hostButton;
    private Button _joinButton;

    private ENetMultiplayerPeer peer;

    public override void _Ready()
    {
        _ipEdit = GetNode<TextEdit>("Ip");
        _portEdit = GetNode<TextEdit>("Port");
        _maxClientsEdit = GetNode<TextEdit>("MaxClients");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");

        _hostButton.Pressed += _on_host_pressed;
        _joinButton.Pressed += _on_join_pressed;

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
    }

    private void ConnectionFailed()
    {
        GD.Print("Connection failed");
    }

    private void ConnectedToServer()
    {
        GD.Print("Connected to server");
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Peer disconnected: " + id);
    }

    private void PeerConnected(long id)
    {
        GD.Print("Peer connected: " + id);
    }

    private void _on_host_pressed()
    {
        GD.Print("Host button pressed");
        if (_hostButton.Text == "Stop")
        {
            GD.Print("Stopping server");
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
            _hostButton.Text = "Host";
            return;
        }
        Multiplayer.MultiplayerPeer = null;
        peer = new ENetMultiplayerPeer();
        GD.Print("Creating server with IP: " + _ipEdit.GetText() + " and port: " + _portEdit.GetText() + " and max clients: " + _maxClientsEdit.GetText());
        var err = peer.CreateServer(_portEdit.GetText().ToInt(), _maxClientsEdit.GetText().ToInt());
        if (err != Error.Ok)
        {
            GD.Print("Error creating server: " + err);
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        _hostButton.Text = "Stop";
    }

    private void _on_join_pressed()
    {
        if (_joinButton.Text == "Disconnect")
        {
            GD.Print("Disconnecting from server");
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
            _joinButton.Text = "Join";
            return;
        }
        peer = new ENetMultiplayerPeer();
        peer.CreateClient(_ipEdit.GetText(), _portEdit.GetText().ToInt());

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        _joinButton.Text = "Disconnect";
    }

    [Rpc]
    private void PrintOncePerClient(long id)
    {
        GD.Print("I will be printed to the console once per each connected client.");
    }
}
