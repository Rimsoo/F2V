using Godot;
using System.Linq;
using System;

public partial class LanMenu : GridContainer
{
    [Export] PackedScene GameScene;

    private TextEdit _nameEdit;
    private TextEdit _ipEdit;
    private TextEdit _portEdit;
    private TextEdit _maxClientsEdit;
    private Button _hostButton;
    private Button _joinButton;

    private ENetMultiplayerPeer peer;

    public override void _Ready()
    {
        _ipEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Ip")) as TextEdit;
        _portEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Port")) as TextEdit;
        _maxClientsEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("MaxClients")) as TextEdit;
        _hostButton = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Host")) as Button;
        _joinButton = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Join")) as Button;
        _nameEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Name")) as TextEdit;

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
        RpcId(1, "SendPlayerInformation", _nameEdit.Text, Multiplayer.GetUniqueId());
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

        SendPlayerInformation(_nameEdit.Text, 1);
        Start();
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
        var err = peer.CreateClient(_ipEdit.GetText(), _portEdit.GetText().ToInt());
        if (err != Error.Ok /*|| peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected*/)
        {
            GD.Print("Error creating client: " + err);
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        _joinButton.Text = "Disconnect";
        StartGame();
    }

    public void Start()
    {
        Rpc("StartGame");
        GetParent().GetParent<Menu>().Visible = false;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string name, int id)
    {
        if (GameManager.Players.Find(p => p.Id == id) == null)
        {
            GD.Print("Adding player: " + name + " with id: " + id);
            GameManager.Players.Add(new PlayerInfo()
            {
                Name = name,
                Id = id
            });
        }
        else
        {
            GD.Print("Updating player: " + name + " with id: " + id);
            GameManager.Players.Find(p => p.Id == id).Name = name;
        }

        if (Multiplayer.IsServer())
        {
            foreach (var player in GameManager.Players)
            {
                Rpc("SendPlayerInformation", player.Name, player.Id);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void StartGame()
    {
        CleanupExistingGame();
        var game = GameScene.Instantiate<Game>();
        GetTree().Root.AddChild(game);
    }

    public bool IsGameLoaded()
    {
        return GetTree().Root.GetChildren().OfType<Game>().Count() != 0;
    }

    private void CleanupExistingGame()
    {
        if (!IsGameLoaded())
            return;
        // Récupérer la référence de la game existante
        Game existingGame = GetTree().Root.GetChildren().OfType<Game>().FirstOrDefault();

        // Supprimer proprement l'ancienne instance
        if (existingGame != null)
        {
            existingGame.QueueFree(); // Suppression propre
            GetTree().Root.RemoveChild(existingGame); // Retrait explicite
        }

        // Nettoyer les références
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
